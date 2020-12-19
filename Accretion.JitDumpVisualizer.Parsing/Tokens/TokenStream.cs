using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal unsafe struct TokenStream
    {
        private static readonly List<object> _gcRoots = new List<object>();

        private readonly char* _end;
        private char* _start;

        public TokenStream(StringSegment text)
        {
            var source = text.AsSpan();

            var pinnedBuffer = GC.AllocateArray<char>(source.Length + 1, pinned: true);
            source.CopyTo(pinnedBuffer);
            pinnedBuffer[^1] = '\0';
            _gcRoots.Add(pinnedBuffer);

            _start = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(pinnedBuffer));
            _end = _start + source.Length;

            Debug.Assert(source[^1] == *(_end - 1));
            Debug.Assert(*_end == '\0', "Pinned buffer must be null-terminated.");
        }

        public TokenStream(char* start, nint length)
        {
            _start = start;
            _end = _start + length;

            Debug.Assert(*_end is '\0');
        }

        public void Skip(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Next();
            }
        }

        public Token Next()
        {
            var token = Peek(_start, _end, out var width);
            _start += width;

            return token;
        }

        public TokenSource? NextTokensBeforeAny(ReadOnlySpan<Token> tokens) => throw new NotImplementedException();

        public TokenSource? NextTokensBefore(ReadOnlySpan<Token> tokens) => throw new NotImplementedException();

        public TokenSource? NextTokensAfter(ReadOnlySpan<Token> tokens) => throw new NotImplementedException();

        public TokenSource NextLine() => throw new NotImplementedException();

        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Deliberate use of statements for ease of future modification.")]
        private static Token Peek(char* start, char* end, out nint width)
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
                                case 'l' or 'J' or ' ': goto Unknown;
                                // BBXX
                                default:
                                    switch (start[3])
                                    {
                                        // Name in register allocation table
                                        case ' ': goto Unknown;
                                        default:
                                            (token, rawWidth) = (new(TokenKind.BasicBlock, IntegersParser.ParseIntegerTwoDigits(start + 2)), 4);
                                            break;
                                    }
                                    break;
                            }
                            break;
                        default: goto Unknown;
                    }
                    break;
                case 'S':
                    switch (start[1])
                    {
                        case 'T':
                            switch (start[2])
                            {
                                case 'M':
                                    switch (start[3])
                                    {
                                        case 'T':
                                            (token, rawWidth) = (new(TokenKind.Statement, IntegersParser.ParseIntegerFiveDigits(start + 4)), 9);
                                            break;
                                        default: goto Unknown;
                                    }
                                    break;
                                default: goto Unknown;
                            }
                            break;
                        case 't':
                            switch (start[1])
                            {
                                case 'a':
                                    switch (start[2])
                                    {
                                        case 'r':
                                            switch (start[3])
                                            {
                                                case 't':
                                                    switch (start[4])
                                                    {
                                                        case 'i':
                                                            Debug.Assert(start[4] is 'n');
                                                            Debug.Assert(start[5] is 'g');

                                                            var skip = "Starting PHASE ".Length;
                                                            var phase = ParseRuyJitPhase(start + skip);
                                                            (token, rawWidth) = (new(TokenKind.StartingPhase, phase), skip);
                                                            break;
                                                        default: goto Unknown;
                                                    }
                                                    break;
                                                default: goto Unknown;
                                            }
                                            break;
                                        default: goto Unknown;
                                    }
                                    break;
                                default: goto Unknown;
                            }
                            break;
                        default: goto Unknown;
                    }
                    break;
                default:
                Unknown:
                    (token, rawWidth) = (new(TokenKind.Unknown), 1); break;
            }

            width = padding + rawWidth;
            return token;
        }

        private static Token PeekInteger(char* start, out nint width)
        {
            width = 0;
            var value = 0;
            // Largest numbers in the dump have 16 digits
            var digits = stackalloc int[16];

            var integerBase = 10;
            while (true)
            {
                var delta = 0;
                var ch = *start;
                switch (ch)
                {
                    // This is correct even taking into account the "0x" prefix (as it will be counted as a leading digit)
                    case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                        delta = '0';
                        break;
                    case 'a' or 'b' or 'c' or 'd' or 'e' or 'f':
                        delta = 'a' - 10;
                        integerBase = 16;
                        break;
                    case 'B' or 'C' or 'D' or 'E' or 'F':
                        delta = 'A' - 10;
                        integerBase = 16;
                        break;
                    case 'x' or 'h':
                        integerBase = 16;
                        break;
                    default:
                        Debug.Assert(width <= 16);

                        var multiplier = 1;
                        for (nint i = width - 1; i >= 0; i--)
                        {
                            value += digits[i] * multiplier;
                            multiplier *= integerBase;
                        }

                        return new(TokenKind.Integer, value);
                }

                digits[width] = ch - delta;
                width++;
                start++;
            }
        }

        private static int ParseIntegerFourDigits(char* start)
        {
            var d1 = start[0];
            var d0 = start[1];

            Debug.Assert(char.IsDigit(d0) && char.IsDigit(d1), $"{d1} or {d0} is not a digit.");

            d0 -= '0';
            d1 -= '0';

            return d0 + 10 * d1;
        }

        private static RuyJitPhase ParseRuyJitPhase(char* start)
        {
            throw new NotImplementedException();
        }
    }
}
