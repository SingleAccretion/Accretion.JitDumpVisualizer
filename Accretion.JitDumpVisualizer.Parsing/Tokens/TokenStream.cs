using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            Debug.Assert(source[^1] == *(_end - 1));
            Debug.Assert(*_end == '\0', "Pinned buffer must be null-terminated.");
        }

        public TokenStream(char* start, nint length)
        {
            _start = start;
            _end = _start + length;
            _lastTokenKind = TokenKind.Unknown;

            Debug.Assert(*_end is '\0');
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
            _lastTokenKind = token.Kind;

            return token;
        }

        public TokenSource? NextTokensBeforeAny(ReadOnlySpan<Token> tokens) => throw new NotImplementedException();

        public TokenSource? NextTokensBefore(ReadOnlySpan<Token> tokens) => throw new NotImplementedException();

        public TokenSource? NextTokensAfter(ReadOnlySpan<Token> tokens) => throw new NotImplementedException();

        public TokenSource NextLine() => throw new NotImplementedException();

        private static Token Peek(char* start, char* end, TokenKind lastToken, out nint width)
        {
            Debug.Assert(end - start >= 0, "There is no peeking outside the bounds.");
            Debug.Assert(*end is '\0', "The source buffer must be alive.");

            nint padding = 0;
            while (*start is ' ' or '\t')
            {
                padding++;
                start++;
            }

            nint rawWidth;
            Token token;
            switch (*start)
            {
                case '\0': (token, rawWidth) = (new(TokenKind.EndOfFile), 0); break;
                case '[': (token, rawWidth) = (new(TokenKind.OpenBracket), 1); break;
                case ']': (token, rawWidth) = (new(TokenKind.CloseBracket), 1); break;
                case '{': (token, rawWidth) = (new(TokenKind.OpenCurly), 1); break;
                case '}': (token, rawWidth) = (new(TokenKind.CloseCurly), 1); break;
                case '(': (token, rawWidth) = (new(TokenKind.OpenParen), 1); break;
                case ')': (token, rawWidth) = (new(TokenKind.CloseParen), 1); break;
                case '<': (token, rawWidth) = (new(TokenKind.LessThan), 1); break;
                case '>': (token, rawWidth) = (new(TokenKind.GreaterThan), 1); break;
                case ':': (token, rawWidth) = (new(TokenKind.Colon), 1); break;
                case ';': (token, rawWidth) = (new(TokenKind.Semicolon), 1); break;
                case '=': (token, rawWidth) = (new(TokenKind.EqualsSign), 1); break;
                case '\'': (token, rawWidth) = (new(TokenKind.SingleQuote), 1); break;
                case '|': (token, rawWidth) = (new(TokenKind.Pipe), 1); break;
                case '"': (token, rawWidth) = (new(TokenKind.DoubleQuote), 1); break;
                case ',': (token, rawWidth) = (new(TokenKind.Comma), 1); break;
                case '\r': (token, rawWidth) = (new(TokenKind.EndOfLine), 2); break;
                case '\n': (token, rawWidth) = (new(TokenKind.EndOfLine), 1); break;
                case '#': (token, rawWidth) = (new(TokenKind.Hash), 1); break;
                case '?': (token, rawWidth) = (new(TokenKind.QuestionMark), 1); break;
                case '.':
                    switch (Count.OfLeading(start, end, '.'))
                    {
                        case 3: (token, rawWidth) = (new(TokenKind.ThreeDots), 3); break;
                        case 2: (token, rawWidth) = (new(TokenKind.TwoDots), 2); break;
                        default: (token, rawWidth) = (new(TokenKind.Dot), 1); break;
                    }
                    break;
                case '*':
                    switch (Count.OfLeading(start, end, '*'))
                    {
                        case 15: (token, rawWidth) = (new(TokenKind.FifteenStars), 15); break;
                        case 4: (token, rawWidth) = (new(TokenKind.FourStars), 4); break;
                        case 6: (token, rawWidth) = (new(TokenKind.SixStars), 6); break;
                        default: (token, rawWidth) = (new(TokenKind.Star), 1); break;
                    }
                    break;
                case '-' when Count.OfLeading(start, end, '-') == 137: (token, rawWidth) = (new(TokenKind.LineOfOneHundredAndThirtySevenDashes), 137); break;
                // All integers in the dump start with characters 0 through 9 
                case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                    token = new(TokenKind.Integer, IntegersParser.ParseGenericInteger(start, out rawWidth));
                    break;
                case 'B':
                    switch (start[1])
                    {
                        case 'B':
                            switch (start[2])
                            {
                                // BBnum
                                case 'n': (token, rawWidth) = (new(TokenKind.BasicBlockNumberColumnHeader), 5); break;
                                // BBid
                                case 'i': (token, rawWidth) = (new(TokenKind.BasicBlockIdColumnHeader), 4); break;
                                // In fgDebugCheckBBlist, BBJ_ALWAYS, BBJ_NONE, RefTypeBB, <some> BB
                                case 'l' or 'J' or ' ': goto ReturnUnknown;
                                // BBXX
                                default:
                                    switch (start[3])
                                    {
                                        // Name in register allocation table
                                        case ' ': goto ReturnUnknown;
                                        default:
                                            (token, rawWidth) = (new(TokenKind.BasicBlock, IntegersParser.ParseIntegerTwoDigits(start + 2)), 4);
                                            break;
                                    }
                                    break;
                            }
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'S' when lastToken == TokenKind.FifteenStars:
                    const string StartingPhase = "Starting PHASE";

                    Debug.Assert(new string(start, 0, StartingPhase.Length) == StartingPhase);

                    var kind = TokenKind.StartingTopLevelPhase;
                    var skip = StartingPhase.Length + 1;

                    goto ReturnPhase;
                case 'F' when lastToken == TokenKind.FifteenStars:
                    const string FinishingPhase = "Finishing PHASE";

                    Debug.Assert(new string(start, 0, FinishingPhase.Length) == FinishingPhase);

                    kind = TokenKind.FinishingTopLevelPhase;
                    skip = FinishingPhase.Length + 1;

                    // This saves on a lengthy Token constructor
                ReturnPhase:
                    var phase = ParseRuyJitPhase(start + skip, out var phaseWidth);
                    (token, rawWidth) = (new(kind, phase), phaseWidth + skip);
                    break;
                default:
                ReturnUnknown:
                    (token, rawWidth) = (new(TokenKind.Unknown), 1); break;
            }

            width = padding + rawWidth;
            return token;
        }

        private static RuyJitPhase ParseRuyJitPhase(char* start, out nint width)
        {
            RuyJitPhase phase;
            switch (start[0])
            {
                case 'A':
                    switch (start[1])
                    {
                        case 'l': (phase, width) = (RuyJitPhase.AllocateObjects, "Allocate Objects".Length); break;
                        default: (phase, width) = (RuyJitPhase.AssertionProp, "Assertion prop".Length); break;
                    }
                    break;
                case 'B': (phase, width) = (RuyJitPhase.BuildSSARepresentation, "Build SSA representation".Length); break;
                case 'C':
                    switch (start[1])
                    {
                        case 'a': (phase, width) = (RuyJitPhase.CalculateStackLevelSlots, "Calculate stack level slots".Length); break;
                        case 'l':
                            switch (start[6])
                            {
                                case 'f': (phase, width) = (RuyJitPhase.CloneFinally, "Clone finally".Length); break;
                                default: (phase, width) = (RuyJitPhase.CloneLoops, "Clone loops".Length); break;
                            }
                            break;
                        case 'o':
                            switch (start[8])
                            {
                                case 'b': (phase, width) = (RuyJitPhase.ComputeBlocksReachability, "Compute blocks reachability".Length); break;
                                case 'e': (phase, width) = (RuyJitPhase.ComputeEdgeWeights, "Compute edge weights".Length); break;
                                default: (phase, width) = (RuyJitPhase.ComputePreds, "Compute preds".Length); break;
                            }
                            break;
                        default: (phase, width) = (RuyJitPhase.CreateEHFunclets, "Create EH funclets".Length); break;
                    }
                    break;
                case 'D':
                    switch (start[1])
                    {
                        case 'e': (phase, width) = (RuyJitPhase.DetermineFirstColdBlock, "Determine first cold block".Length); break;
                        default:
                            switch (start[3])
                            {
                                case '\'': (phase, width) = (RuyJitPhase.DoSimpleLowering, "Do 'simple' lowering".Length); break;
                                default: (phase, width) = (RuyJitPhase.DoValueNumbering, "Do value numbering".Length); break;
                            }
                            break;
                    }
                    break;
                case 'E':
                    switch (start[1])
                    {
                        case 'a': (phase, width) = (RuyJitPhase.EarlyValuePropagation, "Early Value Propagation".Length); break;
                        case 'x': (phase, width) = (RuyJitPhase.ExpandPatchpoints, "Expand patchpoints".Length); break;
                        default:
                            switch (start[5])
                            {
                                case 'c': (phase, width) = (RuyJitPhase.EmitCode, "Emit code".Length); break;
                                default: (phase, width) = (RuyJitPhase.EmitGCPlusEHTables, "Emit GC+EH tables".Length); break;
                            }
                            break;
                    }
                    break;
                case 'F': (phase, width) = (RuyJitPhase.FindOperOrder, "Find oper order".Length); break;
                case 'G':
                    switch (start[1])
                    {
                        case 'e': (phase, width) = (RuyJitPhase.GenerateCode, "Generate code".Length); break;
                        default: (phase, width) = (RuyJitPhase.GSCookie, "GS Cookie".Length); break;
                    }
                    break;
                case 'H': (phase, width) = (RuyJitPhase.HoistLoopCode, "Hoist loop code".Length); break;
                case 'I':
                    switch (start[1])
                    {
                        case 'm': (phase, width) = (RuyJitPhase.Importation, "Importation".Length); break;
                        default:
                            switch (start[2])
                            {
                                case 'd': (phase, width) = (RuyJitPhase.IndirectCallTransform, "Indirect call transform".Length); break;
                                default: (phase, width) = (RuyJitPhase.InsertGCPolls, "Insert GC Polls".Length); break;
                            }
                            break;
                    }
                    break;
                case 'L':
                    switch (start[1])
                    {
                        case 'i': (phase, width) = (RuyJitPhase.LinearScanRegisterAlloc, "Linear scan register alloc".Length); break;
                        default: (phase, width) = (RuyJitPhase.LoweringNodeInfo, "Lowering nodeinfo".Length); break;
                    }
                    break;
                case 'M':
                    switch (start[1])
                    {
                        case 'a': (phase, width) = (RuyJitPhase.MarkLocalVars, "Mark local vars".Length); break;
                        case 'e':
                            switch (start[6])
                            {
                                case 'c': (phase, width) = (RuyJitPhase.MergeCallfinallyChains, "Merge callfinally chains".Length); break;
                                default: (phase, width) = (RuyJitPhase.MergeThrowBlocks, "Merge throw blocks".Length); break;
                            }
                            break;
                        default:
                            switch (start[8])
                            {
                                case 'A': (phase, width) = (RuyJitPhase.MorphAddInternalBlocks, "Morph - Add internal blocks".Length); break;
                                case 'B': (phase, width) = (RuyJitPhase.MorphByRefs, "Morph - ByRefs".Length); break;
                                case 'G': (phase, width) = (RuyJitPhase.MorphGlobal, "Morph - Global".Length); break;
                                case 'I':
                                    switch (start[10])
                                    {
                                        case 'i': (phase, width) = (RuyJitPhase.MorphInit, "Morph - Init".Length); break;
                                        default: (phase, width) = (RuyJitPhase.MorphInlining, "Morph - Inlining".Length); break;
                                    }
                                    break;
                                case 'P': (phase, width) = (RuyJitPhase.MorphPromoteStructs, "Morph - Promote Structs".Length); break;
                                default: (phase, width) = (RuyJitPhase.MorphStructsAddressExposed, "Morph - Structs/AddrExp".Length); break;
                            }
                            break;
                    }
                    break;
                case 'O':
                    switch (start[9])
                    {
                        case 'b': (phase, width) = (RuyJitPhase.OptimizeBools, "Optimize bools".Length); break;
                        case 'i': (phase, width) = (RuyJitPhase.OptimizeIndexChecks, "Optimize index checks".Length); break;
                        case 'l':
                            switch (start[10])
                            {
                                case 'a': (phase, width) = (RuyJitPhase.OptimizeLayout, "Optimize layout".Length); break;
                                default: (phase, width) = (RuyJitPhase.OptimizeLoops, "Optimize loops".Length); break;
                            }
                            break;
                        default: (phase, width) = (RuyJitPhase.OptimizeValnumCSEs, "Optimize Valnum CSEs".Length); break;
                    }
                    break;
                case 'P':
                    switch (start[1])
                    {
                        case 'o': (phase, width) = (RuyJitPhase.PostImport, "Post-import".Length); break;
                        default: (phase, width) = (RuyJitPhase.PreImport, "Pre-import".Length); break;
                    }
                    break;
                case 'R':
                    switch (start[1])
                    {
                        case 'a': (phase, width) = (RuyJitPhase.RationalizeIR, "Rationalize IR".Length); break;
                        default:
                            switch (start[13])
                            {
                                case 'f': (phase, width) = (RuyJitPhase.RemoveEmptyFinally, "Remove empty finally".Length); break;
                                default: (phase, width) = (RuyJitPhase.RemoveEmptyTry, "Remove empty try".Length); break;
                            }
                            break;
                    }
                    break;
                case 'S': (phase, width) = (RuyJitPhase.SetBlockOrder, "Set block order".Length); break;
                case 'U':
                    switch (start[1])
                    {
                        case 'n': (phase, width) = (RuyJitPhase.UnrollLoops, "Unroll loops".Length); break;
                        default: (phase, width) = (RuyJitPhase.UpdateFlowGraphEarlyPass, "Update flow graph early pass".Length); break;
                    }
                    break;
                default: (phase, width) = (RuyJitPhase.VNBasedCopyPropagation, "VN based copy prop".Length); break; ;
            }

            return phase;
        }
    }
}
