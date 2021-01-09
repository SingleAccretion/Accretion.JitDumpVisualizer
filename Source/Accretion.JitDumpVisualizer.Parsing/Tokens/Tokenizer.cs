using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using Accretion.JitDumpVisualizer.Parsing.Auxiliaries.Logging;
using Accretion.JitDumpVisualizer.Parsing.IO;
using Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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

        internal static void NextBlock(ref char* start, ref Token* tokens)
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
                if (OperatingSystem.IsWindows())
                {
                    Assert.NotEqual(start, "\n");
                }
                if (IsEndOfLine(start))
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
                        tokens = Next(tokens, TokenKind.BasicBlockTableCenter);
                        start += 137;
                        continue;

                    case TokenKind.BasicBlockTableCenter:
                    ReturnBasicBlockTableRow:
                        tokens = Pop(ParseBasicBlockTableRow(start, tokens), out start);
                        continue;

                    case TokenKind.BasicBlockILRangeEndInTable:
                    case TokenKind.BasicBlockJumpTargetKindInTable:
                    case TokenKind.BasicBlockFlagInTable:
                        switch (start[0])
                        {
                            case '-':
                                Assert.Equal(start, Token.OneHundredAndThirySevenDashes);
                                tokens = Next(tokens, TokenKind.BasicBlockTableFooter);
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
                    case TokenKind.BasicBlockInInnerHeader:
                    StartGenTreeNodeDetalization:
                        switch (*start)
                        {
                            case '[' or 'N':
                                tokens = Pop(ParseGenTreeNodeDetalization(start, tokens), out start);
                                continue;
                            default: goto UnstructuredData;
                        }

                    // Potential enhancement: emit a shadow token and switch on that
                    case TokenKind.GenTreeNodeUseNumber:
                    case TokenKind.GenTreeNodeDefinitionNumber:
                    case TokenKind.GenTreeNodeAssignmentIsCopy:
                    case TokenKind.GenTreeNodeIsCopy:
                    case TokenKind.GenTreeNodeIsInlineReturn:
                    case TokenKind.GenTreeNodeType:
                    case TokenKind.GenTreeNodeCastType:
                    case TokenKind.GenTreeNodeTypeName:
                    case TokenKind.GenTreeNodeMethodName:
                    case TokenKind.GenTreeNodeFieldName:
                    case TokenKind.GenTreeNodeMethodHandle:
                    case TokenKind.GenTreeNodeHelperMethod:
                    case TokenKind.GenTreeNodeIntegerConstant:
                    case TokenKind.GenTreeNodeIntegerConstantNull:
                    case TokenKind.GenTreeNodeLargeIntegerConstant:
                    case TokenKind.GenTreeNodeConstantIconHandle:
                    case TokenKind.GenTreeNodeLastUse:
                    case TokenKind.GenTreeNodeLocalVariableTemporaryNumber:
                        goto StartGenTreeNodeDetalization;
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

            tokens = Next(tokens,
                TokenKind.BasicBlockNumberColumnHeader,
                TokenKind.BasicBlockIdColumnHeader,
                TokenKind.BasicBlockRefCountInTable,
                TokenKind.BasicBlockTryCountInTable,
                TokenKind.BasicBlockHandleCountInTable);

            if (start[23] is 'p')
            {
                Assert.Equal(start + 23, "preds");
                tokens = Next(tokens, TokenKind.BasicBlockPredsColumnHeader);
            }

            Assert.Equal(start + 39, "weight    lp [IL range]     [jump]      [EH region]         [flags]");

            start += 106;

            return Next(tokens,
                TokenKind.BasicBlockWeightColumnHeader,
                TokenKind.BasicBlockILRangeColumnHeader,
                TokenKind.BasicBlockJumpTargetsColumnHeader,
                TokenKind.BasicBlockExceptionHandlingColumnHeader,
                TokenKind.BasicBlockFlagsColumnHeader);
        }

        internal static Token* ParseBasicBlockTableRow(char* start, Token* tokens)
        {
            tokens = Next(tokens, TokenKind.BasicBlockInTable, PeekBasicBlock(start));
            start += 6;
            Assert.FormatEqual(start, "0000]");
            tokens = Next(tokens, TokenKind.BasicBlockIdInTable, IntegersParser.ParseIntegerFourDigits(start));
            start += 5;
            Assert.FormatEqual(start, "000", valid: ' ');
            tokens = Next(tokens, TokenKind.BasicBlockRefCountInTable, IntegersParser.ParseGenericInteger(start, out _));
            tokens = Next(tokens, TokenKind.BasicBlockTryColumnHeader, 0);
            tokens = Next(tokens, TokenKind.BasicBlockHandleCountInTable, 0);

            start += 10;
            for (int i = 0; start[i] is 'B'; i += 5)
            {
                tokens = Next(tokens, TokenKind.BasicBlockPredInTable, PeekBasicBlock(start + i));
            }

            start += 22;
            tokens = Next(tokens, TokenKind.BasicBlockWeightInTable, (uint)BitConverter.SingleToInt32Bits(IntegersParser.ParseGenericFloat(start, out _)));
            start += 10;
            tokens = Next(tokens, TokenKind.BasicBlockILRangeStartInTable, PeekILRange(start));
            start += 5;
            tokens = Next(tokens, TokenKind.BasicBlockILRangeEndInTable, PeekILRange(start));
            start += 4;
            if (*start is '-')
            {
                Assert.Equal(start, "->");
                tokens = Next(tokens, TokenKind.BasicBlockJumpTargetInTable, PeekBasicBlock(start + 3));
            }
            start += 8;
            if (*start is '(')
            {
                tokens = Next(tokens, TokenKind.BasicBlockJumpTargetKindInTable, (uint)Lexer.ParseBasicBlockJumpTargetKind(start, out _));
            }
            start += 29;
            while (IsNotEndOfLine(start + 1))
            {
                tokens = Next(tokens, TokenKind.BasicBlockFlagInTable, (uint)Lexer.ParseBasicBlockFlag(start, out var width));
                start += width + 1;
            }

            return Push(tokens, start);
        }

        internal static Token* ParseBasicBlockTableRowManualCalls(char* start, Token* tokens)
        {
            var returnAddress = -1;
            nint i = 0;
        Return:
            switch (returnAddress)
            {
                case -1:
                    tokens = NextKind(tokens, TokenKind.BasicBlockInTable);

                    returnAddress = 0;
                    goto PeekBasicBlock;
                case 0:
                    start += 6;
                    tokens = NextKind(tokens, TokenKind.BasicBlockIdInTable);

                    returnAddress = 1;
                    goto ParseIntegerFourDigits;
                case 1:
                    start += 5;
                    tokens = NextKind(tokens, TokenKind.BasicBlockRefCountInTable);

                    returnAddress = 2;
                    goto ParseGenericInteger;
                case 2:
                    tokens = Next(tokens, TokenKind.BasicBlockTryColumnHeader, 0);
                    tokens = Next(tokens, TokenKind.BasicBlockHandleCountInTable, 0);
                    start += 7;

                    returnAddress = 3;
                    goto case 3;
                case 3:
                    if (start[i] is 'B')
                    {
                        tokens = NextKind(tokens, TokenKind.BasicBlockPredInTable);
                        i += 5;
                        goto PeekBasicBlock;
                    };

                    start += 22;
                    tokens = NextKind(tokens, TokenKind.BasicBlockWeightInTable);

                    returnAddress = 4;
                    goto ParseGenericFloat;
                case 4:
                    start += 10;
                    tokens = NextKind(tokens, TokenKind.BasicBlockILRangeStartInTable);

                    returnAddress = 5;
                    goto PeekILRange;
                case 5:
                    start += 5;
                    tokens = NextKind(tokens, TokenKind.BasicBlockILRangeEndInTable);

                    returnAddress = 6;
                    goto PeekILRange;
                case 6:
                    start += 4;
                    if (*start is '-')
                    {
                        start += 3;
                        tokens = NextKind(tokens, TokenKind.BasicBlockJumpTargetInTable);

                        returnAddress = 7;
                        goto PeekBasicBlock;
                    }
                    goto Continue;
                case 7:
                    start -= 3;
                Continue:
                    start += 8;
                    if (*start is '(')
                    {
                        tokens = NextKind(tokens, TokenKind.BasicBlockJumpTargetKindInTable);

                        returnAddress = 8;
                        goto ParseBasicBlockJumpTargetKindIgnoreWidth;
                    }
                    goto case 8;
                case 8:
                    start += 28;
                    goto case 9;
                case 9:
                    start += 1;
                    if (IsNotEndOfLine(start))
                    {
                        tokens = NextKind(tokens, TokenKind.BasicBlockFlagInTable);

                        returnAddress = 9;
                        goto ParseBasicBlockFlag;
                    }
                    goto default;
                default:
                    return Push(tokens, start);
            }
        ParseIntegerFourDigits:
            var characters = Sse2.LoadScalarVector128((long*)start).AsInt16();
            var values = Sse2.Subtract(characters, Vector128.Create('0').AsInt16());
            var result = Sse2.MultiplyAddAdjacent(values, Vector128.Create(1000, 100, 10, 1, 0, 0, 0, 0)).AsUInt32();

            tokens = NextValue(tokens, result.GetElement(0) + result.GetElement(1));
            goto Return;
        ParseGenericInteger:
            while (*start is ' ')
            {
                start++;
            }

            nint digitCount = 0;
            var digits = stackalloc uint[32];
            while ((uint)(*start - '0') is <= 9 and var digit)
            {
                digits[digitCount] = digit;
                digitCount++;
                start++;
            }

            var number = 0u;
            var multiplier = 1u;
            for (nint j = digitCount - 1; j >= 0; j--)
            {
                number += digits[j] * multiplier;
                multiplier *= 10;
            }

            tokens = NextValue(tokens, number);
            goto Return;
        PeekBasicBlock:
            var d1 = (uint)start[2] - '0';
            var d2 = (uint)start[3] - '0';

            tokens = NextValue(tokens, d1 * 10 + d2);
            goto Return;
        ParseGenericFloat:
            i = 0;
            var fpDigits = stackalloc float[16];
            digitCount = 0;
            nint dotPosition = 0;
            while (true)
            {
                switch (start[i])
                {
                    case <= '9' and >= '0' and var digitChar:
                        fpDigits[i] = digitChar - '0';
                        digitCount++;
                        break;
                    case '.':
                        dotPosition = i;
                        break;
                    default:
                        if (dotPosition is 0)
                        {
                            dotPosition = digitCount;
                        }
                        var fpNumber = 0f;
                        var fpMultiplier = 1f;
                        for (i = dotPosition - 1; i >= 0; i--)
                        {
                            fpNumber += fpDigits[i] * fpMultiplier;
                            fpMultiplier *= 10f;
                        }
                        fpMultiplier = 0.1f;
                        for (i = dotPosition + 1; i <= digitCount; i++)
                        {
                            fpNumber += fpDigits[i] * fpMultiplier;
                            fpMultiplier *= 0.1f;
                        }
                        tokens = NextValue(tokens, (uint)BitConverter.SingleToInt32Bits(fpNumber));
                        goto Return;
                }

                i++;
            }
        PeekILRange:
            switch (*start)
            {
                case '?':
                    Assert.Equal(start, "???");
                    tokens = NextValue(tokens, unchecked((uint)-1));
                    goto Return;
                default:
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    static uint ToHexDigit(char ch) => ch switch
                    {
                        >= 'a' and <= 'f' => (uint)ch - 'a',
                        >= 'A' and <= 'F' => (uint)ch - 'A',
                        _ => (uint)ch - '0'
                    };

                    d1 = ToHexDigit(start[0]);
                    d2 = ToHexDigit(start[1]);
                    var d3 = ToHexDigit(start[2]);

                    tokens = NextValue(tokens, d1 * 16 * 16 + d2 * 16 + d3);
                    goto Return;
            }
        ParseBasicBlockJumpTargetKindIgnoreWidth:
            var kind = (start[2]) switch
            {
                'c' => BasicBlockJumpTargetKind.Conditional,
                'l' => BasicBlockJumpTargetKind.Always,
                'e' => BasicBlockJumpTargetKind.Return,
                _ => BasicBlockJumpTargetKind.Conditional
            };

            tokens = NextValue(tokens, (uint)kind);
            goto Return;
        ParseBasicBlockFlag:
            var flag = (*start) switch
            {
                'i' => (start[1]) switch
                {
                    'd' => Lexer.Result(BasicBlockFlag.IdxLen, "idxlen", ref start),
                    'n' => Lexer.Result(BasicBlockFlag.Internal, "internal", ref start),
                    _ => Lexer.Result(BasicBlockFlag.I, "i", ref start)
                },
                'l' => Lexer.Result(BasicBlockFlag.Label, "label", ref start),
                't' => Lexer.Result(BasicBlockFlag.Target, "target", ref start),
                'h' => Lexer.Result(BasicBlockFlag.HasCall, "hascall", ref start),
                'n' => (start[1]) switch
                {
                    'e' => Lexer.Result(BasicBlockFlag.NewObj, "newobj", ref start),
                    _ => Lexer.Result(BasicBlockFlag.NullCheck, "nullcheck", ref start)
                },
                'g' => Lexer.Result(BasicBlockFlag.GCSafe, "gcsafe", ref start),
                _ => Lexer.Result(BasicBlockFlag.LIR, "LIR", ref start)
            };

            tokens = NextValue(tokens, (uint)flag);
            goto Return;
        }

        private static Token* ParseBasicBlockDetalizationTopHeader(ref char* start, Token* tokens)
        {
            static Token* ParseBasicBlockSequence(ref char* start, Token* tokens, TokenKind kind)
            {
                while (*start is 'B')
                {
                    tokens = Next(tokens, kind, PeekBasicBlock(start));
                    start += 5;
                }
                if (*start is '}')
                {
                    start++;
                }

                return tokens;
            }

            tokens = Next(tokens, TokenKind.BasicBlockInTopHeader, PeekBasicBlock(start));
            start += 6;
            tokens = Next(tokens, TokenKind.BasicBlockILRangeStartInTopHeader, PeekILRange(start));
            start += 5;
            tokens = Next(tokens, TokenKind.BasicBlockILRangeEndInTopHeader, PeekILRange(start));
            start += 5;
            if (*start is '-')
            {
                Assert.Equal(start, "-> ");
                start += 3;
                tokens = Next(tokens, TokenKind.BasicBlockJumpTargetInTopHeader, PeekBasicBlock(start));
                start += 5;
            }
            if (*start is '(')
            {
                tokens = Next(tokens, TokenKind.BasicBlockJumpTargetKindInTopHeader, (uint)Lexer.ParseBasicBlockJumpTargetKind(start, out var width));
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
            tokens = Next(tokens, TokenKind.BasicBlockInInnerHeader, PeekBasicBlock(start));
            start += 4;
            start = *start is ',' ? start + 2 : SkipEndOfLine(start);
            if (*start is 'S')
            {
                tokens = Next(tokens, TokenKind.Statement, PeekStatement(start));
                start += 9;

                if (*start is '(')
                {
                    tokens = Next(tokens, TokenKind.StatementDetalizationState, (uint)Lexer.ParseStatementDetalizationState(start, out var width));
                    start += width;
                }
                else
                {
                    start += 7;
                    tokens = Next(tokens, TokenKind.StatementILRangeStart, PeekILRange(start));
                    start += 8;
                    tokens = Next(tokens, TokenKind.StatementILRangeEnd, PeekILRange(start));
                    start += 4;
                }
            }

            return tokens;
        }

        private static Token* ParseGenTreeNodeDetalization(char* start, Token* tokens)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Token* StoreGenTreeTypeName(char* start, Token* tokens)
            {
                Assert.Equal(start - 7, "struct<");
                tokens = Next(tokens, TokenKind.GenTreeNodeTypeName, LexGenTreeNodeTypeName(start, out var nameWidth).Value);
                start += nameWidth + 1;

                return Push(tokens, start);
            }

            if (*start is 'N')
            {
                Assert.FormatEqual(start, "N000");
                tokens = Next(tokens, TokenKind.GenTreeNodeSequenceNumber, IntegersParser.ParseIntegerThreeDigits(start + 1));
                start += 6;
                Assert.FormatEqual(start - 1, "(000,000)", valid: ' ');
                tokens = Next(tokens, TokenKind.GenTreeNodeEstimatedTime, IntegersParser.ParseGenericInteger(start, out _));
                start += 4;
                tokens = Next(tokens, TokenKind.GenTreeNodeEstimatedCost, IntegersParser.ParseGenericInteger(start, out _));
                start += 5;
            }

            Assert.FormatEqual(start, "[000000]");
            start++;
            tokens = Next(tokens, TokenKind.GenTreeNodeId, IntegersParser.ParseIntegerSixDigits(start));
            start += 8;
            tokens = Next(tokens, TokenKind.GenTreeNodeFlags, (uint)Lexer.ParseGenTreeNodeFlags(start));
            start += 13;

            switch (*start)
            {
                case 'p':
                    Assert.Equal(start, "pred");
                    tokens = Next(tokens, TokenKind.GenTreeNodePred, PeekBasicBlock(start + 5));
                    break;
                case 't':
                    Assert.Equal(start, "this");
                    tokens = Next(tokens, TokenKind.GenTreeNodeThisArgumentInfo);
                    var width = 4;
                    goto StoreArgumentInfo;
                case 'a':
                    tokens = Next(tokens, TokenKind.GenTreeNodeArgumentInfo, PeekArgument(start, out width));
                    goto StoreArgumentInfo;
                StoreArgumentInfo:
                    switch (start[width + 1])
                    {
                        case 'S':
                            Assert.Equal(start + width + 1, "SETUP");
                            tokens = Next(tokens, TokenKind.GenTreeNodeArgumentInfoSetup);
                            break;
                        case 'i':
                            Assert.Equal(start + width + 1, "in");
                            tokens = Next(tokens, TokenKind.GenTreeNodeArgumentInfoRegister, (uint)Lexer.ParseRegister(start + width + 4, out _));
                            break;
                        default:
                            break;
                    }
                    break;
                default: break;
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
            tokens = Next(tokens, TokenKind.GenTreeNodePadding, padding);
            var kind = Lexer.ParseGenTreeNodeKind(start, out var kindWidth);
            tokens = Next(tokens, TokenKind.GenTreeNodeKind, (uint)kind);
            start += kindWidth + 1;

            switch (*start)
            {
                case 'h' when kind is GenTreeNodeKind.CNS_INT or GenTreeNodeKind.CNS_LNG:
                    Assert.Equal(start - 1, "(h)");
                    start += 1;
                    break;
                // The handling here is incomplete
                // We're missing ind, stub, r2r_ind, unmanaged calls...
                case 'n' when kind is GenTreeNodeKind.CALL:
                    Assert.Equal(start, "nullcheck");
                    tokens = Next(tokens, TokenKind.GenTreeNodeNullcheck);
                    start += 5;
                    break;
                default: break;
            }

            start += Math.Max(0, 9 - kindWidth);
            tokens = Next(tokens, TokenKind.GenTreeNodeType, (uint)Lexer.ParseGenTreeNodeType(start, out _));
            start += 7;

            switch (kind)
            {
                case GenTreeNodeKind.CNS_INT:
                    if (*start is 'n')
                    {
                        Assert.Equal(start, "null");
                        tokens = Next(tokens, TokenKind.GenTreeNodeIntegerConstantNull);
                        start += 4;
                        break;
                    }

                    if (start[-10] is 'h')
                    {
                        Assert.FormatEqual(start, "0x00000000", hex: true);
                        tokens = Next(tokens, TokenKind.GenTreeNodeConstantHandle);
                        start += 11;
                        // Note: this doesn't handle literal constant strings: "Constant string"
                        tokens = Next(tokens, TokenKind.GenTreeNodeConstantIconHandle, (uint)Lexer.ParseGenTreeNodeConstantIconHandle(start, out var handleWidth));
                        start += handleWidth;
                    }
                    else
                    {
                        if (start[1] is 'x')
                        {
                            Assert.FormatEqual(start, "0x000000000000", hex: true);
                            tokens = Next(tokens, TokenKind.GenTreeNodeLargeIntegerConstant, LexLargeHexInteger(start + 2, out _).Value);
                            start += 14;
                        }
                        else
                        {
                            // These are values between -1000 and 1000
                            tokens = Next(tokens, TokenKind.GenTreeNodeIntegerConstant, IntegersParser.ParseGenericInteger(start, out var integerWidth));
                            start += integerWidth;
                        }
                    }

                    switch (start[1])
                    {
                        case 'f':
                            Assert.Equal(start + 1, "field offset");
                            start += 13;
                            tokens = Next(tokens, TokenKind.GenTreeNodeIntegerConstantIsFieldSequenceOffset);
                            break;
                        case 'v':
                            Assert.Equal(start + 1, "vector element count");
                            tokens = Next(tokens, TokenKind.GenTreeNodeIntegerConstantIsVectorElementCount);
                            start += 20;
                            break;
                        case 'r':
                            Assert.Equal(start + 1, "reuse reg val");
                            tokens = Next(tokens, TokenKind.GenTreeNodeIntegerConstantReuseRegisterValue);
                            start += 13;
                            break;
                        default: break;
                    }

                    if (start[1] is 'F')
                    {
                        start += 1;
                        tokens = Pop(ParseGenTreeNodeFieldSequence(start, tokens), out start);
                    }
                    break;
                case GenTreeNodeKind.CALL:
                    if (start[-9] is 'p')
                    {
                        Assert.Equal(start - 12, "help");
                        tokens = Next(tokens, TokenKind.GenTreeNodeHelperMethod, (uint)Lexer.ParseRyuJitHelperMethod(start, out var methodWidth));
                        start += methodWidth;
                    }
                    else
                    {
                        tokens = Next(tokens, TokenKind.GenTreeNodeMethodName, LexGenTreeNodeMethodName(start, out var methodWidth).Value);
                        start += methodWidth;
                        if (start[1] is '(')
                        {
                            Assert.Equal(start, " (exactContextHnd=0x");
                            start += 20;
                            Assert.FormatEqual(start, "0000000000000000)", hex: true);
                            tokens = Next(tokens, TokenKind.GenTreeNodeMethodHandle, LexLargeHexInteger(start, out _).Value);
                            start += 17;
                        }
                    }
                    break;
                case GenTreeNodeKind.OBJ:
                    tokens = Pop(StoreGenTreeTypeName(start, tokens), out start);
                    break;
                case GenTreeNodeKind.PHI_ARG:
                case GenTreeNodeKind.LCL_VAR:
                    if (tokens[-1].GenTreeNodeType is GenTreeNodeType.Struct)
                    {
                        tokens = Pop(StoreGenTreeTypeName(start, tokens), out start);
                        if (*start is '(')
                        {
                            switch (start[1])
                            {
                                case 'A':
                                    Assert.Equal(start, "(AX)");
                                    tokens = Next(tokens, TokenKind.GenTreeNodeLocalVariableIsAddressExposed);
                                    start += 4;
                                    break;
                                case 'U':
                                    Assert.Equal(start, "(U)");
                                    tokens = Next(tokens, TokenKind.GenTreeNodeLocalVariableIsUnusedStruct);
                                    start += 3;
                                    break;
                                default:
                                    if (start[2] is '?')
                                    {
                                        Assert.Equal(start, "(P?!)");
                                        tokens = Next(tokens, TokenKind.GenTreeNodeLocalVariableIsPromotedStructBeingRewritten);
                                        start += 5;
                                    }
                                    else
                                    {
                                        Assert.Equal(start, "(P)");
                                        tokens = Next(tokens, TokenKind.GenTreeNodeLocalVariableIsPromotedStruct);
                                        start += 3;
                                    }
                                    break;
                            }

                        }
                        start += 1;
                    }
                    Assert.FormatEqual(start, "V00");
                    start += 1;
                    tokens = Next(tokens, TokenKind.GenTreeNodeLocalVariableIndex, IntegersParser.ParseIntegerTwoDigits(start));
                    start += 3;
                    switch (*start)
                    {
                        case 'a':
                            tokens = Next(tokens, TokenKind.GenTreeNodeLocalVariableArgumentNumber, PeekArgument(start, out _));
                            start += 13;
                            break;
                        default:
                            Assert.FormatEqual(start, "tmp0");
                            start += 3;
                            tokens = Next(tokens, TokenKind.GenTreeNodeLocalVariableTemporaryNumber, IntegersParser.ParseGenericInteger(start, out _));
                            start += 10;
                            break;
                    }
                    switch (*start)
                    {
                        case 'd':
                            Assert.FormatEqual(start, "d:0");
                            start += 2;
                            tokens = Next(tokens, TokenKind.GenTreeNodeDefinitionNumber, IntegersParser.ParseGenericInteger(start, out var definitionWidth));
                            start += definitionWidth;
                            break;
                        case 'u':
                            Assert.FormatEqual(start, "u:0");
                            start += 2;
                            tokens = Next(tokens, TokenKind.GenTreeNodeUseNumber, IntegersParser.ParseGenericInteger(start, out var useWidth));
                            start += useWidth;
                            if (start[1] is '(')
                            {
                                Assert.Equal(start + 1, "(last use)");
                                tokens = Next(tokens, TokenKind.GenTreeNodeLastUse);
                                start += 11;
                            }
                            break;
                        default: break;
                    }
                    if (start[1] is 'Z')
                    {
                        Assert.Equal(start + 1, "Zero");
                        start += 6;
                        tokens = Next(tokens, TokenKind.GenTreeNodeLocalVariableHasZeroOffsetFieldSequence);
                        tokens = Pop(ParseGenTreeNodeFieldSequence(start, tokens), out start);
                    }
                    break;
                case GenTreeNodeKind.RET_EXPR:
                    Assert.FormatEqual(start - 1, "(inl return from call [000000])");
                    start += 22;
                    tokens = Next(tokens, TokenKind.GenTreeNodeIsInlineReturn, IntegersParser.ParseIntegerSixDigits(start));
                    start += 8;
                    break;
                case GenTreeNodeKind.FIELD:
                    tokens = Next(tokens, TokenKind.GenTreeNodeFieldName, LexGenTreeNodeFieldName(start, out var nameWidth, '\r', '\n').Value);
                    start += nameWidth;
                    break;
                case GenTreeNodeKind.ASG:
                    switch (start[1])
                    {
                        case 'i':
                            Assert.Equal(start, "(init)");
                            tokens = Next(tokens, TokenKind.GenTreeNodeAssignmentIsCopy);
                            start += 6;
                            break;
                        case 'c':
                            Assert.Equal(start, "(copy)");
                            tokens = Next(tokens, TokenKind.GenTreeNodeAssignmentIsCopy);
                            start += 6;
                            break;
                        default: goto Backtrack;
                    }
                    break;
                case GenTreeNodeKind.CAST:
                    while (start[-3] is '<')
                    {
                        Assert.Equal(start - 3, "<-");
                        tokens = Next(tokens, TokenKind.GenTreeNodeCastType, (uint)Lexer.ParseGenTreeNodeType(start, out var typeWidth));
                        start += typeWidth + 4;
                    }
                    start -= 4;
                    break;
                default:
                Backtrack:
                    start -= 1;
                    break;
            }

            if (OperatingSystem.IsWindows())
            {
                Assert.Equal(start, "\r");
            }
            else
            {
                Assert.Equal(start, "\n");
            }

            return Push(tokens, start);
        }

        private static Token* ParseGenTreeNodeFieldSequence(char* start, Token* tokens)
        {
            Assert.Equal(start, "Fseq[");
            start += 5;
            while (start[-2] is not ']')
            {
                var name = LexGenTreeNodeFieldName(start, out var width, ',', ']').Value;
                tokens = Next(tokens, TokenKind.GenTreeNodeFieldName, name);
                start += width + 2;
            }
            start -= 1;
            Assert.Equal(start - 1, "]");

            return Push(tokens, start);
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

        private static TypeNameHandle LexGenTreeNodeTypeName(char* start, out int width)
        {
            Assert.NotEqual(start, "-");

            // Technically UB
            var index = new Span<char>(start, int.MaxValue).IndexOf('>');

            width = index;

            // We do not have an implementation for string pools yet
            return new TypeNameHandle(0);
        }

        private static MethodNameHandle LexGenTreeNodeMethodName(char* start, out int width)
        {
            // Technically UB
            var index = new Span<char>(start, int.MaxValue).IndexOfAny(' ', '\r', '\n');

            width = index;

            // We do not have an implementation for string pools yet
            return new MethodNameHandle(0);
        }

        private static FieldNameHandle LexGenTreeNodeFieldName(char* start, out int width, char endCharOne, char endCharTwo)
        {
            // Technically UB
            var index = new Span<char>(start, int.MaxValue).IndexOfAny(endCharOne, endCharTwo);
            width = index;

            return new FieldNameHandle(0);
        }

        private static LargeIntegerHandle LexLargeHexInteger(char* start, out int width)
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