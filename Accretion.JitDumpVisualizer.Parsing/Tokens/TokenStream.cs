using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Deliberate use of statements for ease of future modification.")]
    internal unsafe struct TokenStream
    {
        private static readonly List<object> _gcRoots = new List<object>();

        private readonly char* _end;
        private char* _start;
        private TokenKind _lastTokenKind;

        public TokenStream(StringSegment text)
        {
            var source = text.AsSpan();

            var pinnedBuffer = GC.AllocateArray<char>(source.Length + 1, pinned: true);
            source.CopyTo(pinnedBuffer);
            pinnedBuffer[^1] = '\0';
            _gcRoots.Add(pinnedBuffer);

            _start = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(pinnedBuffer));
            _end = _start + source.Length;
            _lastTokenKind = TokenKind.Unknown;

            Assert.True(source[^1] == *(_end - 1));
            Assert.True(*_end == '\0', "Pinned buffer must be null-terminated.");
        }

        public TokenStream(char* start, nint length)
        {
            _start = start;
            _end = _start + length;
            _lastTokenKind = TokenKind.Unknown;

            Assert.True(*_end is '\0');
        }

        public void Skip(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Next();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Token Next()
        {
            var token = Peek(_start, _end, _lastTokenKind, out var width);
            _start += width;
            if (token.Kind != TokenKind.EndOfLine)
            {
                _lastTokenKind = token.Kind;
            }

            return token;
        }

        public TokenSource? NextTokensBeforeAny(ReadOnlySpan<Token> tokens) => throw new NotImplementedException();

        public TokenSource? NextTokensBefore(ReadOnlySpan<Token> tokens) => throw new NotImplementedException();

        public TokenSource? NextTokensAfter(ReadOnlySpan<Token> tokens) => throw new NotImplementedException();

        public TokenSource NextLine() => throw new NotImplementedException();

        private static Token Peek(char* start, char* end, TokenKind lastToken, out nint finalWidth)
        {
            Assert.True(end - start >= 0, "No peeking outside the bounds.");
            Assert.True(*end is '\0', "The source buffer must be alive.");

            finalWidth = 0;
            while (*start is ' ' or '\t'
#if RELEASE
                or '\r' or '\n'
#endif
                )
            {
                finalWidth++;
                start++;
            }

            TokenKind kind;
            nint rawWidth = 0;
            int rawValue = 0;

#if DEBUG
            if (start[0] is '\n')
            {
                finalWidth += 1;
                return new(TokenKind.EndOfLine);
            }
            else if (start[0] is '\r')
            {
                Assert.Equal(start, "\r\n");
                finalWidth += 2;
                return new(TokenKind.EndOfLine);
            }
#endif

            switch (lastToken)
            {
                // Handling of starting and finsihing phases
                case TokenKind.FifteenStars or TokenKind.InlineStartingAt:
                    switch (start[0])
                    {
                        case 'S':
                            Assert.Equal(start, "Starting PHASE");
                            kind = TokenKind.StartingPhase;
                            rawWidth = "Starting PHASE ".Length;
                            goto ParsePhase;
                        case 'F':
                            Assert.Equal(start, "Finishing PHASE");
                            kind = TokenKind.FinishingPhase;
                            rawWidth = "Finishing PHASE ".Length;
                        ParsePhase:
                            var phase = ParseRyuJitPhase(start + rawWidth, out var phaseWidth);
                            rawWidth += phaseWidth;
                            rawValue = (int)phase;
                            break;
                        case 'I':
                            switch (start[2])
                            {
                                case ' ':
                                    Assert.Equal(start, "In");
                                    rawWidth = "In ".Length;
                                    var function = ParseRyuJitFunction(start + rawWidth, out var functionWidth);
                                    rawWidth += functionWidth;
                                    (kind, rawValue) = (TokenKind.StartingFunction, (int)function);
                                    break;
                                default:
                                    switch (start[7])
                                    {
                                        case '@':
                                            Assert.FormatEqual(start, "Inline @[000000]");
                                            rawWidth = "Inline @[".Length;
                                            var inlineStart = IntegersParser.ParseIntegerSixDigits(start + rawWidth);
                                            rawWidth += "000000]".Length;
                                            (kind, rawValue) = (TokenKind.InlineStartingAt, inlineStart);
                                            break;
                                        case 'T':
                                            Assert.Equal(start, "Inline Tree");
                                            rawWidth = "Inline Tree".Length;
                                            kind = TokenKind.InlineTreeHeader;
                                            break;
                                        default: Assert.Impossible(start); goto ReturnUnknown;
                                    }
                                    break;
                            }
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;

                case TokenKind.FinishingPhase:
                    switch (start[0])
                    {
                        case '[':
                            Assert.Equal(start, "[no changes]");
                            kind = TokenKind.NoChangesInPhase;
                            rawWidth = "[no changes]".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;

                #region Handling of basic block table
                case TokenKind.BasicBlockTableHeader:
                    Assert.Equal(start, "BBnum");
                    kind = TokenKind.BasicBlockNumberColumnHeader;
                    rawWidth = "BBnum".Length;
                    break;

                case TokenKind.BasicBlockNumberColumnHeader:
                    switch (start[0])
                    {
                        case 'B':
                            Assert.Equal(start, "BBid");
                            (kind, rawWidth) = (TokenKind.BasicBlockIdColumnHeader, "BBid".Length);
                            break;
                        case 'R':
                            Assert.Equal(start, "Reachable by");
                            (kind, rawWidth) = (TokenKind.ReachabilitySetsHeader, "Reachable by".Length);
                            break;
                        case 'D':
                            Assert.Equal(start, "Dominated by");
                            (kind, rawWidth) = (TokenKind.DominatorSetsHeader, "Dominated by".Length);
                            break;
                        default: Assert.Impossible(start); goto ReturnUnknown;
                    }
                    break;

                case TokenKind.BasicBlockIdColumnHeader:
                    Assert.Equal(start, "ref");
                    (kind, rawWidth) = (TokenKind.RefColumnHeader, "ref".Length);
                    break;

                case TokenKind.RefColumnHeader:
                    Assert.Equal(start, "try");
                    (kind, rawWidth) = (TokenKind.TryColumnHeader, "try".Length);
                    break;

                case TokenKind.TryColumnHeader:
                    Assert.Equal(start, "hnd");
                    (kind, rawWidth) = (TokenKind.HandleColumnHeader, "hnd".Length);
                    break;

                case TokenKind.HandleColumnHeader:
                    // We normalize the table by adding missing columns
                    kind = TokenKind.PredsColumnHeader;
                    switch (start[0])
                    {
                        case 'p':
                            Assert.Equal(start, "preds");
                            rawWidth = "preds".Length;
                            break;
                        default:
                            rawWidth = 0;
                            break;
                    }
                    break;

                case TokenKind.PredsColumnHeader:
                    Assert.Equal(start, "weight");
                    (kind, rawWidth) = (TokenKind.WeightColumnHeader, "weight".Length);
                    break;

                case TokenKind.WeightColumnHeader:
                    Assert.Equal(start, "lp [IL range]");
                    (kind, rawWidth) = (TokenKind.ILRangeColumnHeader, "lp [IL range]".Length);
                    break;

                case TokenKind.ILRangeColumnHeader:
                    Assert.Equal(start, "[jump]");
                    (kind, rawWidth) = (TokenKind.JumpColumnHeader, "[jump]".Length);
                    break;

                case TokenKind.JumpColumnHeader:
                    Assert.Equal(start, "[EH region]");
                    (kind, rawWidth) = (TokenKind.ExceptionHandlingColumnHeader, "[EH region]".Length);
                    break;

                case TokenKind.ExceptionHandlingColumnHeader:
                    Assert.Equal(start, "[flags]");
                    (kind, rawWidth) = (TokenKind.FlagsColumnHeader, "[flags]".Length);
                    break;

                case TokenKind.FlagsColumnHeader:
                    Assert.Equal(start, Token.OneHundredAndThirySevenDashes);
                    kind = TokenKind.BasicBlockTableCenter;
                    rawWidth = Token.OneHundredAndThirySevenDashes.Length;
                    break;

                case TokenKind.BasicBlockTableCenter:
                ReturnBasicBlockInTable:
                    kind = TokenKind.BasicBlockInTable;
                    goto ReturnAnyBasicBlock;

                case TokenKind.BasicBlockInTable:
                    kind = TokenKind.BasicBlockIdInTable;
                    rawWidth = "[0000]".Length;
                    rawValue = IntegersParser.ParseIntegerFourDigits(start + "[".Length);
                    break;

                case TokenKind.BasicBlockIdInTable:
                    kind = TokenKind.BasicBlockRefCountInTable;
                    rawValue = IntegersParser.ParseGenericInteger(start, out var refCountWidth);
                    rawWidth = refCountWidth;
                    break;

                case TokenKind.BasicBlockRefCountInTable:
                    kind = TokenKind.BasicBlockTryCountInTable;
                    rawWidth = 0;
                    break;

                case TokenKind.BasicBlockTryCountInTable:
                    kind = TokenKind.BasicBlockHandleCountInTable;
                    rawWidth = 0;
                    break;

                case TokenKind.BasicBlockHandleCountInTable:
                    switch (start[0])
                    {
                        case 'B':
                            kind = TokenKind.BasicBlockPredInTable;
                            goto ReturnAnyBasicBlock;
                        default: goto ReturnBasicBlockWeightInTable;
                    }

                case TokenKind.BasicBlockPredInTable:
                    switch (start[0])
                    {
                        case ',':
                            start++;
                            rawWidth++;
                            kind = TokenKind.BasicBlockPredInTable;
                            goto ReturnAnyBasicBlock;
                        default: goto ReturnBasicBlockWeightInTable;
                    }

                ReturnBasicBlockWeightInTable:
                    kind = TokenKind.BasicBlockWeightInTable;
                    rawValue = BitConverter.SingleToInt32Bits(IntegersParser.ParseGenericFloat(start, out var weightWidth));
                    rawWidth = weightWidth;
                    break;

                case TokenKind.BasicBlockWeightInTable:
                    kind = TokenKind.BasicBlockILRangeStartInTable;
                    goto ReturnTwoDotILRangeStart;

                case TokenKind.BasicBlockILRangeStartInTable:
                    kind = TokenKind.BasicBlockILRangeEndInTable;
                    goto ReturnTwoDotILRangeEnd;

                case TokenKind.BasicBlockILRangeEndInTable:
                    switch (start[0])
                    {
                        case '-' when start[1] is '>':
                            kind = TokenKind.BasicBlockJumpTargetInTable;
                            goto ReturnAnyBasicBlockJumpTarget;
                        case '(':
                            kind = TokenKind.BasicBlockJumpTargetKindInTable;
                            goto ReturnAnyBasicBlockJumpTargetKind;
                        default: goto FinishBasicBlockRowInTable;
                    }

                case TokenKind.BasicBlockJumpTargetInTable:
                    kind = TokenKind.BasicBlockJumpTargetKindInTable;
                    goto ReturnAnyBasicBlockJumpTargetKind;

                case TokenKind.BasicBlockFlagInTable or TokenKind.BasicBlockJumpTargetKindInTable:
                FinishBasicBlockRowInTable:
                    switch (start[0])
                    {
                        case 'B': goto ReturnBasicBlockInTable;
                        case '-':
                            Assert.Equal(start, Token.OneHundredAndThirySevenDashes);
                            kind = TokenKind.BasicBlockTableFooter;
                            rawWidth = Token.OneHundredAndThirySevenDashes.Length;
                            break;
                        default:
                            kind = TokenKind.BasicBlockFlagInTable;
                            rawValue = (int)ParseBasicBlockFlag(start, out var flagWidth);
                            rawWidth = flagWidth;
                            break;
                    }
                    break;
                #endregion

                #region Handling of basic block detalization header
                case TokenKind.TwelveDashes:
                    switch (start[0])
                    {
                        case 'B':
                            kind = TokenKind.BasicBlockInTopHeader;
                            goto ReturnAnyBasicBlock;
                        default: goto ReturnUnknown;
                    }

                case TokenKind.BasicBlockInTopHeader:
                    kind = TokenKind.BasicBlockILRangeStartInTopHeader;
                    goto ReturnTwoDotILRangeStart;

                case TokenKind.BasicBlockILRangeStartInTopHeader:
                    kind = TokenKind.BasicBlockILRangeEndInTopHeader;
                    goto ReturnTwoDotILRangeEnd;

                case TokenKind.BasicBlockILRangeEndInTopHeader:
                    switch (start[0])
                    {
                        case '-':
                            kind = TokenKind.BasicBlockJumpTargetInTopHeader;
                            goto ReturnAnyBasicBlockJumpTarget;
                        case ',': goto ReturnBasicBlockPredInTopHeader;
                        case '(':
                            kind = TokenKind.BasicBlockJumpTargetKindInTopHeader;
                            goto ReturnAnyBasicBlockJumpTargetKind;
                        default: Assert.Impossible(start); goto ReturnUnknown;
                    }

                case TokenKind.BasicBlockJumpTargetInTopHeader:
                    kind = TokenKind.BasicBlockJumpTargetKindInTopHeader;
                    goto ReturnAnyBasicBlockJumpTargetKind;

                case TokenKind.BasicBlockPredInTopHeader or TokenKind.BasicBlockJumpTargetKindInTopHeader:
                ReturnBasicBlockPredInTopHeader:
                    switch (start[0])
                    {
                        case ',':
                            if (start[1] != 'B')
                            {
                                Assert.Equal(start, ", preds={");
                                start += ", preds={".Length;
                                rawWidth += ", preds={".Length;
                                goto ReturnBasicBlockPredInTopHeader;
                            }
                            Assert.FormatEqual(start, ",BB00");
                            start++;
                            rawWidth++;
                            goto case 'B';
                        case 'B':
                            kind = TokenKind.BasicBlockPredInTopHeader;
                            goto ReturnAnyBasicBlock;
                        default:
                            Assert.Equal(start, "} succs={");
                            start += "} succs={".Length;
                            rawWidth += "} succs={".Length;
                            goto ReturnBasicBlockSuccInTopHeader;
                    }

                case TokenKind.BasicBlockSuccInTopHeader:
                ReturnBasicBlockSuccInTopHeader:
                    switch (start[0])
                    {
                        case ',':
                            start++;
                            rawWidth++;
                            goto case 'B';
                        case 'B':
                            kind = TokenKind.BasicBlockSuccInTopHeader;
                            goto ReturnAnyBasicBlock;
                        default: goto ReturnUnknown;
                    }
                #endregion

                ReturnTwoDotILRangeStart:
                    Assert.FormatEqual(start, "[000", hex: true, valid: '?');
                    rawValue = ParseILRange(start + "[".Length);
                    rawWidth = "[000".Length;
                    break;

                ReturnTwoDotILRangeEnd:
                    Assert.FormatEqual(start, "..000)", hex: true, valid: '?');
                    rawValue = ParseILRange(start + "..".Length);
                    rawWidth = "..000)".Length;
                    break;

                ReturnAnyBasicBlockJumpTarget:
                    Assert.FormatEqual(start, "-> BB00");
                    start += "-> ".Length;
                    rawWidth += "-> ".Length;
                    goto ReturnAnyBasicBlock;

                ReturnAnyBasicBlockJumpTargetKind:
                    rawValue = (int)ParseBasicBlockJumpTargetKind(start, out var jumpKindWidth);
                    rawWidth = jumpKindWidth;
                    break;

                #region Handling of basic block detalization
                case TokenKind.FourStars:
                    kind = TokenKind.BasicBlockInInnerHeader;
                    goto ReturnAnyBasicBlock;

                case TokenKind.BasicBlockInInnerHeader:
                    switch (start[0])
                    {
                        case 'S':
                            Assert.FormatEqual(start, "STMT00000");
                            kind = TokenKind.Statement;
                            rawWidth = "STMT00000".Length;
                            rawValue = IntegersParser.ParseIntegerFiveDigits(start + "STMT".Length);
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;

                case TokenKind.Statement:
                    Assert.Equal(start, "(IL");
                    kind = TokenKind.StatementILRangeStart;
                    rawValue = ParseILRange(start + "(IL 0x".Length);
                    rawWidth = "(IL 0x000".Length;
                    break;

                case TokenKind.StatementILRangeStart:
                    Assert.Equal(start, "...");
                    kind = TokenKind.StatementILRangeEnd;
                    rawValue = ParseILRange(start + "...0x".Length);
                    rawWidth = "...0x000)".Length;
                    break;

                case TokenKind.StatementILRangeEnd:
                    switch (start[0])
                    {
                        case 'N':
                            Assert.FormatEqual(start, "N000");
                            kind = TokenKind.Node;
                            rawValue = IntegersParser.ParseIntegerThreeDigits(start + "N".Length);
                            rawWidth = "N000".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;

                case TokenKind.Node:
                    Assert.FormatEqual(start, "(000,", valid: ' ');
                    kind = TokenKind.NodeLeftValue;
                    rawValue = IntegersParser.ParseGenericInteger(start + "(".Length, out _);
                    rawWidth = "(000,".Length;
                    break;

                case TokenKind.NodeLeftValue:
                    kind = TokenKind.NodeRightValue;
                    rawValue = IntegersParser.ParseGenericInteger(start, out var leftValueWidth);
                    rawWidth = leftValueWidth + ")".Length;
                    break;

                case TokenKind.NodeRightValue:
                    Assert.FormatEqual(start, "[000000]");
                    kind = TokenKind.NodeValue;
                    rawValue = IntegersParser.ParseIntegerSixDigits(start + "[".Length);
                    rawWidth = "[000000]".Length;
                    break;

                case TokenKind.NodeValue:
                    kind = TokenKind.NodeFlags;
                    rawValue = (int)ParseNodeFlags(start);
                    rawWidth = "------------".Length;
                    break;
                #endregion

                #region Handling of unstructured data
                default:
                    switch (start[0])
                    {
                        case '\0': (kind, rawWidth) = (TokenKind.EndOfFile, 0); break;
                        case '[': (kind, rawWidth) = (TokenKind.OpenBracket, 1); break;
                        case ']': (kind, rawWidth) = (TokenKind.CloseBracket, 1); break;
                        case '{': (kind, rawWidth) = (TokenKind.OpenCurly, 1); break;
                        case '}': (kind, rawWidth) = (TokenKind.CloseCurly, 1); break;
                        case '(': (kind, rawWidth) = (TokenKind.OpenParen, 1); break;
                        case ')': (kind, rawWidth) = (TokenKind.CloseParen, 1); break;
                        case '<': (kind, rawWidth) = (TokenKind.LessThan, 1); break;
                        case '>': (kind, rawWidth) = (TokenKind.GreaterThan, 1); break;
                        case ':': (kind, rawWidth) = (TokenKind.Colon, 1); break;
                        case ';': (kind, rawWidth) = (TokenKind.Semicolon, 1); break;
                        case '=': (kind, rawWidth) = (TokenKind.EqualsSign, 1); break;
                        case '\'': (kind, rawWidth) = (TokenKind.SingleQuote, 1); break;
                        case '|': (kind, rawWidth) = (TokenKind.Pipe, 1); break;
                        case '"': (kind, rawWidth) = (TokenKind.DoubleQuote, 1); break;
                        case ',': (kind, rawWidth) = (TokenKind.Comma, 1); break;
                        case '#': (kind, rawWidth) = (TokenKind.Hash, 1); break;
                        case '?': (kind, rawWidth) = (TokenKind.QuestionMark, 1); break;
                        case '.':
                            switch (Count.OfLeading(start, end, '.'))
                            {
                                case 3: (kind, rawWidth) = (TokenKind.ThreeDots, 3); break;
                                case 2: (kind, rawWidth) = (TokenKind.TwoDots, 2); break;
                                case 1: (kind, rawWidth) = (TokenKind.Dot, 1); break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case '*':
                            switch (Count.OfLeading(start, end, '*'))
                            {
                                case 75: (kind, rawWidth) = (TokenKind.SeventyFiveStars, 75); break;
                                case 15: (kind, rawWidth) = (TokenKind.FifteenStars, 15); break;
                                case 4: (kind, rawWidth) = (TokenKind.FourStars, 4); break;
                                case 6: (kind, rawWidth) = (TokenKind.SixStars, 6); break;
                                case 1: (kind, rawWidth) = (TokenKind.Star, 1); break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case '-':
                            switch (Count.OfLeading(start, end, '-'))
                            {
                                case 137: (kind, rawWidth) = (TokenKind.BasicBlockTableHeader, 137); break;
                                case 48: (kind, rawWidth) = (TokenKind.FourtyEightDashes, 48); break;
                                case 17: (kind, rawWidth) = (TokenKind.SeventeenDashes, 17); break;
                                case 12: (kind, rawWidth) = (TokenKind.TwelveDashes, 12); break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        // All integers in the dump start with characters 0 through 9
                        case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                            kind = TokenKind.Integer;
                            rawValue = IntegersParser.ParseGenericInteger(start, out rawWidth);
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                #endregion

                ReturnAnyBasicBlock:
                    Assert.FormatEqual(start, "BB00");
                    rawValue = IntegersParser.ParseIntegerTwoDigits(start + "BB".Length);
                    rawWidth += "BB00".Length;
                    break;
                ReturnUnknown:
                    (kind, rawWidth) = (TokenKind.Unknown, 1); break;
            }

            finalWidth += rawWidth;
            return new(kind, (uint)rawValue);
        }

        private static RyuJitPhase ParseRyuJitPhase(char* start, out nint width)
        {
            RyuJitPhase phase;
            switch (start[0])
            {
                case 'A':
                    switch (start[1])
                    {
                        case 'l': (phase, width) = (RyuJitPhase.AllocateObjects, "Allocate Objects".Length); break;
                        default: (phase, width) = (RyuJitPhase.AssertionProp, "Assertion prop".Length); break;
                    }
                    break;
                case 'B': (phase, width) = (RyuJitPhase.BuildSSARepresentation, "Build SSA representation".Length); break;
                case 'C':
                    switch (start[1])
                    {
                        case 'a': (phase, width) = (RyuJitPhase.CalculateStackLevelSlots, "Calculate stack level slots".Length); break;
                        case 'l':
                            switch (start[6])
                            {
                                case 'f': (phase, width) = (RyuJitPhase.CloneFinally, "Clone finally".Length); break;
                                default: (phase, width) = (RyuJitPhase.CloneLoops, "Clone loops".Length); break;
                            }
                            break;
                        case 'o':
                            switch (start[8])
                            {
                                case 'b': (phase, width) = (RyuJitPhase.ComputeBlocksReachability, "Compute blocks reachability".Length); break;
                                case 'e': (phase, width) = (RyuJitPhase.ComputeEdgeWeights, "Compute edge weights".Length); break;
                                default: (phase, width) = (RyuJitPhase.ComputePreds, "Compute preds".Length); break;
                            }
                            break;
                        default: (phase, width) = (RyuJitPhase.CreateEHFunclets, "Create EH funclets".Length); break;
                    }
                    break;
                case 'D':
                    switch (start[1])
                    {
                        case 'e': (phase, width) = (RyuJitPhase.DetermineFirstColdBlock, "Determine first cold block".Length); break;
                        default:
                            switch (start[3])
                            {
                                case '\'': (phase, width) = (RyuJitPhase.DoSimpleLowering, "Do 'simple' lowering".Length); break;
                                default: (phase, width) = (RyuJitPhase.DoValueNumbering, "Do value numbering".Length); break;
                            }
                            break;
                    }
                    break;
                case 'E':
                    switch (start[1])
                    {
                        case 'a': (phase, width) = (RyuJitPhase.EarlyValuePropagation, "Early Value Propagation".Length); break;
                        case 'x': (phase, width) = (RyuJitPhase.ExpandPatchpoints, "Expand patchpoints".Length); break;
                        default:
                            switch (start[5])
                            {
                                case 'c': (phase, width) = (RyuJitPhase.EmitCode, "Emit code".Length); break;
                                default: (phase, width) = (RyuJitPhase.EmitGCPlusEHTables, "Emit GC+EH tables".Length); break;
                            }
                            break;
                    }
                    break;
                case 'F': (phase, width) = (RyuJitPhase.FindOperOrder, "Find oper order".Length); break;
                case 'G':
                    switch (start[1])
                    {
                        case 'e': (phase, width) = (RyuJitPhase.GenerateCode, "Generate code".Length); break;
                        default: (phase, width) = (RyuJitPhase.GSCookie, "GS Cookie".Length); break;
                    }
                    break;
                case 'H': (phase, width) = (RyuJitPhase.HoistLoopCode, "Hoist loop code".Length); break;
                case 'I':
                    switch (start[1])
                    {
                        case 'm': (phase, width) = (RyuJitPhase.Importation, "Importation".Length); break;
                        default:
                            switch (start[2])
                            {
                                case 'd': (phase, width) = (RyuJitPhase.IndirectCallTransform, "Indirect call transform".Length); break;
                                default: (phase, width) = (RyuJitPhase.InsertGCPolls, "Insert GC Polls".Length); break;
                            }
                            break;
                    }
                    break;
                case 'L':
                    switch (start[1])
                    {
                        case 'i': (phase, width) = (RyuJitPhase.LinearScanRegisterAlloc, "Linear scan register alloc".Length); break;
                        default: (phase, width) = (RyuJitPhase.LoweringNodeInfo, "Lowering nodeinfo".Length); break;
                    }
                    break;
                case 'M':
                    switch (start[1])
                    {
                        case 'a': (phase, width) = (RyuJitPhase.MarkLocalVars, "Mark local vars".Length); break;
                        case 'e':
                            switch (start[6])
                            {
                                case 'c': (phase, width) = (RyuJitPhase.MergeCallfinallyChains, "Merge callfinally chains".Length); break;
                                default: (phase, width) = (RyuJitPhase.MergeThrowBlocks, "Merge throw blocks".Length); break;
                            }
                            break;
                        default:
                            switch (start[8])
                            {
                                case 'A': (phase, width) = (RyuJitPhase.MorphAddInternalBlocks, "Morph - Add internal blocks".Length); break;
                                case 'B': (phase, width) = (RyuJitPhase.MorphByRefs, "Morph - ByRefs".Length); break;
                                case 'G': (phase, width) = (RyuJitPhase.MorphGlobal, "Morph - Global".Length); break;
                                case 'I':
                                    switch (start[10])
                                    {
                                        case 'i': (phase, width) = (RyuJitPhase.MorphInit, "Morph - Init".Length); break;
                                        default: (phase, width) = (RyuJitPhase.MorphInlining, "Morph - Inlining".Length); break;
                                    }
                                    break;
                                case 'P': (phase, width) = (RyuJitPhase.MorphPromoteStructs, "Morph - Promote Structs".Length); break;
                                default: (phase, width) = (RyuJitPhase.MorphStructsAddressExposed, "Morph - Structs/AddrExp".Length); break;
                            }
                            break;
                    }
                    break;
                case 'O':
                    switch (start[9])
                    {
                        case 'b': (phase, width) = (RyuJitPhase.OptimizeBools, "Optimize bools".Length); break;
                        case 'i': (phase, width) = (RyuJitPhase.OptimizeIndexChecks, "Optimize index checks".Length); break;
                        case 'l':
                            switch (start[10])
                            {
                                case 'a': (phase, width) = (RyuJitPhase.OptimizeLayout, "Optimize layout".Length); break;
                                default: (phase, width) = (RyuJitPhase.OptimizeLoops, "Optimize loops".Length); break;
                            }
                            break;
                        default: (phase, width) = (RyuJitPhase.OptimizeValnumCSEs, "Optimize Valnum CSEs".Length); break;
                    }
                    break;
                case 'P':
                    switch (start[1])
                    {
                        case 'o': (phase, width) = (RyuJitPhase.PostImport, "Post-import".Length); break;
                        default: (phase, width) = (RyuJitPhase.PreImport, "Pre-import".Length); break;
                    }
                    break;
                case 'R':
                    switch (start[1])
                    {
                        case 'a': (phase, width) = (RyuJitPhase.RationalizeIR, "Rationalize IR".Length); break;
                        default:
                            switch (start[13])
                            {
                                case 'f': (phase, width) = (RyuJitPhase.RemoveEmptyFinally, "Remove empty finally".Length); break;
                                default: (phase, width) = (RyuJitPhase.RemoveEmptyTry, "Remove empty try".Length); break;
                            }
                            break;
                    }
                    break;
                case 'S': (phase, width) = (RyuJitPhase.SetBlockOrder, "Set block order".Length); break;
                case 'U':
                    switch (start[1])
                    {
                        case 'n': (phase, width) = (RyuJitPhase.UnrollLoops, "Unroll loops".Length); break;
                        default: (phase, width) = (RyuJitPhase.UpdateFlowGraphEarlyPass, "Update flow graph early pass".Length); break;
                    }
                    break;
                default: (phase, width) = (RyuJitPhase.VNBasedCopyPropagation, "VN based copy prop".Length); break; ;
            }

            return phase;
        }

        private static RyuJitFunction ParseRyuJitFunction(char* start, out nint width)
        {
            width = 0;
            var function = RyuJitFunction.Unknown;
            switch (*start)
            {
                case 'c': (function, width) = (RyuJitFunction.CompInitDebuggingInfo, "compInitDebuggingInfo()".Length); break;
                case 'e':
                    switch (start[4])
                    {
                        case 'E': (function, width) = (RyuJitFunction.EmitEndCodeGen, "emitEndCodeGen()".Length); break;
                        default: (function, width) = (RyuJitFunction.EmitJumpDistBind, "emitJumpDistBind()".Length); break;
                    }
                    break;
                case 'f':
                    switch (start[2])
                    {
                        case 'C':
                            switch (start[3])
                            {
                                case 'o':
                                    switch (start[9])
                                    {
                                        case 'B': (function, width) = (RyuJitFunction.FgComputeBlockAndEdgeWeights, "fgComputeBlockAndEdgeWeights()".Length); break;
                                        case 'D': (function, width) = (RyuJitFunction.FgComputeDoms, "fgComputeDoms".Length); break;
                                        case 'P': (function, width) = (RyuJitFunction.FgComputePreds, "fgComputePreds()".Length); break;
                                        default: (function, width) = (RyuJitFunction.FgComputeReachability, "fgComputeReachability".Length); break;
                                    }
                                    break;
                                default: (function, width) = (RyuJitFunction.FgCreateFunclets, "fgCreateFunclets()".Length); break; ;
                            }
                            break;
                        case 'D':
                            switch (start[4])
                            {
                                case 'b': (function, width) = (RyuJitFunction.FgDebugCheckBBlist, "fgDebugCheckBBlist".Length); break;
                                default: (function, width) = (RyuJitFunction.FgDetermineFirstColdBlock, "fgDetermineFirstColdBlock()".Length); break;
                            }
                            break;
                        case 'E': (function, width) = (RyuJitFunction.FgExpandRarelyRunBlocks, "fgExpandRarelyRunBlocks()".Length); break;
                        case 'F':
                            switch (start[6])
                            {
                                case 'B': (function, width) = (RyuJitFunction.FgFindBasicBlocks, "fgFindBasicBlocks()".Length); break;
                                default: (function, width) = (RyuJitFunction.FgFindOperOrder, "fgFindOperOrder()".Length); break;
                            }
                            break;
                        case 'I': (function, width) = (RyuJitFunction.FgInterBlockLocalVarLiveness, "fgInterBlockLocalVarLiveness()".Length); break;
                        case 'L': (function, width) = (RyuJitFunction.FgLocalVarLiveness, "fgLocalVarLiveness()".Length); break;
                        case 'M':
                            switch (start[3])
                            {
                                case 'a': (function, width) = (RyuJitFunction.FgMarkAddressExposedLocals, "fgMarkAddressExposedLocals()".Length); break;
                                default: (function, width) = (RyuJitFunction.FgMorphBlocks, "fgMorphBlocks()".Length); break;
                            }
                            break;
                        case 'P':
                            switch (start[3])
                            {
                                case 'e': (function, width) = (RyuJitFunction.FgPerBlockLocalVarLiveness, "fgPerBlockLocalVarLiveness()".Length); break;
                                default: (function, width) = (RyuJitFunction.FgPromoteStructs, "fgPromoteStructs()".Length); break;
                            }
                            break;
                        case 'R':
                            switch (start[4])
                            {
                                case 'm':
                                    switch (start[13])
                                    {
                                        case 'B': (function, width) = (RyuJitFunction.FgRemoveEmptyBlocks, "fgRemoveEmptyBlocks".Length); break;
                                        default: (function, width) = (RyuJitFunction.FgRemoveEmptyTry, "fgRemoveEmptyTry()".Length); break;
                                    }
                                    break;
                                case 'o': (function, width) = (RyuJitFunction.FgReorderBlocks, "fgReorderBlocks()".Length); break;
                                case 's': (function, width) = (RyuJitFunction.FgResetImplicitByRefRefCount, "fgResetImplicitByRefRefCount()".Length); break;
                                default: (function, width) = (RyuJitFunction.FgRetypeImplicitByRefArgs, "fgRetypeImplicitByRefArgs()".Length); break;
                            }
                            break;
                        case 'S': (function, width) = (RyuJitFunction.FgSetBlockOrder, "fgSetBlockOrder()".Length); break;
                        case 'T': (function, width) = (RyuJitFunction.FgTailMergeThrows, "fgTailMergeThrows".Length); break;
                        case 'U': (function, width) = (RyuJitFunction.FgUpdateFlowGraph, "fgUpdateFlowGraph()".Length); break;
                        default: (function, width) = (RyuJitFunction.FgValueNumber, "fgValueNumber()".Length); break;
                    }
                    break;
                case 'g':
                    switch (start[1])
                    {
                        case 'c': (function, width) = (RyuJitFunction.GcInfoBlockHdrSave, "gcInfoBlockHdrSave()".Length); break;
                        default:
                            switch (start[3])
                            {
                                case 'E': (function, width) = (RyuJitFunction.GenEnregisterIncomingStackArgs, "genEnregisterIncomingStackArgs()".Length); break;
                                case 'F':
                                    switch (start[5])
                                    {
                                        case 'E': (function, width) = (RyuJitFunction.GenFnEpilog, "genFnEpilog()".Length); break;
                                        default:
                                            switch (start[11])
                                            {
                                                case '(': (function, width) = (RyuJitFunction.GenFnProlog, "genFnProlog()".Length); break;
                                                default: (function, width) = (RyuJitFunction.GenFnPrologCalleeRegArgs, "genFnPrologCalleeRegArgs()".Length); break;
                                            }
                                            break;
                                    }
                                    break;
                                case 'G': (function, width) = (RyuJitFunction.GenGenerateCode, "genGenerateCode()".Length); break;
                                case 'I': (function, width) = (RyuJitFunction.GenIPmappingGen, "genIPmappingGen()".Length); break;
                                default: (function, width) = (RyuJitFunction.GenSetScopeInfo, "genSetScopeInfo()".Length); break;
                            }
                            break;
                    }
                    break;
                case 'i': (function, width) = (RyuJitFunction.ImpImport, "impImport()".Length); break;
                case 'L': (function, width) = (RyuJitFunction.LinearScan_allocateRegisters, "LinearScan::allocateRegisters()".Length); break;
                case 'l':
                    switch (start[3])
                    {
                        case 'A': (function, width) = (RyuJitFunction.LvaAssignFrameOffsets, "lvaAssignFrameOffsets(FINAL_FRAME_LAYOUT)".Length); break;
                        default: (function, width) = (RyuJitFunction.LvaMarkLocalVars, "lvaMarkLocalVars()".Length); break;
                    }
                    break;
                case 'o':
                    switch (start[3])
                    {
                        case 'A':
                            switch (start[4])
                            {
                                case 'd': (function, width) = (RyuJitFunction.OptAddCopies, "optAddCopies()".Length); break;
                                default: (function, width) = (RyuJitFunction.OptAssertionPropMain, "optAssertionPropMain()".Length); break;
                            }
                            break;
                        case 'C': (function, width) = (RyuJitFunction.OptCloneLoops, "optCloneLoops()".Length); break;
                        case 'E': (function, width) = (RyuJitFunction.OptEarlyProp, "optEarlyProp()".Length); break;
                        case 'O':
                            switch (start[11])
                            {
                                case 'B': (function, width) = (RyuJitFunction.OptOptimizeBools, "optOptimizeBools()".Length); break;
                                case 'C': (function, width) = (RyuJitFunction.OptOptimizeCSEs, "optOptimizeCSEs()".Length); break;
                                case 'L':
                                    switch (start[12])
                                    {
                                        case 'a': (function, width) = (RyuJitFunction.OptOptimizeLayout, "optOptimizeLayout()".Length); break;
                                        default: (function, width) = (RyuJitFunction.OptOptimizeLoops, "optOptimizeLoops()".Length); break;
                                    }
                                    break;
                                default: (function, width) = (RyuJitFunction.OptOptimizeValnumCSEs, "optOptimizeValnumCSEs()".Length); break;
                            }
                            break;
                        case 'R': (function, width) = (RyuJitFunction.OptRemoveRedundantZeroInits, "optRemoveRedundantZeroInits()".Length); break;
                        default: (function, width) = (RyuJitFunction.OptVnCopyProp, "optVnCopyProp()".Length); break;
                    }
                    break;
                case 'O': (function, width) = (RyuJitFunction.OptimizeRangeChecks, "OptimizeRangeChecks()".Length); break;
                default:
                    switch (start[12])
                    {
                        case 'B': (function, width) = (RyuJitFunction.SsaBuilder_Build, "SsaBuilder::Build()".Length); break;
                        case 'I': (function, width) = (RyuJitFunction.SsaBuilder_InsertPhiFunctions, "SsaBuilder::InsertPhiFunctions()".Length); break;
                        default: (function, width) = (RyuJitFunction.SsaBuilder_RenameVariables, "SsaBuilder::RenameVariables()".Length); break;
                    }
                    break;
            }

            return function;
        }

        private static BasicBlockJumpTargetKind ParseBasicBlockJumpTargetKind(char* start, out nint width)
        {
            BasicBlockJumpTargetKind kind;
            switch (start[2])
            {
                case 'c':
                    Assert.Equal(start, "( cond )");
                    kind = BasicBlockJumpTargetKind.Conditional;
                    width = "( cond )".Length;
                    break;
                case 'l':
                    Assert.Equal(start, "(always)");
                    kind = BasicBlockJumpTargetKind.Always;
                    width = "(always)".Length;
                    break;
                case 'e':
                    Assert.Equal(start, "(return)");
                    kind = BasicBlockJumpTargetKind.Return;
                    width = "(return)".Length;
                    break;
                case 'o':
                    Assert.Equal(start, "(cond)");
                    kind = BasicBlockJumpTargetKind.Conditional;
                    width = "(cond)".Length;
                    break;
                default:
                    Assert.Impossible(start);
                    kind = BasicBlockJumpTargetKind.Unknown;
                    width = 1;
                    break;
            }

            return kind;
        }

        private static BasicBlockFlag ParseBasicBlockFlag(char* start, out nint width)
        {
            BasicBlockFlag flag;
            switch (*start)
            {
                case 'i':
                    switch (start[1])
                    {
                        case 'd':
                            Assert.Equal(start, "idxlen");
                            flag = BasicBlockFlag.IdxLen;
                            width = "idxlen".Length;
                            break;
                        case 'n':
                            Assert.Equal(start, "internal");
                            flag = BasicBlockFlag.Internal;
                            width = "internal".Length;
                            break;
                        case ' ':
                            Assert.Equal(start, "i");
                            flag = BasicBlockFlag.I;
                            width = "i".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'l':
                    Assert.Equal(start, "label");
                    flag = BasicBlockFlag.Label;
                    width = "label".Length;
                    break;
                case 't':
                    Assert.Equal(start, "target");
                    flag = BasicBlockFlag.Target;
                    width = "target".Length;
                    break;
                case 'h':
                    Assert.Equal(start, "hascall");
                    flag = BasicBlockFlag.HasCall;
                    width = "hascall".Length;
                    break;
                case 'n':
                    switch (start[1])
                    {
                        case 'e':
                            Assert.Equal(start, "newobj");
                            flag = BasicBlockFlag.NewObj;
                            width = "newobj".Length;
                            break;
                        case 'u':
                            Assert.Equal(start, "nullcheck");
                            flag = BasicBlockFlag.NullCheck;
                            width = "nullcheck".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'g':
                    Assert.Equal(start, "gcsafe");
                    flag = BasicBlockFlag.GCSafe;
                    width = "gcsafe".Length;
                    break;
                case 'L':
                    Assert.Equal(start, "LIR");
                    flag = BasicBlockFlag.LIR;
                    width = "LIR".Length;
                    break;
                default:
                ReturnUnknown:
                    Assert.Impossible(start);
                    flag = BasicBlockFlag.Unknown;
                    width = 1;
                    break;
            }

            return flag;
        }

        private static int ParseILRange(char* start)
        {
            switch (*start)
            {
                case '?':
                    Assert.Equal(start, "???");
                    return -1;
                default:
                    return IntegersParser.ParseHexIntegerThreeDigits(start);
            }
        }

        private static NodeFlags ParseNodeFlags(char* start)
        {
            var flags = NodeFlags.None;

            switch (start[0])
            {
                case 'I': flags |= NodeFlags.I; break;
                case 'H': flags |= NodeFlags.H; break;
                case '#': flags |= NodeFlags.Hash; break;
                case 'D': flags |= NodeFlags.D; break;
                case 'n': flags |= NodeFlags.n; break;
                case 'J': flags |= NodeFlags.J; break;
                case '*': flags |= NodeFlags.Star; break;
                default: Assert.Equal(start, "-"); break;
            }
            switch (start[1])
            {
                case 'A': flags |= NodeFlags.A; break;
                case 'c': flags |= NodeFlags.c; break;
                default: Assert.Equal(start + 1, "-"); break;
            }
            switch (start[2])
            {
                case 'C': flags |= NodeFlags.C; break;
                default: Assert.Equal(start + 2, "-"); break;
            }
            switch (start[3])
            {
                case 'X': flags |= NodeFlags.X; break;
                default: Assert.Equal(start + 3, "-"); break;
            }
            switch (start[4])
            {
                case 'G': flags |= NodeFlags.G; break;
                default: Assert.Equal(start + 4, "-"); break;
            }
            switch (start[5])
            {
                case 'O': flags |= NodeFlags.O; break;
                case '+': flags |= NodeFlags.Plus; break;
                default: Assert.Equal(start + 5, "-"); break;
            }
            Assert.Equal(start + 6, "-");
            switch (start[7])
            {
                case 'N': flags |= NodeFlags.N; break;
                default: Assert.Equal(start + 7, "-"); break;
            }
            switch (start[8])
            {
                case 'R': flags |= NodeFlags.R; break;
                default: Assert.Equal(start + 8, "-"); break;
            }
            Assert.Equal(start + 9, "-");
            switch (start[10])
            {
                case 'L': flags |= NodeFlags.L; break;
                default: Assert.Equal(start + 10, "-"); break;
            }
            Assert.Equal(start + 11, "-");

            return flags;

        }
    }
}