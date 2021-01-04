using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using Accretion.JitDumpVisualizer.Parsing.Auxiliaries.Logging;
using Accretion.JitDumpVisualizer.Parsing.IO;
using Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Deliberate use of statements for ease of future modification.")]
    internal unsafe static partial class Tokenizer
    {
        // Empirically determined, intended to be an overstimation
        private const double TokensPerChar = 1;
        private const int InputBufferLength = 4 * 1024;
        private const int InputBufferSafetyPadding = 1 * 1024;
        private const int LookaheadLimit = 200;

        public static Token[] Tokenize(string dump)
        {
            throw new NotImplementedException();
        }

        public static Token[] Tokenize(FileReader fileReader)
        {
            var dumpCharCount = fileReader.Length;
            Logger.Log(LoggedEvent.DumpFileSize, dumpCharCount);

            var tokenCount = (int)(dumpCharCount * TokensPerChar);
            Logger.Log(LoggedEvent.EstimatedTokenCount, tokenCount);
            var tokens = GC.AllocateUninitializedArray<Token>(tokenCount, pinned: true);

            // We overallocate here to avoid buffer overruns and use lookahead freely
            // The padding is now limited to 2KB, which should be enough
            fixed (char* bufferStartPtr = GC.AllocateUninitializedArray<char>(InputBufferLength + InputBufferSafetyPadding))
            {
                // Clearing only the padding allows us to stay in the managed land
                new Span<char>(bufferStartPtr + InputBufferLength, InputBufferSafetyPadding).Clear();

                var tokensPtr = (Token*)Unsafe.AsPointer(ref tokens[0]);
                bool endHasBeenReached = false;
                var currentPtr = bufferStartPtr + InputBufferLength;
                while (!endHasBeenReached)
                {
                    var remainsFromLastBlock = (int)(InputBufferLength - (currentPtr - bufferStartPtr));
                    Buffer.MemoryCopy(currentPtr, bufferStartPtr, InputBufferLength, remainsFromLastBlock * sizeof(char));

                    var newBlock = new Span<char>(bufferStartPtr, InputBufferLength);
                    var remainingNewBlock = newBlock.Slice(remainsFromLastBlock);
                    var bytesRead = fileReader.ReadBlock(remainingNewBlock);
                    if (bytesRead < remainingNewBlock.Length)
                    {
                        remainingNewBlock.Slice(bytesRead).Clear();
                        endHasBeenReached = true;
                    }

                    currentPtr = bufferStartPtr;
                    NextBlock(ref currentPtr, ref tokensPtr);
                }
            }

            return tokens;
        }

        private static void NextBlock(ref char* start, ref Token* tokens)
        {
            var end = start + InputBufferLength - LookaheadLimit;
            while (start < end)
            {
                while (*start is ' ' or '\t'
#if RELEASE
                or '\r' or '\n'
#endif
                )
                {
                    start++;
                }

                var startCopy = start;
                TokenKind kind;
                int rawWidth = 0;
                uint rawValue = 0;

#if DEBUG
                if (!IsNotEndOfLine(start))
                {
                    start = SkipEndOfLine(start);
                    continue;
                }
#endif

                switch (tokens[-1].Kind)
                {
                    #region Handling of starting and finishing phases
                    case TokenKind.FifteenStars or TokenKind.InlineStartingAt:
                        switch (startCopy[0])
                        {
                            case 'S':
                                Assert.Equal(startCopy, "Starting PHASE");
                                kind = TokenKind.StartingPhase;
                                rawWidth = "Starting PHASE ".Length;
                                goto ParsePhase;
                            case 'F':
                                Assert.Equal(startCopy, "Finishing PHASE");
                                kind = TokenKind.FinishingPhase;
                                rawWidth = "Finishing PHASE ".Length;
                            ParsePhase:
                                rawValue = (uint)Lexer.ParseRyuJitPhase(startCopy + rawWidth, out var phaseWidth);
                                rawWidth += phaseWidth;
                                break;
                            case 'I':
                                switch (startCopy[2])
                                {
                                    case ' ':
                                        Assert.Equal(startCopy, "In");
                                        kind = TokenKind.StartingFunction;
                                        rawValue = (uint)Lexer.ParseRyuJitFunction(startCopy + "In ".Length, out rawWidth);
                                        rawWidth += "In ".Length;
                                        break;
                                    default:
                                        switch (startCopy[7])
                                        {
                                            case '@':
                                                Assert.FormatEqual(startCopy, "Inline @[000000]");
                                                kind = TokenKind.InlineStartingAt;
                                                rawValue = IntegersParser.ParseIntegerSixDigits(startCopy + "Inline @[".Length);
                                                rawWidth = "Inline @[000000]".Length;
                                                break;
                                            case 'T':
                                                Assert.Equal(startCopy, "Inline Tree");
                                                kind = TokenKind.InlineTreeHeader;
                                                rawWidth = "Inline Tree".Length;
                                                break;
                                            default: Assert.Impossible(startCopy); goto ReturnUnknown;
                                        }
                                        break;
                                }
                                break;
                            default: goto ReturnUnknown;
                        }
                        break;

                    case TokenKind.FinishingPhase:
                        switch (startCopy[0])
                        {
                            case '[':
                                Assert.Equal(startCopy, "[no changes]");
                                kind = TokenKind.NoChangesInPhase;
                                rawWidth = "[no changes]".Length;
                                break;
                            default: goto ReturnUnknown;
                        }
                        break;
                    #endregion

                    #region Handling of basic block table
                    case TokenKind.BasicBlockTableHeader:
                        tokens = ParseBasicBlockTableColumnHeaders(ref start, tokens);
                        continue;

                    case TokenKind.BasicBlockFlagsColumnHeader:
                        Assert.Equal(start, Token.OneHundredAndThirySevenDashes);
                        tokens = Store(tokens, TokenKind.BasicBlockTableCenter);
                        start += 137;
                        continue;

                    case TokenKind.BasicBlockTableCenter:
                    ReturnBasicBlockTableRow:
                        tokens = ParseBasicBlockTableRow(ref start, tokens);
                        continue;

                    case TokenKind.BasicBlockILRangeEndInTable:
                    case TokenKind.BasicBlockJumpTargetKindInTable:
                    case TokenKind.BasicBlockFlagInTable:
                        switch (start[0])
                        {
                            case '-':
                                Assert.Equal(start, Token.OneHundredAndThirySevenDashes);
                                tokens = Store(tokens, TokenKind.BasicBlockTableFooter);
                                start += 137;
                                continue;
                            default: goto ReturnBasicBlockTableRow;
                        }
                    #endregion

                    #region Handling of basic block detalization top header
                    case TokenKind.TwelveDashes when start[0] is 'B':
                        tokens = ParseBasicBlockDetalizationTopHeader(ref start, tokens);
                        continue;
                    #endregion

                    #region Handling of basic block detalization inner header
                    case TokenKind.FourStars:
                        tokens = ParseBasicBlockDetalizationInnerHeader(ref start, tokens);
                        continue;
                    #endregion

                    #region Handling of basic block detalization
                    case TokenKind.StatementDetalizationState:
                    case TokenKind.StatementILRangeEnd:
                    StartGenTreeNodeDetalizationRow:
                        switch (*start)
                        {
                            case '[' or 'N':
                                tokens = ParseGenTreeNodeDetalization(ref start, tokens);
                                continue;
                            default: goto UnstructuredData;
                        }

                    case TokenKind.GenTreeNodeType:
                        switch (startCopy[0])
                        {
                            case '(':
                                switch (startCopy[1])
                                {
                                    case 'i':
                                        switch (startCopy[3])
                                        {
                                            default:
                                                Assert.Equal(startCopy, "(init)");
                                                kind = TokenKind.GenTreeNodeIsInit;
                                                rawWidth = "(init)".Length;
                                                break;
                                        }
                                        break;
                                    default:
                                        Assert.Equal(startCopy, "(copy)");
                                        kind = TokenKind.GenTreeNodeIsCopy;
                                        rawWidth = "(copy)".Length;
                                        break;
                                }
                                break;
                            default: goto StartGenTreeNodeDetalizationRow;
                        }
                        break;

                    case TokenKind.GenTreeNodeLocalVariableTemporaryNumber:
                    case TokenKind.GenTreeNodeLocalVariableArgumentNumber:
                        switch (startCopy[0])
                        {
                            case 'd':
                                Assert.FormatEqual(startCopy, "d:0");
                                kind = TokenKind.GenTreeNodeDNumber;
                                rawValue = IntegersParser.ParseGenericInteger(startCopy + "d:".Length, out rawWidth);
                                rawWidth += "d:".Length;
                                break;
                            case 'u':
                                Assert.FormatEqual(startCopy, "u:0");
                                kind = TokenKind.GenTreeNodeUNumber;
                                rawValue = IntegersParser.ParseGenericInteger(startCopy + "u:".Length, out rawWidth);
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
                    case TokenKind.GenTreeNodeTypeName:
                    case TokenKind.GenTreeNodeMethodName:
                    case TokenKind.GenTreeNodeMethodHandle:
                    case TokenKind.GenTreeNodeHelperMethod:
                        goto StartGenTreeNodeDetalizationRow;
                    #endregion

                    #region Handling of unstructured data
                    default:
                    UnstructuredData:
                        switch (startCopy[0])
                        {
                            case '\0': (kind, rawWidth) = (TokenKind.EndOfFile, int.MaxValue); break;
                            case '*':
                                switch (Count.OfLeading(startCopy, '*'))
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
                                switch (Count.OfLeading(startCopy, '-'))
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

                    ReturnUnknown:
                        kind = TokenKind.Unknown;
                        rawWidth = 1;
                        break;
                }

                var token = new Token(kind, rawValue);
                *tokens++ = token;
                start += rawWidth;

                Assert.Dump(token);
            }
        }

        private static Token* ParseBasicBlockTableColumnHeaders(ref char* start, Token* tokens)
        {
            Assert.Equal(start, "BBnum BBid ref try hnd");

            tokens = Store(tokens,
                TokenKind.BasicBlockNumberColumnHeader,
                TokenKind.BasicBlockIdColumnHeader,
                TokenKind.BasicBlockRefCountInTable,
                TokenKind.BasicBlockTryCountInTable,
                TokenKind.BasicBlockHandleCountInTable);

            if (start[23] is 'p')
            {
                Assert.Equal(start + 23, "preds");
                tokens = Store(tokens, TokenKind.BasicBlockPredsColumnHeader);
            }

            Assert.Equal(start + 39, "weight    lp [IL range]     [jump]      [EH region]         [flags]");

            start += 106;

            return Store(tokens,
                TokenKind.BasicBlockWeightColumnHeader,
                TokenKind.BasicBlockILRangeColumnHeader,
                TokenKind.BasicBlockJumpTargetsColumnHeader,
                TokenKind.BasicBlockExceptionHandlingColumnHeader,
                TokenKind.BasicBlockFlagsColumnHeader);
        }

        private static Token* ParseBasicBlockTableRow(ref char* start, Token* tokens)
        {
            tokens = Store(tokens, TokenKind.BasicBlockInTable, PeekBasicBlock(start));
            start += 6;
            Assert.FormatEqual(start, "0000]");
            tokens = Store(tokens, TokenKind.BasicBlockIdInTable, IntegersParser.ParseIntegerFourDigits(start));
            start += 5;
            Assert.FormatEqual(start, "000", valid: ' ');
            tokens = Store(tokens, TokenKind.BasicBlockRefCountInTable, IntegersParser.ParseGenericInteger(start, out _));
            tokens = Store(tokens, TokenKind.BasicBlockTryColumnHeader, 0);
            tokens = Store(tokens, TokenKind.BasicBlockHandleCountInTable, 0);

            start += 10;
            for (int i = 0; start[i] is 'B'; i += 5)
            {
                tokens = Store(tokens, TokenKind.BasicBlockPredInTable, PeekBasicBlock(start + i));
            }

            start += 22;
            tokens = Store(tokens, TokenKind.BasicBlockWeightInTable, (uint)BitConverter.SingleToInt32Bits(IntegersParser.ParseGenericFloat(start, out _)));
            start += 10;
            tokens = Store(tokens, TokenKind.BasicBlockILRangeStartInTable, PeekILRange(start));
            start += 5;
            tokens = Store(tokens, TokenKind.BasicBlockILRangeEndInTable, PeekILRange(start));
            start += 4;
            if (*start is '-')
            {
                Assert.Equal(start, "->");
                tokens = Store(tokens, TokenKind.BasicBlockJumpTargetInTable, PeekBasicBlock(start + 3));
            }
            start += 8;
            if (*start is '(')
            {
                tokens = Store(tokens, TokenKind.BasicBlockJumpTargetKindInTable, (uint)Lexer.ParseBasicBlockJumpTargetKind(start, out _));
            }
            start += 29;
            while (IsNotEndOfLine(start + 1))
            {
                var dbg = new string(start, 0, 100);
                tokens = Store(tokens, TokenKind.BasicBlockFlagInTable, (uint)Lexer.ParseBasicBlockFlag(start, out var width));
                start += width + 1;
            }

            return tokens;
        }

        private static Token* ParseBasicBlockDetalizationTopHeader(ref char* start, Token* tokens)
        {
            static Token* ParseBasicBlockSequence(ref char* start, Token* tokens, TokenKind kind)
            {
                while (*start is 'B')
                {
                    tokens = Store(tokens, kind, PeekBasicBlock(start));
                    start += 5;
                }
                if (*start is '}')
                {
                    start++;
                }

                return tokens;
            }

            tokens = Store(tokens, TokenKind.BasicBlockInTopHeader, PeekBasicBlock(start));
            start += 6;
            tokens = Store(tokens, TokenKind.BasicBlockILRangeStartInTopHeader, PeekILRange(start));
            start += 5;
            tokens = Store(tokens, TokenKind.BasicBlockILRangeEndInTopHeader, PeekILRange(start));
            start += 5;
            if (*start is '-')
            {
                Assert.Equal(start, "-> ");
                start += 3;
                tokens = Store(tokens, TokenKind.BasicBlockJumpTargetInTopHeader, PeekBasicBlock(start));
                start += 5;
            }
            if (*start is '(')
            {
                tokens = Store(tokens, TokenKind.BasicBlockJumpTargetKindInTopHeader, (uint)Lexer.ParseBasicBlockJumpTargetKind(start, out var width));
                start += width + 1;
            }

            Assert.Equal(start, " preds={");
            start += 8;
            tokens = ParseBasicBlockSequence(ref start, tokens, TokenKind.BasicBlockPredInTopHeader);

            Assert.Equal(start, " succs={");
            start += 8;
            tokens = ParseBasicBlockSequence(ref start, tokens, TokenKind.BasicBlockSuccInTopHeader);

            return tokens;
        }

        private static Token* ParseBasicBlockDetalizationInnerHeader(ref char* start, Token* tokens)
        {
            tokens = Store(tokens, TokenKind.BasicBlockInInnerHeader, PeekBasicBlock(start));
            start += 4;
            start = *start is ',' ? start + 2 : SkipEndOfLine(start);
            if (*start is 'S')
            {
                tokens = Store(tokens, TokenKind.Statement, PeekStatement(start));
                start += 9;

                if (*start is '(')
                {
                    tokens = Store(tokens, TokenKind.StatementDetalizationState, (uint)Lexer.ParseStatementDetalizationState(start, out var width));
                    start += width;
                }
                else
                {
                    start += 7;
                    tokens = Store(tokens, TokenKind.StatementILRangeStart, PeekILRange(start));
                    start += 8;
                    tokens = Store(tokens, TokenKind.StatementILRangeEnd, PeekILRange(start));
                    start += 4;
                }
            }

            return tokens;
        }

        private static Token* ParseGenTreeNodeDetalization(ref char* start, Token* tokens)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Token* StoreGenTreeTypeName(ref char* start, Token* tokens)
            {
                Assert.Equal(start - 7, "struct<");
                tokens = Store(tokens, TokenKind.GenTreeNodeTypeName, ParseGenTreeNodeTypeName(start, out var nameWidth).Value);
                start += nameWidth + 1;
                return tokens;
            }

            if (*start is 'N')
            {
                Assert.FormatEqual(start, "N000");
                tokens = Store(tokens, TokenKind.GenTreeLIRNode, IntegersParser.ParseIntegerThreeDigits(start + 1));
                start += 6;
                Assert.FormatEqual(start, "(000,000)", valid: ' ');
                tokens = Store(tokens, TokenKind.GenTreeNodeEstimatedTime, IntegersParser.ParseGenericInteger(start, out _));
                start += 4;
                tokens = Store(tokens, TokenKind.GenTreeNodeEstimatedCost, IntegersParser.ParseGenericInteger(start, out _));
                start += 5;
            }

            Assert.FormatEqual(start, "[000000]");
            start++;
            tokens = Store(tokens, TokenKind.GenTreeNodeId, IntegersParser.ParseIntegerSixDigits(start));
            start += 8;
            tokens = Store(tokens, TokenKind.GenTreeNodeFlags, (uint)Lexer.ParseGenTreeNodeFlags(start));
            start += 13;

            switch (*start)
            {
                case 'p':
                    Assert.Equal(start, "pred");
                    tokens = Store(tokens, TokenKind.GenTreeNodePred, PeekBasicBlock(start + 5));
                    break;
                case 't':
                    Assert.Equal(start, "this");
                    tokens = Store(tokens, TokenKind.GenTreeNodeThisArgumentInfo);
                    var width = 4;
                    goto StoreArgumentInfo;
                case 'a':
                    tokens = Store(tokens, TokenKind.GenTreeNodeArgumentInfo, PeekArgument(start, out width));
                    goto StoreArgumentInfo;
                StoreArgumentInfo:
                    switch (start[width + 1])
                    {
                        case 'S':
                            Assert.Equal(start + width + 1, "SETUP");
                            tokens = Store(tokens, TokenKind.GenTreeNodeArgumentInfoSetup);
                            break;
                        case 'i':
                            Assert.Equal(start + width + 1, "in");
                            tokens = Store(tokens, TokenKind.GenTreeNodeArgumentInfoRegister, (uint)Lexer.ParseRegister(start + width + 4, out _));
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            start += 13;
            var padding = 0;
            while (*start is not '*')
            {
                start += 3;
                padding++;
            }
            Assert.Equal(start, "*  ");
            start += 3;
            tokens = Store(tokens, TokenKind.GenTreeNodePadding, padding);
            var kind = Lexer.ParseGenTreeNodeKind(start, out var kindWidth);
            tokens = Store(tokens, TokenKind.GenTreeNodeKind, (uint)kind);
            start += kindWidth + 1;

            switch (*start)
            {
                case 'h' when kind is GenTreeNodeKind.CNS_INT or GenTreeNodeKind.CNS_LNG:
                    Assert.Equal(start - 1, "(h)");
                    start += 1;
                    break;
                case 'n' when kind is GenTreeNodeKind.CALL:
                    Assert.Equal(start, "nullcheck");
                    tokens = Store(tokens, TokenKind.GenTreeNodeNullcheck);
                    start += 5;
                    break;
                default:
                    break;
            }

            start += Math.Max(0, 9 - kindWidth);
            tokens = Store(tokens, TokenKind.GenTreeNodeType, (uint)Lexer.ParseGenTreeNodeType(start, out _));
            start += 7;

            switch (kind)
            {
                case GenTreeNodeKind.CNS_INT:
                case GenTreeNodeKind.CNS_LNG:
                    if (start[-10] is 'h')
                    {
                        Assert.FormatEqual(start, "0x00000000", hex: true);
                        tokens = Store(tokens, TokenKind.GenTreeNodeConstantHandle);
                        start += 11;
                        tokens = Store(tokens, TokenKind.GenTreeNodeConstantHandleKind, (uint)Lexer.ParseGenTreeConstantHandleKind(start, out var handleWidth));
                        start += handleWidth;
                    }
                    else
                    {
                        if (start[1] is 'x')
                        {
                            Assert.FormatEqual(start, "0x000000000000", hex: true);
                            tokens = Store(tokens, TokenKind.GenTreeNodeLargeIntegerConstant, ParseLargeHexInteger(start + 2, out _).Value);
                            start += 14;
                        }
                        else
                        {
                            if (*start is 'n')
                            {
                                Assert.Equal(start, "null");
                                tokens = Store(tokens, TokenKind.GenTreeNodeIntegerConstantNull);
                                start += 4;
                            }
                            else
                            {
                                tokens = Store(tokens, TokenKind.GenTreeNodeIntegerConstant, IntegersParser.ParseGenericInteger(start, out var integerWidth));
                                start += integerWidth;
                            }
                        }
                    }
                    break;
                case GenTreeNodeKind.CALL:
                    if (start[-9] is 'p')
                    {
                        Assert.Equal(start - 12, "help");
                        tokens = Store(tokens, TokenKind.GenTreeNodeHelperMethod, (uint)Lexer.ParseRyuJitHelperMethod(start, out var methodWidth));
                        start += methodWidth;
                    }
                    else
                    {
                        tokens = Store(tokens, TokenKind.GenTreeNodeMethodName, ParseGenTreeNodeMethodName(start, out var methodWidth).Value);
                        start += methodWidth;
                        if (start[1] is '(')
                        {
                            Assert.Equal(start, " (exactContextHnd=0x");
                            start += 20;
                            Assert.FormatEqual(start, "0000000000000000)", hex: true);
                            tokens = Store(tokens, TokenKind.GenTreeNodeMethodHandle, ParseLargeHexInteger(start, out _).Value);
                            start += 17;
                        }
                    }
                    break;
                case GenTreeNodeKind.OBJ:
                    tokens = StoreGenTreeTypeName(ref start, tokens);
                    break;
                case GenTreeNodeKind.LCL_VAR:
                    if (tokens[-1].GenTreeNodeType is GenTreeNodeType.Struct)
                    {
                        tokens = StoreGenTreeTypeName(ref start, tokens);
                        start += 1;
                    }
                    Assert.FormatEqual(start, "V00");
                    start += 1;
                    tokens = Store(tokens, TokenKind.GenTreeNodeLocalVariableIndex, IntegersParser.ParseIntegerTwoDigits(start));
                    start += 3;
                    switch (*start)
                    {
                        case 'a':
                            tokens = Store(tokens, TokenKind.GenTreeNodeLocalVariableArgumentNumber, PeekArgument(start, out var argumentWidth));
                            start += argumentWidth;
                            break;
                        default:
                            Assert.FormatEqual(start, "tmp0");
                            start += 3;
                            tokens = Store(tokens, TokenKind.GenTreeNodeLocalVariableTemporaryNumber, IntegersParser.ParseGenericInteger(start, out var tempWidth));
                            start += tempWidth;
                            break;
                    }
                    break;
                case GenTreeNodeKind.RET_EXPR:
                    Assert.FormatEqual(start - 1, "(inl return from call [000000])");
                    start += 22;
                    tokens = Store(tokens, TokenKind.GenTreeNodeIsInlineReturn, IntegersParser.ParseIntegerSixDigits(start));
                    start += 8;
                    break;
                default:
                    break;
            }

            return tokens;
        }

        private static int PeekILRange(char* start)
        {
            switch (*start)
            {
                case '?':
                    Assert.Equal(start, "???");
                    return -1;
                default:
                    return (int)IntegersParser.ParseHexIntegerThreeDigits(start);
            }
        }

        private static uint PeekBasicBlock(char* start)
        {
            Assert.FormatEqual(start, "BB00");
            return IntegersParser.ParseIntegerTwoDigits(start + 2);
        }

        private static uint PeekStatement(char* start)
        {
            Assert.FormatEqual(start, "STMT00000");
            return IntegersParser.ParseIntegerFiveDigits(start + 4);
        }

        private static int PeekArgument(char* start, out int width)
        {
            Assert.Equal(start, "arg");
            var index = IntegersParser.ParseGenericInteger(start + 3, out width);
            width += 3;
            return (int)index;
        }

        public static TypeNameHandle ParseGenTreeNodeTypeName(char* start, out int width)
        {
            Assert.NotEqual(start, "-");

            // Technically UB
            var index = new Span<char>(start, int.MaxValue).IndexOf('>');

            width = index;

            // We do not have an implementation for string pools yet
            return new TypeNameHandle(0);
        }

        private static MethodNameHandle ParseGenTreeNodeMethodName(char* start, out int width)
        {
            // Technically UB
            var index = new Span<char>(start, int.MaxValue).IndexOf(' ');

            width = index;

            // We do not have an implementation for string pools yet
            return new MethodNameHandle(0);
        }

        private static LargeIntegerHandle ParseLargeHexInteger(char* start, out int width)
        {
            width = 0;
            while (start[width] is
                >= '0' and <= '9' or
                >= 'a' and <= 'f' or
                >= 'A' and <= 'F')
            {
                width++;
            }

            return new LargeIntegerHandle(0);
        }
    }
}