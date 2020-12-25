using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Deliberate use of statements for ease of future modification.")]
    internal unsafe partial struct TokenStream
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

        private static Token Peek(char* start, char* end, TokenKind lastToken, out int finalWidth)
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
            int rawWidth = 0;
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
                #region Handling of starting and finishing phases
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
                #endregion

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
                            kind = TokenKind.BasicBlockIdColumnHeader;
                            rawWidth = "BBid".Length;
                            break;
                        case 'R':
                            Assert.Equal(start, "Reachable by");
                            kind = TokenKind.ReachabilitySetsHeader;
                            rawWidth = "Reachable by".Length;
                            break;
                        case 'D':
                            Assert.Equal(start, "Dominated by");
                            kind = TokenKind.DominatorSetsHeader;
                            rawWidth = "Dominated by".Length;
                            break;
                        default: Assert.Impossible(start); goto ReturnUnknown;
                    }
                    break;

                case TokenKind.BasicBlockIdColumnHeader:
                    Assert.Equal(start, "ref");
                    kind = TokenKind.RefColumnHeader;
                    rawWidth = "ref".Length;
                    break;

                case TokenKind.RefColumnHeader:
                    Assert.Equal(start, "try");
                    kind = TokenKind.TryColumnHeader;
                    rawWidth = "try".Length;
                    break;

                case TokenKind.TryColumnHeader:
                    Assert.Equal(start, "hnd");
                    kind = TokenKind.HandleColumnHeader;
                    rawWidth = "hnd".Length;
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
                    kind = TokenKind.WeightColumnHeader;
                    rawWidth = "weight".Length;
                    break;

                case TokenKind.WeightColumnHeader:
                    Assert.Equal(start, "lp [IL range]");
                    kind = TokenKind.ILRangeColumnHeader;
                    rawWidth = "lp [IL range]".Length;
                    break;

                case TokenKind.ILRangeColumnHeader:
                    Assert.Equal(start, "[jump]");
                    kind = TokenKind.JumpColumnHeader;
                    rawWidth = "[jump]".Length;
                    break;

                case TokenKind.JumpColumnHeader:
                    Assert.Equal(start, "[EH region]");
                    kind = TokenKind.ExceptionHandlingColumnHeader;
                    rawWidth = "[EH region]".Length;
                    break;

                case TokenKind.ExceptionHandlingColumnHeader:
                    Assert.Equal(start, "[flags]");
                    kind = TokenKind.FlagsColumnHeader;
                    rawWidth = "[flags]".Length;
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

                #region Shared labels for basic block table and detalization header
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
                #endregion

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
                StartGenTreeNodeDetalizationRow:
                    switch (start[0])
                    {
                        case 'N':
                            Assert.FormatEqual(start, "N000");
                            kind = TokenKind.GenTreeLIRNode;
                            rawValue = IntegersParser.ParseIntegerThreeDigits(start + "N".Length);
                            rawWidth = "N000".Length;
                            break;
                        case '[': goto ReturnGenTreeNodeId;
                        default: goto ReturnUnknown;
                    }
                    break;

                case TokenKind.GenTreeLIRNode:
                    Assert.FormatEqual(start, "(000,", valid: ' ');
                    kind = TokenKind.GenTreeNodeEstimatedTime;
                    rawValue = IntegersParser.ParseGenericInteger(start + "(".Length, out _);
                    rawWidth = "(000,".Length;
                    break;

                case TokenKind.GenTreeNodeEstimatedTime:
                    kind = TokenKind.GenTreeNodeEstimatedCost;
                    rawValue = IntegersParser.ParseGenericInteger(start, out var leftValueWidth);
                    rawWidth = leftValueWidth + ")".Length;
                    break;

                case TokenKind.GenTreeNodeEstimatedCost:
                ReturnGenTreeNodeId:
                    Assert.FormatEqual(start, "[000000]");
                    kind = TokenKind.GenTreeNodeId;
                    rawValue = IntegersParser.ParseIntegerSixDigits(start + "[".Length);
                    rawWidth = "[000000]".Length;
                    break;

                case TokenKind.GenTreeNodeId:
                    kind = TokenKind.GenTreeNodeFlags;
                    rawValue = (int)ParseGenTreeNodeFlags(start);
                    rawWidth = "------------".Length;
                    break;

                case TokenKind.GenTreeNodeFlags:
                    switch (start[0])
                    {
                        case '*' or '+' or '|' or '\\':
                            kind = TokenKind.GenTreeNodeNesting;
                            while (*start is not '*')
                            {
                                Assert.True(*start is '+' or '|' or '\\' or '-' or ' ');
                                start++;
                                rawWidth++;
                            }
                            rawValue = rawWidth / 3;
                            break;
                        default:
                            Assert.Equal(start, "pred ");
                            kind = TokenKind.GenTreeNodePred;
                            rawWidth = "pred ".Length;
                            start += rawWidth;
                            goto ReturnAnyBasicBlock;
                    }
                    break;


                case TokenKind.GenTreeNodeNesting:
                    Assert.Equal(start, "*  ");
                    kind = TokenKind.GenTreeNodeKind;
                    rawWidth = "*  ".Length;
                    rawValue = (int)ParseGenTreeNodeKind(start + rawWidth, out var nodeKindWidth);
                    rawWidth += nodeKindWidth;
                    break;

                case TokenKind.GenTreeNodeKind:
                    switch (start[0])
                    {
                        case '(':
                            Assert.Equal(start, "(h)");
                            kind = TokenKind.GenTreeNodeIsHandle;
                            rawWidth = "(h)".Length;
                            break;
                        case 'n':
                            Assert.Equal(start, "nullcheck");
                            kind = TokenKind.GenTreeNodeNullcheck;
                            rawWidth = "nullcheck".Length;
                            break;
                        case 'h':
                            Assert.Equal(start, "help");
                            kind = TokenKind.GenTreeNodeHelper;
                            rawWidth = "help".Length;
                            break;
                        default: goto ReturnGenTreeNodeType;
                    }
                    break;

                case TokenKind.GenTreeNodeIsHandle:
                case TokenKind.GenTreeNodeNullcheck:
                ReturnGenTreeNodeType:
                    kind = TokenKind.GenTreeNodeType;
                    rawValue = (int)ParseGenTreeNodeType(start, out rawWidth);
                    break;

                case TokenKind.GenTreeNodeType:
                    switch (start[0])
                    {
                        case '<':
                            kind = TokenKind.GenTreeNodeExactType;
                            rawValue = ParseGenTreeNodeExactType(start, out rawWidth).Value;
                            break;
                        case 'V': goto ReturnGenTreeNodeVNumber;
                        case 'n':
                            Assert.Equal(start, "null");
                            kind = TokenKind.GenTreeNodeNullConstant;
                            rawWidth = "null".Length;
                            break;
                        case '(':
                            switch (start[1])
                            {
                                case 'i':
                                    switch (start[3])
                                    {
                                        case 'i':
                                            Assert.Equal(start, "(init)");
                                            kind = TokenKind.GenTreeNodeIsInit;
                                            rawWidth = "(init)".Length;
                                            break;
                                        default:
                                            Assert.FormatEqual(start, "(inl return from call [000000])");
                                            kind = TokenKind.GenTreeNodeIsInlineReturn;
                                            rawValue = IntegersParser.ParseIntegerSixDigits(start + "(inl return from call [".Length);
                                            rawWidth = "(inl return from call [000000])".Length;
                                            break;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "(copy)");
                                    kind = TokenKind.GenTreeNodeIsCopy;
                                    rawWidth = "(copy)".Length;
                                    break;
                            }
                            break;
                        default: goto StartGenTreeNodeDetalizationRow;
                    }
                    break;

                case TokenKind.GenTreeNodeExactType:
                    switch (start[0])
                    {
                        case 'V': goto ReturnGenTreeNodeVNumber;
                        default: goto StartGenTreeNodeDetalizationRow;
                    }

                ReturnGenTreeNodeVNumber:
                    Assert.FormatEqual(start, "V00");
                    kind = TokenKind.GenTreeNodeVNumber;
                    rawValue = IntegersParser.ParseIntegerTwoDigits(start + "V".Length);
                    rawWidth = "V00".Length;
                    break;

                case TokenKind.GenTreeNodeVNumber:
                    switch (start[0])
                    {
                        case 't':
                            Assert.Equal(start, "tmp");
                            kind = TokenKind.GenTreeNodeTemporaryNumber;
                            rawWidth = "tmp".Length;
                            rawValue = IntegersParser.ParseGenericInteger(start + rawWidth, out var temporaryNumberWidth);
                            rawWidth += temporaryNumberWidth;
                            break;
                        case 'a':
                            Assert.Equal(start, "arg");
                            kind = TokenKind.GenTreeNodeArgumentNumber;
                            rawWidth = "arg".Length;
                            rawValue = IntegersParser.ParseGenericInteger(start + rawWidth, out var argumentNumberWidth);
                            rawWidth += argumentNumberWidth;
                            break;
                        default: Assert.Impossible(start); goto ReturnUnknown;
                    }
                    break;

                case TokenKind.GenTreeNodeTemporaryNumber:
                case TokenKind.GenTreeNodeArgumentNumber:
                    switch (start[0])
                    {
                        case 'd':
                            Assert.FormatEqual(start, "d:0");
                            kind = TokenKind.GenTreeNodeDNumber;
                            rawValue = IntegersParser.ParseGenericInteger(start + "d:".Length, out rawWidth);
                            rawWidth += "d:".Length;
                            break;
                        case 'u':
                            Assert.FormatEqual(start, "u:0");
                            kind = TokenKind.GenTreeNodeUNumber;
                            rawValue = IntegersParser.ParseGenericInteger(start + "u:".Length, out rawWidth);
                            rawWidth += "u:".Length;
                            break;
                        default: goto StartGenTreeNodeDetalizationRow;
                    }
                    break;

                case TokenKind.GenTreeNodeUNumber:
                case TokenKind.GenTreeNodeDNumber:
                case TokenKind.GenTreeNodeIsInit:
                case TokenKind.GenTreeNodeIsCopy:
                case TokenKind.GenTreeNodeIsInlineReturn:
                    goto StartGenTreeNodeDetalizationRow;
                #endregion

                #region Handling of unstructured data
                default:
                    switch (start[0])
                    {
                        case '\0': (kind, rawWidth) = (TokenKind.EndOfFile, 0); break;
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

        // Parsing methods in this file save on machine code size in switches by packing the information
        // This is in the following general format (measured to save about up to ~15% in code size as compared to the naive approach):
        // ulong result = [...[Width][Enum]]
        // Tight packing ensures nothing is wasted on encoding the constants in assembly
        // Further savings could be achieved by returning "result" directly
        // Instead of using "out int width"
        // I am not prepared to go that far yet

        private static RyuJitPhase ParseRyuJitPhase(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<RyuJitPhase>() is 1);

            ulong result;
            switch (start[0])
            {
                case 'A':
                    switch (start[1])
                    {
                        case 'l':
                            Assert.Equal(start, "Allocate Objects");
                            result = (ulong)RyuJitPhase.AllocateObjects | ((ulong)"Allocate Objects".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            Assert.Equal(start, "Assertion prop");
                            result = (ulong)RyuJitPhase.AssertionProp | ((ulong)"Assertion prop".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'B':
                    Assert.Equal(start, "Build SSA representation");
                    result = (ulong)RyuJitPhase.BuildSSARepresentation | ((ulong)"Build SSA representation".Length << (sizeof(RyuJitPhase) * 8));
                    break;
                case 'C':
                    switch (start[1])
                    {
                        case 'a':
                            Assert.Equal(start, "Calculate stack level slots");
                            result = (ulong)RyuJitPhase.CalculateStackLevelSlots | ((ulong)"Calculate stack level slots".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'l':
                            switch (start[6])
                            {
                                case 'f':
                                    Assert.Equal(start, "Clone finally");
                                    result = (ulong)RyuJitPhase.CloneFinally | ((ulong)"Clone finally".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Clone loops");
                                    result = (ulong)RyuJitPhase.CloneLoops | ((ulong)"Clone loops".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                        case 'o':
                            switch (start[8])
                            {
                                case 'b':
                                    Assert.Equal(start, "Compute blocks reachability");
                                    result = (ulong)RyuJitPhase.ComputeBlocksReachability | ((ulong)"Compute blocks reachability".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                case 'e':
                                    Assert.Equal(start, "Compute edge weights");
                                    result = (ulong)RyuJitPhase.ComputeEdgeWeights | ((ulong)"Compute edge weights".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Compute preds");
                                    result = (ulong)RyuJitPhase.ComputePreds | ((ulong)"Compute preds".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                        default:
                            Assert.Equal(start, "Create EH funclets");
                            result = (ulong)RyuJitPhase.CreateEHFunclets | ((ulong)"Create EH funclets".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'D':
                    switch (start[1])
                    {
                        case 'e':
                            Assert.Equal(start, "Determine first cold block");
                            result = (ulong)RyuJitPhase.DetermineFirstColdBlock | ((ulong)"Determine first cold block".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            switch (start[3])
                            {
                                case '\'':
                                    Assert.Equal(start, "Do 'simple' lowering");
                                    result = (ulong)RyuJitPhase.DoSimpleLowering | ((ulong)"Do 'simple' lowering".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Do value numbering");
                                    result = (ulong)RyuJitPhase.DoValueNumbering | ((ulong)"Do value numbering".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'E':
                    switch (start[1])
                    {
                        case 'a':
                            Assert.Equal(start, "Early Value Propagation");
                            result = (ulong)RyuJitPhase.EarlyValuePropagation | ((ulong)"Early Value Propagation".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'x':
                            Assert.Equal(start, "Expand patchpoints");
                            result = (ulong)RyuJitPhase.ExpandPatchpoints | ((ulong)"Expand patchpoints".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            switch (start[5])
                            {
                                case 'c':
                                    Assert.Equal(start, "Emit code");
                                    result = (ulong)RyuJitPhase.EmitCode | ((ulong)"Emit code".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Emit GC+EH tables");
                                    result = (ulong)RyuJitPhase.EmitGCPlusEHTables | ((ulong)"Emit GC+EH tables".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'F':
                    Assert.Equal(start, "Find oper order");
                    result = (ulong)RyuJitPhase.FindOperOrder | ((ulong)"Find oper order".Length << (sizeof(RyuJitPhase) * 8));
                    break;
                case 'G':
                    switch (start[1])
                    {
                        case 'e':
                            Assert.Equal(start, "Generate code");
                            result = (ulong)RyuJitPhase.GenerateCode | ((ulong)"Generate code".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            Assert.Equal(start, "GS Cookie");
                            result = (ulong)RyuJitPhase.GSCookie | ((ulong)"GS Cookie".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'H':
                    Assert.Equal(start, "Hoist loop code");
                    result = (ulong)RyuJitPhase.HoistLoopCode | ((ulong)"Hoist loop code".Length << (sizeof(RyuJitPhase) * 8));
                    break;
                case 'I':
                    switch (start[1])
                    {
                        case 'm':
                            Assert.Equal(start, "Importation");
                            result = (ulong)RyuJitPhase.Importation | ((ulong)"Importation".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            switch (start[2])
                            {
                                case 'd':
                                    Assert.Equal(start, "Indirect call transform");
                                    result = (ulong)RyuJitPhase.IndirectCallTransform | ((ulong)"Indirect call transform".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Insert GC Polls");
                                    result = (ulong)RyuJitPhase.InsertGCPolls | ((ulong)"Insert GC Polls".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'L':
                    switch (start[1])
                    {
                        case 'i':
                            Assert.Equal(start, "Linear scan register alloc");
                            result = (ulong)RyuJitPhase.LinearScanRegisterAlloc | ((ulong)"Linear scan register alloc".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default: result = (ulong)RyuJitPhase.LoweringNodeInfo | ((ulong)"Lowering nodeinfo".Length << (sizeof(RyuJitPhase) * 8)); break;
                    }
                    break;
                case 'M':
                    switch (start[1])
                    {
                        case 'a':
                            Assert.Equal(start, "Mark local vars");
                            result = (ulong)RyuJitPhase.MarkLocalVars | ((ulong)"Mark local vars".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'e':
                            switch (start[6])
                            {
                                case 'c':
                                    Assert.Equal(start, "Merge callfinally chains");
                                    result = (ulong)RyuJitPhase.MergeCallfinallyChains | ((ulong)"Merge callfinally chains".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Merge throw blocks");
                                    result = (ulong)RyuJitPhase.MergeThrowBlocks | ((ulong)"Merge throw blocks".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                        default:
                            switch (start[8])
                            {
                                case 'A':
                                    Assert.Equal(start, "Morph - Add internal blocks");
                                    result = (ulong)RyuJitPhase.MorphAddInternalBlocks | ((ulong)"Morph - Add internal blocks".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                case 'B':
                                    Assert.Equal(start, "Morph - ByRefs");
                                    result = (ulong)RyuJitPhase.MorphByRefs | ((ulong)"Morph - ByRefs".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                case 'G':
                                    Assert.Equal(start, "Morph - Global");
                                    result = (ulong)RyuJitPhase.MorphGlobal | ((ulong)"Morph - Global".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                case 'I':
                                    switch (start[10])
                                    {
                                        case 'i':
                                            Assert.Equal(start, "Morph - Init");
                                            result = (ulong)RyuJitPhase.MorphInit | ((ulong)"Morph - Init".Length << (sizeof(RyuJitPhase) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "Morph - Inlining");
                                            result = (ulong)RyuJitPhase.MorphInlining | ((ulong)"Morph - Inlining".Length << (sizeof(RyuJitPhase) * 8));
                                            break;
                                    }
                                    break;
                                case 'P':
                                    Assert.Equal(start, "Morph - Promote Structs");
                                    result = (ulong)RyuJitPhase.MorphPromoteStructs | ((ulong)"Morph - Promote Structs".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Morph - Structs/AddrExp");
                                    result = (ulong)RyuJitPhase.MorphStructsAddressExposed | ((ulong)"Morph - Structs/AddrExp".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'O':
                    switch (start[9])
                    {
                        case 'b':
                            Assert.Equal(start, "Optimize bools");
                            result = (ulong)RyuJitPhase.OptimizeBools | ((ulong)"Optimize bools".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'i':
                            Assert.Equal(start, "Optimize index checks");
                            result = (ulong)RyuJitPhase.OptimizeIndexChecks | ((ulong)"Optimize index checks".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'l':
                            switch (start[10])
                            {
                                case 'a':
                                    Assert.Equal(start, "Optimize layout");
                                    result = (ulong)RyuJitPhase.OptimizeLayout | ((ulong)"Optimize layout".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Optimize loops");
                                    result = (ulong)RyuJitPhase.OptimizeLoops | ((ulong)"Optimize loops".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                        default:
                            Assert.Equal(start, "Optimize Valnum CSEs");
                            result = (ulong)RyuJitPhase.OptimizeValnumCSEs | ((ulong)"Optimize Valnum CSEs".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'P':
                    switch (start[1])
                    {
                        case 'o':
                            Assert.Equal(start, "Post-import");
                            result = (ulong)RyuJitPhase.PostImport | ((ulong)"Post-import".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            Assert.Equal(start, "Pre-import");
                            result = (ulong)RyuJitPhase.PreImport | ((ulong)"Pre-import".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'R':
                    switch (start[1])
                    {
                        case 'a':
                            Assert.Equal(start, "Rationalize IR");
                            result = (ulong)RyuJitPhase.RationalizeIR | ((ulong)"Rationalize IR".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            switch (start[13])
                            {
                                case 'f':
                                    Assert.Equal(start, "Remove empty finally");
                                    result = (ulong)RyuJitPhase.RemoveEmptyFinally | ((ulong)"Remove empty finally".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Remove empty try");
                                    result = (ulong)RyuJitPhase.RemoveEmptyTry | ((ulong)"Remove empty try".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'S':
                    Assert.Equal(start, "Set block order");
                    result = (ulong)RyuJitPhase.SetBlockOrder | ((ulong)"Set block order".Length << (sizeof(RyuJitPhase) * 8));
                    break;
                case 'U':
                    switch (start[1])
                    {
                        case 'n':
                            Assert.Equal(start, "Unroll loops");
                            result = (ulong)RyuJitPhase.UnrollLoops | ((ulong)"Unroll loops".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            Assert.Equal(start, "Update flow graph early pass");
                            result = (ulong)RyuJitPhase.UpdateFlowGraphEarlyPass | ((ulong)"Update flow graph early pass".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                default:
                    Assert.Equal(start, "VN based copy prop");
                    result = (ulong)RyuJitPhase.VNBasedCopyPropagation | ((ulong)"VN based copy prop".Length << (sizeof(RyuJitPhase) * 8));
                    break;
            }

            width = (int)(result >> (sizeof(RyuJitPhase) * 8));
            return (RyuJitPhase)result;
        }

        private static RyuJitFunction ParseRyuJitFunction(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<RyuJitFunction>() is 1);

            ulong result;
            switch (*start)
            {
                case 'c':
                    Assert.Equal(start, "compInitDebuggingInfo()");
                    result = (ulong)RyuJitFunction.CompInitDebuggingInfo | ((ulong)"compInitDebuggingInfo()".Length << (sizeof(RyuJitFunction) * 8));
                    break;
                case 'e':
                    switch (start[4])
                    {
                        case 'E':
                            Assert.Equal(start, "emitEndCodeGen()");
                            result = (ulong)RyuJitFunction.EmitEndCodeGen | ((ulong)"emitEndCodeGen()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "emitJumpDistBind()");
                            result = (ulong)RyuJitFunction.EmitJumpDistBind | ((ulong)"emitJumpDistBind()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
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
                                        case 'B':
                                            Assert.Equal(start, "fgComputeBlockAndEdgeWeights()");
                                            result = (ulong)RyuJitFunction.FgComputeBlockAndEdgeWeights | ((ulong)"fgComputeBlockAndEdgeWeights()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        case 'D':
                                            Assert.Equal(start, "fgComputeDoms");
                                            result = (ulong)RyuJitFunction.FgComputeDoms | ((ulong)"fgComputeDoms".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        case 'P':
                                            Assert.Equal(start, "fgComputePreds()");
                                            result = (ulong)RyuJitFunction.FgComputePreds | ((ulong)"fgComputePreds()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "fgComputeReachability");
                                            result = (ulong)RyuJitFunction.FgComputeReachability | ((ulong)"fgComputeReachability".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "fgCreateFunclets()");
                                    result = (ulong)RyuJitFunction.FgCreateFunclets | ((ulong)"fgCreateFunclets()".Length << (sizeof(RyuJitFunction) * 8));
                                    break; ;
                            }
                            break;
                        case 'D':
                            switch (start[4])
                            {
                                case 'b':
                                    Assert.Equal(start, "fgDebugCheckBBlist");
                                    result = (ulong)RyuJitFunction.FgDebugCheckBBlist | ((ulong)"fgDebugCheckBBlist".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgDetermineFirstColdBlock()");
                                    result = (ulong)RyuJitFunction.FgDetermineFirstColdBlock | ((ulong)"fgDetermineFirstColdBlock()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'E':
                            Assert.Equal(start, "fgExpandRarelyRunBlocks()");
                            result = (ulong)RyuJitFunction.FgExpandRarelyRunBlocks | ((ulong)"fgExpandRarelyRunBlocks()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'F':
                            switch (start[6])
                            {
                                case 'B':
                                    Assert.Equal(start, "fgFindBasicBlocks()");
                                    result = (ulong)RyuJitFunction.FgFindBasicBlocks | ((ulong)"fgFindBasicBlocks()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgFindOperOrder()");
                                    result = (ulong)RyuJitFunction.FgFindOperOrder | ((ulong)"fgFindOperOrder()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'I':
                            Assert.Equal(start, "fgInterBlockLocalVarLiveness()");
                            result = (ulong)RyuJitFunction.FgInterBlockLocalVarLiveness | ((ulong)"fgInterBlockLocalVarLiveness()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'L':
                            Assert.Equal(start, "fgLocalVarLiveness()");
                            result = (ulong)RyuJitFunction.FgLocalVarLiveness | ((ulong)"fgLocalVarLiveness()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'M':
                            switch (start[3])
                            {
                                case 'a':
                                    Assert.Equal(start, "fgMarkAddressExposedLocals()");
                                    result = (ulong)RyuJitFunction.FgMarkAddressExposedLocals | ((ulong)"fgMarkAddressExposedLocals()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgMorphBlocks()");
                                    result = (ulong)RyuJitFunction.FgMorphBlocks | ((ulong)"fgMorphBlocks()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'P':
                            switch (start[3])
                            {
                                case 'e':
                                    Assert.Equal(start, "fgPerBlockLocalVarLiveness()");
                                    result = (ulong)RyuJitFunction.FgPerBlockLocalVarLiveness | ((ulong)"fgPerBlockLocalVarLiveness()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgPromoteStructs()");
                                    result = (ulong)RyuJitFunction.FgPromoteStructs | ((ulong)"fgPromoteStructs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'R':
                            switch (start[4])
                            {
                                case 'm':
                                    switch (start[13])
                                    {
                                        case 'B':
                                            Assert.Equal(start, "fgRemoveEmptyBlocks");
                                            result = (ulong)RyuJitFunction.FgRemoveEmptyBlocks | ((ulong)"fgRemoveEmptyBlocks".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "fgRemoveEmptyTry()");
                                            result = (ulong)RyuJitFunction.FgRemoveEmptyTry | ((ulong)"fgRemoveEmptyTry()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                    }
                                    break;
                                case 'o':
                                    Assert.Equal(start, "fgReorderBlocks()");
                                    result = (ulong)RyuJitFunction.FgReorderBlocks | ((ulong)"fgReorderBlocks()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 's':
                                    Assert.Equal(start, "fgResetImplicitByRefRefCount()");
                                    result = (ulong)RyuJitFunction.FgResetImplicitByRefRefCount | ((ulong)"fgResetImplicitByRefRefCount()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgRetypeImplicitByRefArgs()");
                                    result = (ulong)RyuJitFunction.FgRetypeImplicitByRefArgs | ((ulong)"fgRetypeImplicitByRefArgs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'S':
                            Assert.Equal(start, "fgSetBlockOrder()");
                            result = (ulong)RyuJitFunction.FgSetBlockOrder | ((ulong)"fgSetBlockOrder()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'T':
                            Assert.Equal(start, "fgTailMergeThrows");
                            result = (ulong)RyuJitFunction.FgTailMergeThrows | ((ulong)"fgTailMergeThrows".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'U':
                            Assert.Equal(start, "fgUpdateFlowGraph()");
                            result = (ulong)RyuJitFunction.FgUpdateFlowGraph | ((ulong)"fgUpdateFlowGraph()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "fgValueNumber()");
                            result = (ulong)RyuJitFunction.FgValueNumber | ((ulong)"fgValueNumber()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                    }
                    break;
                case 'g':
                    switch (start[1])
                    {
                        case 'c':
                            Assert.Equal(start, "gcInfoBlockHdrSave()");
                            result = (ulong)RyuJitFunction.GcInfoBlockHdrSave | ((ulong)"gcInfoBlockHdrSave()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            switch (start[3])
                            {
                                case 'E':
                                    Assert.Equal(start, "genEnregisterIncomingStackArgs()");
                                    result = (ulong)RyuJitFunction.GenEnregisterIncomingStackArgs | ((ulong)"genEnregisterIncomingStackArgs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 'F':
                                    switch (start[5])
                                    {
                                        case 'E':
                                            Assert.Equal(start, "genFnEpilog()");
                                            result = (ulong)RyuJitFunction.GenFnEpilog | ((ulong)"genFnEpilog()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        default:
                                            switch (start[11])
                                            {
                                                case '(':
                                                    Assert.Equal(start, "genFnProlog()");
                                                    result = (ulong)RyuJitFunction.GenFnProlog | ((ulong)"genFnProlog()".Length << (sizeof(RyuJitFunction) * 8));
                                                    break;
                                                default:
                                                    Assert.Equal(start, "genFnPrologCalleeRegArgs()");
                                                    result = (ulong)RyuJitFunction.GenFnPrologCalleeRegArgs | ((ulong)"genFnPrologCalleeRegArgs()".Length << (sizeof(RyuJitFunction) * 8));
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case 'G':
                                    Assert.Equal(start, "genGenerateCode()");
                                    result = (ulong)RyuJitFunction.GenGenerateCode | ((ulong)"genGenerateCode()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 'I':
                                    Assert.Equal(start, "genIPmappingGen()");
                                    result = (ulong)RyuJitFunction.GenIPmappingGen | ((ulong)"genIPmappingGen()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "genSetScopeInfo()");
                                    result = (ulong)RyuJitFunction.GenSetScopeInfo | ((ulong)"genSetScopeInfo()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'i':
                    Assert.Equal(start, "impImport()");
                    result = (ulong)RyuJitFunction.ImpImport | ((ulong)"impImport()".Length << (sizeof(RyuJitFunction) * 8));
                    break;
                case 'L':
                    Assert.Equal(start, "LinearScan::allocateRegisters()");
                    result = (ulong)RyuJitFunction.LinearScan_allocateRegisters | ((ulong)"LinearScan::allocateRegisters()".Length << (sizeof(RyuJitFunction) * 8));
                    break;
                case 'l':
                    switch (start[3])
                    {
                        case 'A':
                            Assert.Equal(start, "lvaAssignFrameOffsets(FINAL_FRAME_LAYOUT)");
                            result = (ulong)RyuJitFunction.LvaAssignFrameOffsets | ((ulong)"lvaAssignFrameOffsets(FINAL_FRAME_LAYOUT)".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "lvaMarkLocalVars()");
                            result = (ulong)RyuJitFunction.LvaMarkLocalVars | ((ulong)"lvaMarkLocalVars()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                    }
                    break;
                case 'o':
                    switch (start[3])
                    {
                        case 'A':
                            switch (start[4])
                            {
                                case 'd':
                                    Assert.Equal(start, "optAddCopies()");
                                    result = (ulong)RyuJitFunction.OptAddCopies | ((ulong)"optAddCopies()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "optAssertionPropMain()");
                                    result = (ulong)RyuJitFunction.OptAssertionPropMain | ((ulong)"optAssertionPropMain()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'C':
                            Assert.Equal(start, "optCloneLoops()");
                            result = (ulong)RyuJitFunction.OptCloneLoops | ((ulong)"optCloneLoops()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'E':
                            Assert.Equal(start, "optEarlyProp()");
                            result = (ulong)RyuJitFunction.OptEarlyProp | ((ulong)"optEarlyProp()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'O':
                            switch (start[11])
                            {
                                case 'B':
                                    Assert.Equal(start, "optOptimizeBools()");
                                    result = (ulong)RyuJitFunction.OptOptimizeBools | ((ulong)"optOptimizeBools()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 'C':
                                    Assert.Equal(start, "optOptimizeCSEs()");
                                    result = (ulong)RyuJitFunction.OptOptimizeCSEs | ((ulong)"optOptimizeCSEs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 'L':
                                    switch (start[12])
                                    {
                                        case 'a':
                                            Assert.Equal(start, "optOptimizeLayout()");
                                            result = (ulong)RyuJitFunction.OptOptimizeLayout | ((ulong)"optOptimizeLayout()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "optOptimizeLoops()");
                                            result = (ulong)RyuJitFunction.OptOptimizeLoops | ((ulong)"optOptimizeLoops()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "optOptimizeValnumCSEs()");
                                    result = (ulong)RyuJitFunction.OptOptimizeValnumCSEs | ((ulong)"optOptimizeValnumCSEs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'R':
                            Assert.Equal(start, "optRemoveRedundantZeroInits()");
                            result = (ulong)RyuJitFunction.OptRemoveRedundantZeroInits | ((ulong)"optRemoveRedundantZeroInits()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "optVnCopyProp()");
                            result = (ulong)RyuJitFunction.OptVnCopyProp | ((ulong)"optVnCopyProp()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                    }
                    break;
                case 'O':
                    Assert.Equal(start, "OptimizeRangeChecks()");
                    result = (ulong)RyuJitFunction.OptimizeRangeChecks | ((ulong)"OptimizeRangeChecks()".Length << (sizeof(RyuJitFunction) * 8));
                    break;
                default:
                    switch (start[12])
                    {
                        case 'B':
                            Assert.Equal(start, "SsaBuilder::Build()");
                            result = (ulong)RyuJitFunction.SsaBuilder_Build | ((ulong)"SsaBuilder::Build()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'I':
                            Assert.Equal(start, "SsaBuilder::InsertPhiFunctions()");
                            result = (ulong)RyuJitFunction.SsaBuilder_InsertPhiFunctions | ((ulong)"SsaBuilder::InsertPhiFunctions()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "SsaBuilder::RenameVariables()");
                            result = (ulong)RyuJitFunction.SsaBuilder_RenameVariables | ((ulong)"SsaBuilder::RenameVariables()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                    }
                    break;
            }

            width = (int)(result >> (sizeof(RyuJitFunction) * 8));
            return (RyuJitFunction)result;
        }

        private static BasicBlockJumpTargetKind ParseBasicBlockJumpTargetKind(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<BasicBlockJumpTargetKind>() is 1);

            ulong result;
            switch (start[2])
            {
                case 'c':
                    Assert.Equal(start, "( cond )");
                    result = (ulong)BasicBlockJumpTargetKind.Conditional | ((ulong)"( cond )".Length << (sizeof(BasicBlockJumpTargetKind) * 8));
                    break;
                case 'l':
                    Assert.Equal(start, "(always)");
                    result = (ulong)BasicBlockJumpTargetKind.Always | ((ulong)"(always)".Length << (sizeof(BasicBlockJumpTargetKind) * 8));
                    break;
                case 'e':
                    Assert.Equal(start, "(return)");
                    result = (ulong)BasicBlockJumpTargetKind.Return | ((ulong)"(return)".Length << (sizeof(BasicBlockJumpTargetKind) * 8));
                    break;
                default:
                    Assert.Equal(start, "(cond)");
                    result = (ulong)BasicBlockJumpTargetKind.Conditional | ((ulong)"(cond)".Length << (sizeof(BasicBlockJumpTargetKind) * 8));
                    break;
            }

            width = (int)(result >> (sizeof(BasicBlockJumpTargetKind) * 8));
            return (BasicBlockJumpTargetKind)result;
        }

        private static BasicBlockFlag ParseBasicBlockFlag(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<BasicBlockFlag>() is 1);

            ulong result;
            switch (*start)
            {
                case 'i':
                    switch (start[1])
                    {
                        case 'd':
                            Assert.Equal(start, "idxlen");
                            result = (ulong)BasicBlockFlag.IdxLen | ((ulong)"idxlen".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                        case 'n':
                            Assert.Equal(start, "internal");
                            result = (ulong)BasicBlockFlag.Internal | ((ulong)"internal".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                        default:
                            Assert.Equal(start, "i");
                            result = (ulong)BasicBlockFlag.I | ((ulong)"i".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                    }
                    break;
                case 'l':
                    Assert.Equal(start, "label");
                    result = (ulong)BasicBlockFlag.Label | ((ulong)"label".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
                case 't':
                    Assert.Equal(start, "target");
                    result = (ulong)BasicBlockFlag.Target | ((ulong)"target".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
                case 'h':
                    Assert.Equal(start, "hascall");
                    result = (ulong)BasicBlockFlag.HasCall | ((ulong)"hascall".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
                case 'n':
                    switch (start[1])
                    {
                        case 'e':
                            Assert.Equal(start, "newobj");
                            result = (ulong)BasicBlockFlag.NewObj | ((ulong)"newobj".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                        default:
                            Assert.Equal(start, "nullcheck");
                            result = (ulong)BasicBlockFlag.NullCheck | ((ulong)"nullcheck".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                    }
                    break;
                case 'g':
                    Assert.Equal(start, "gcsafe");
                    result = (ulong)BasicBlockFlag.GCSafe | ((ulong)"gcsafe".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
                default:
                    Assert.Equal(start, "LIR");
                    result = (ulong)BasicBlockFlag.LIR | ((ulong)"LIR".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
            }

            width = (int)(result >> (sizeof(BasicBlockFlag) * 8));
            return (BasicBlockFlag)result;
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

        private static GenTreeNodeFlags ParseGenTreeNodeFlags(char* start)
        {
            var flags = GenTreeNodeFlags.None;

            switch (start[0])
            {
                case 'I': flags |= GenTreeNodeFlags.I; break;
                case 'H': flags |= GenTreeNodeFlags.H; break;
                case '#': flags |= GenTreeNodeFlags.Hash; break;
                case 'D': flags |= GenTreeNodeFlags.D; break;
                case 'n': flags |= GenTreeNodeFlags.n; break;
                case 'J': flags |= GenTreeNodeFlags.J; break;
                case '*': flags |= GenTreeNodeFlags.Star; break;
                default: Assert.Equal(start, "-"); break;
            }
            switch (start[1])
            {
                case 'A': flags |= GenTreeNodeFlags.A; break;
                case 'c': flags |= GenTreeNodeFlags.c; break;
                default: Assert.Equal(start + 1, "-"); break;
            }
            switch (start[2])
            {
                case 'C': flags |= GenTreeNodeFlags.C; break;
                default: Assert.Equal(start + 2, "-"); break;
            }
            switch (start[3])
            {
                case 'X': flags |= GenTreeNodeFlags.X; break;
                default: Assert.Equal(start + 3, "-"); break;
            }
            switch (start[4])
            {
                case 'G': flags |= GenTreeNodeFlags.G; break;
                default: Assert.Equal(start + 4, "-"); break;
            }
            switch (start[5])
            {
                case 'O': flags |= GenTreeNodeFlags.O; break;
                case '+': flags |= GenTreeNodeFlags.Plus; break;
                default: Assert.Equal(start + 5, "-"); break;
            }
            Assert.Equal(start + 6, "-");
            switch (start[7])
            {
                case 'N': flags |= GenTreeNodeFlags.N; break;
                default: Assert.Equal(start + 7, "-"); break;
            }
            switch (start[8])
            {
                case 'R': flags |= GenTreeNodeFlags.R; break;
                default: Assert.Equal(start + 8, "-"); break;
            }
            Assert.Equal(start + 9, "-");
            switch (start[10])
            {
                case 'L': flags |= GenTreeNodeFlags.L; break;
                default: Assert.Equal(start + 10, "-"); break;
            }
            Assert.Equal(start + 11, "-");

            return flags;

        }

        private static GenTreeNodeType ParseGenTreeNodeType(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<GenTreeNodeType>() is 1);
            // This enusres that we have already handled "nullcheck" and "help"
            Assert.True(start[0] is not 'n' or 'h');

            ulong result;
            switch (start[0])
            {
                case '<':
                    Assert.Equal(start, "<UNDEF>");
                    result = (ulong)GenTreeNodeType.Undefined | ((ulong)"<UNDEF>".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 'b':
                    switch (start[2])
                    {
                        case 'k':
                            Assert.Equal(start, "blk");
                            result = (ulong)GenTreeNodeType.Blk | ((ulong)"blk".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case 'o':
                            Assert.Equal(start, "bool");
                            result = (ulong)GenTreeNodeType.Bool | ((ulong)"bool".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case 'r':
                            Assert.Equal(start, "byref");
                            result = (ulong)GenTreeNodeType.Byref | ((ulong)"byref".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        default:
                            Assert.Equal(start, "byte");
                            result = (ulong)GenTreeNodeType.Byte | ((ulong)"byte".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                    }
                    break;
                case 'd':
                    Assert.Equal(start, "double");
                    result = (ulong)GenTreeNodeType.Double | ((ulong)"double".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 'f':
                    Assert.Equal(start, "float");
                    result = (ulong)GenTreeNodeType.Float | ((ulong)"float".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 'i':
                    Assert.Equal(start, "int");
                    result = (ulong)GenTreeNodeType.Int | ((ulong)"int".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 'l':
                    switch (start[1])
                    {
                        case 'c':
                            Assert.Equal(start, "lclBlk");
                            result = (ulong)GenTreeNodeType.LclBlk | ((ulong)"lclBlk".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        default:
                            Assert.Equal(start, "long");
                            result = (ulong)GenTreeNodeType.Long | ((ulong)"long".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                    }
                    break;
                case 'r':
                    Assert.Equal(start, "ref");
                    result = (ulong)GenTreeNodeType.Ref | ((ulong)"ref".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 's':
                    switch (start[4])
                    {
                        case 't':
                            Assert.Equal(start, "short");
                            result = (ulong)GenTreeNodeType.Short | ((ulong)"short".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case '1':
                            switch (start[5])
                            {
                                case '2':
                                    Assert.Equal(start, "simd12");
                                    result = (ulong)GenTreeNodeType.Simd12 | ((ulong)"simd12".Length << (sizeof(GenTreeNodeType) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "simd16");
                                    result = (ulong)GenTreeNodeType.Simd16 | ((ulong)"simd16".Length << (sizeof(GenTreeNodeType) * 8));
                                    break;
                            }
                            break;
                        case '3':
                            Assert.Equal(start, "simd32");
                            result = (ulong)GenTreeNodeType.Simd32 | ((ulong)"simd32".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case '8':
                            Assert.Equal(start, "simd8");
                            result = (ulong)GenTreeNodeType.Simd8 | ((ulong)"simd8".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        default:
                            Assert.Equal(start, "struct");
                            result = (ulong)GenTreeNodeType.Struct | ((ulong)"struct".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                    }
                    break;
                case 'u':
                    switch (start[1])
                    {
                        case 'b':
                            Assert.Equal(start, "ubyte");
                            result = (ulong)GenTreeNodeType.UByte | ((ulong)"ubyte".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case 'i':
                            Assert.Equal(start, "uint");
                            result = (ulong)GenTreeNodeType.UInt | ((ulong)"uint".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case 'l':
                            Assert.Equal(start, "ulong");
                            result = (ulong)GenTreeNodeType.ULong | ((ulong)"ulong".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        default:
                            Assert.Equal(start, "ushort");
                            result = (ulong)GenTreeNodeType.UShort | ((ulong)"ushort".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                    }
                    break;
                default:
                    Assert.Equal(start, "void");
                    result = (ulong)GenTreeNodeType.Void | ((ulong)"void".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
            }

            width = (int)(result >> (sizeof(GenTreeNodeType) * 8));
            return (GenTreeNodeType)result;
        }

        private static GenTreeNodeExactTypeHandle ParseGenTreeNodeExactType(char* start, out int width)
        {
            var index = new Span<char>(start, int.MaxValue).IndexOf('>');
            width = index + ">".Length;

            // We do not yet have an implementation for string pools
            return new GenTreeNodeExactTypeHandle(0);
        }
    }
}