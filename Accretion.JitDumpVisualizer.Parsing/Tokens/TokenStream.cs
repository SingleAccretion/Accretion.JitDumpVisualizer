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

            nint padding = 0;
            while (*start is ' ' or '\t'
#if RELEASE
                or '\r' or '\n'
#endif
                )
            {
                padding++;
                start++;
            }
            finalWidth = padding;

            TokenKind kind;
            nint rawWidth = 0;
            int rawValue = 0;
            var first = start[0];

#if DEBUG
            if (first is '\n')
            {
                finalWidth += 1;
                return new(TokenKind.EndOfLine);
            }
            else if (first is '\r')
            {
                Assert.Equal(start, "\r\n");
                finalWidth += 2;
                return new(TokenKind.EndOfLine);
            }
#endif

            switch (lastToken)
            {
                case TokenKind.FifteenStars or TokenKind.InlineStartingAt:
                    switch (first)
                    {
                        case 'S':
                            const string StartingPhase = "Starting PHASE";

                            Assert.Equal(start, StartingPhase);

                            kind = TokenKind.StartingPhase;
                            rawWidth += StartingPhase.Length + 1;

                            goto ParsePhase;
                        case 'F':
                            const string FinishingPhase = "Finishing PHASE";

                            Assert.Equal(start, FinishingPhase);

                            kind = TokenKind.FinishingPhase;
                            rawWidth += FinishingPhase.Length + 1;

                        ParsePhase:
                            var phase = ParseRyuJitPhase(start + rawWidth, out var phaseWidth);
                            rawWidth += phaseWidth;
                            rawValue = (int)phase;
                            break;
                        case 'I':
                            switch (start[2])
                            {
                                case ' ':
                                    const string In = "In";

                                    Assert.Equal(start, In);

                                    rawWidth += In.Length + 1;
                                    var function = ParseRyuJitFunction(start + rawWidth, out var functionWidth);

                                    rawWidth += functionWidth;
                                    (kind, rawValue) = (TokenKind.StartingFunction, (int)function);
                                    break;
                                default:
                                    switch (start[7])
                                    {
                                        case '@':
                                            const string Inline = "Inline @[";

                                            Assert.Equal(start, Inline);

                                            rawWidth += Inline.Length;
                                            var inlineStart = IntegersParser.ParseIntegerSixDigits(start + rawWidth);

                                            rawWidth += "000000]".Length;
                                            (kind, rawValue) = (TokenKind.InlineStartingAt, inlineStart);
                                            break;
                                        case 'T':
                                            const string InlineTree = "Inline Tree";

                                            Assert.Equal(start, InlineTree);

                                            rawWidth += InlineTree.Length;
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
                case TokenKind.BasicBlockNumberColumnHeader:
                    switch (first)
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
                    switch (first)
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
                    Assert.Equal(start, Token.OneHundredAndThirySevenLines);
                    kind = TokenKind.BasicBlockTableCenter;
                    rawWidth = Token.OneHundredAndThirySevenLines.Length;
                    break;
                case TokenKind.BasicBlockTableCenter:
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
                case TokenKind.FourStars:
                    kind = TokenKind.BasicBlockInInnerHeader;
                    goto ReturnAnyBasicBlock;
                case TokenKind.TwelveDashes when first is 'B':
                    kind = TokenKind.BasicBlockInTopHeader;
                    goto ReturnAnyBasicBlock;
                default:
                    switch (first)
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
                                case 12: (kind, rawWidth) = (TokenKind.TwelveDashes, 12); break;
                                case 48: (kind, rawWidth) = (TokenKind.FourtyEightDashes, 48); break;
                                case 137: (kind, rawWidth) = (TokenKind.LineOfOneHundredAndThirtySevenDashes, 137); break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        // All integers in the dump start with characters 0 through 9
                        case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                            kind = TokenKind.Integer;
                            rawValue = IntegersParser.ParseGenericInteger(start, out rawWidth);
                            break;
                        case 'B':
                            switch (start[1])
                            {
                                case 'B':
                                    switch (start[2])
                                    {
                                        // BBnum
                                        case 'n': (kind, rawWidth) = (TokenKind.BasicBlockNumberColumnHeader, "BBnum".Length); break;
                                        // In fgDebugCheckBBlist, BBJ_ALWAYS, BBJ_NONE, RefTypeBB, <some> BB
                                        // BBid should be hadnled in the "after BBnum" code
                                        case 'l' or 'J' or ' ': goto ReturnUnknown;
                                        // BBXX
                                        default:
                                            switch (start[3])
                                            {
                                                // Name in register allocation table
                                                case ' ': goto ReturnUnknown;
                                                default: kind = TokenKind.BasicBlock; goto ReturnAnyBasicBlock;
                                            }
                                    }
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                ReturnAnyBasicBlock:
                    Assert.Equal(start, "BB");
                    var dbg = new string(start, 0, 10);
                    rawValue = IntegersParser.ParseIntegerTwoDigits(start + 2);
                    rawWidth += 4;
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
    }
}