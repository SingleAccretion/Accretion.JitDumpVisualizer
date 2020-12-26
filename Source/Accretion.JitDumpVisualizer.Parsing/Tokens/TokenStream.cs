using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing;
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
                            rawValue = (int)Lexer.ParseRyuJitPhase(start + rawWidth, out var phaseWidth);
                            rawWidth += phaseWidth;
                            break;
                        case 'I':
                            switch (start[2])
                            {
                                case ' ':
                                    Assert.Equal(start, "In");
                                    kind = TokenKind.StartingFunction;
                                    rawValue = (int)Lexer.ParseRyuJitFunction(start + "In ".Length, out rawWidth);
                                    rawWidth += "In ".Length;
                                    break;
                                default:
                                    switch (start[7])
                                    {
                                        case '@':
                                            Assert.FormatEqual(start, "Inline @[000000]");
                                            kind = TokenKind.InlineStartingAt;
                                            rawValue = IntegersParser.ParseIntegerSixDigits(start + "Inline @[".Length);
                                            rawWidth = "Inline @[000000]".Length;
                                            break;
                                        case 'T':
                                            Assert.Equal(start, "Inline Tree");
                                            kind = TokenKind.InlineTreeHeader;
                                            rawWidth = "Inline Tree".Length;
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
                    rawValue = IntegersParser.ParseIntegerFourDigits(start + "[".Length);
                    rawWidth = "[0000]".Length;
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
                            rawValue = (int)Lexer.ParseBasicBlockFlag(start, out var flagWidth);
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
                    rawValue = (int)Lexer.ParseBasicBlockJumpTargetKind(start, out var jumpKindWidth);
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
                    rawValue = IntegersParser.ParseGenericInteger(start, out rawWidth);
                    rawWidth += ")".Length;
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
                    rawValue = (int)Lexer.ParseGenTreeNodeFlags(start);
                    rawWidth = "------------".Length;
                    break;

                case TokenKind.GenTreeNodeFlags:
                    switch (start[0])
                    {
                        case '*' or '+' or '|' or '\\':
                            kind = TokenKind.GenTreeNodeKind;
                            var paddingWidth = 0;
                            while (*start is not '*')
                            {
                                Assert.True(*start is '+' or '|' or '\\' or '-' or ' ');
                                start++;
                                paddingWidth++;
                            }
                            Assert.Equal(start, "*  ");
                            rawValue = (int)ParseGenTreeNodeKind(start + "*  ".Length, out rawWidth);
                            rawWidth += paddingWidth + "*  ".Length;
                            break;
                        default:
                            Assert.Equal(start, "pred ");
                            kind = TokenKind.GenTreeNodePred;
                            rawWidth = "pred ".Length;
                            start += rawWidth;
                            goto ReturnAnyBasicBlock;
                    }
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
                    rawValue = (int)Lexer.ParseGenTreeNodeType(start, out rawWidth);
                    break;

                case TokenKind.GenTreeNodeType:
                    switch (start[0])
                    {
                        case '<':
                            switch (start[1])
                            {
                                case '-': goto ReturnUnknown;
                                default:
                                    kind = TokenKind.GenTreeNodeExactType;
                                    rawValue = Lexer.ParseGenTreeNodeExactType(start, out rawWidth).Value;
                                    break;
                            }
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
                            rawValue = IntegersParser.ParseGenericInteger(start + "tmp".Length, out rawWidth);
                            rawWidth += "tmp".Length;
                            break;
                        case 'a':
                            Assert.Equal(start, "arg");
                            kind = TokenKind.GenTreeNodeArgumentNumber;
                            rawValue = IntegersParser.ParseGenericInteger(start + "arg".Length, out rawWidth);
                            rawWidth += "arg".Length;
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
    }
}