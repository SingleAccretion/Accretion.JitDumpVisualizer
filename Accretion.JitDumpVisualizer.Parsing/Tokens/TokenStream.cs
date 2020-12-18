using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal unsafe struct TokenStream
    {
        private readonly char* _end;
        private char* _start;

        public TokenStream(StringSegment text)
        {
            var textArray = GC.AllocateUninitializedArray<char>(text.Length, pinned: true);
            text.AsSpan().CopyTo(textArray);

            _start = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(textArray));
            _end = _start + text.Length;

            Debug.Assert(*_end is '\0');
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

        private Token PeekNonConstant()
        {
            var span = new Span<char>(_start, (int)_end);
            var digitCount = 0;
            var letterCount = 0;
            var hexDigitCount = 0;
            var length = 0;
            for (int i = 0; i < span.Length; i++)
            {
                var c = span[i];
                if (char.IsDigit(c))
                {
                    digitCount++;
                }
                else if (char.IsLetter(c) || c == '_')
                {
                    letterCount++;
                    if ("abcdefABCDEF".Contains(c))
                    {
                        hexDigitCount++;
                    }
                }
                else if (i != 0 && i != span.Length - 1 && span[i] == '-') { }
                else
                {
                    break;
                }

                length = i + 1;
            }

            // Dumps use all the following formats for hex numbers:
            // 0xXXX, XXX, XXXH, XXXh - hex
            // Where X can be lower/upper case hex digits or normal digits
            // This means that there is an ambiguity between e. g. BB01 - basic block identifier and a hex number
            // And that's what we're going to use as a tie breaker - starting with "BB"
            var kind = (digitCount, letterCount, hexDigitCount) switch
            {
                ( > 0, 0, 0) => TokenKind.Integer,
                ( > 0, > 0, > 0) when
                    letterCount == hexDigitCount && !span.StartsWith("BB") ||
                    letterCount == hexDigitCount + 1 && (span[^1] is 'h' or 'H' || span.StartsWith("0x")) => TokenKind.Integer,
                (0, > 0, _) => TokenKind.Word,
                ( > 0, > 0, _) => TokenKind.Identifier,
                _ => TokenKind.Unknown
            };

            Debug.Assert(length > 0 || kind == TokenKind.Unknown);
            // We want to make some progress even when we don't have a recognizable token
            if (kind == TokenKind.Unknown)
            {
                length = 1;
            }

            // Format of ------------ token: 12 charachters, letter, dashes, #, *
            Debug.Assert(kind != TokenKind.Unknown, $"'{span[0]}' is not a start of a recognizable token.");
            return new Token(kind, length);
        }

        // These are the preliminary results for frequencies of
        // various token types (as well as raw characters) in the dump
        // Whitespace                               31,18%
        // Unknown                                  17,70%
        // Word                                     15,70%
        // Integer                                  7,83%
        // Identifier                               5,94%
        // OpenBracket                              2,52%
        // CloseBracket                             2,37%
        // CloseParen                               2,03%
        // OpenParen                                1,88%
        // EqualsSign                               1,84%
        // Colon                                    1,68%
        // Star                                     1,63%
        // Comma                                    1,59%
        // GreaterThan                              1,06%
        // Dot                                      1,01%
        // QuestionMark                             0,79%
        // LessThan                                 0,78%
        // Hash                                     0,64%
        // OpenCurly                                0,48%
        // CloseCurly                               0,48%
        // SingleQuote                              0,18%
        // TwoDots                                  0,16%
        // ThreeDots                                0,13%
        // Semicolon                                0,13%
        // FourStars                                0,12%
        // FifteenStars                             0,11%
        // LineOfOneHundredAndThirtySevenDashes     0,06%
        // SixStars                                 0,00%
        // Total                                    300 000 tokens
        // 20.1 tokenization time
        // Assuming 6x speedup, that's ~3ms

        // ' '                                      280518 32,88%
        // '-'                                      82461  9,67%
        // '0'                                      48281  5,66%
        // 'e'                                      24146  2,83%
        // '1'                                      18882  2,21%
        // 'r'                                      16144  1,89%
        // 't'                                      15901  1,86%
        // '\r'                                     13336  1,56%
        // '\n'                                     13336  1,56%
        // 'i'                                      12969  1,52%
        // 'n'                                      12339  1,45%
        // 's'                                      11697  1,37%
        // '*'                                      11250  1,32%
        // 'a'                                      10350  1,21%
        // 'f'                                      10230  1,20%
        // 'o'                                      9668   1,13%
        // '2'                                      9524   1,12%
        // 'l'                                      9110   1,07%
        // 'R'                                      8469   0,99%
        // 'V'                                      7579   0,89%
        // '['                                      7403   0,87%
        // 'N'                                      7356   0,86%
        // 'B'                                      7272   0,85%
        // 'g'                                      7103   0,83%
        // ']'                                      6956   0,82%
        // 'L'                                      6860   0,80%
        // 'A'                                      6279   0,74%
        // 'm'                                      6047   0,71%
        // 'p'                                      6002   0,70%
        // '3'                                      5977   0,70%
        // ')'                                      5969   0,70%
        // 'd'                                      5550   0,65%
        // '('                                      5522   0,65%
        // '='                                      5410   0,63%
        // '4'                                      5274   0,62%
        // 'c'                                      5197   0,61%
        // 'C'                                      5164   0,61%
        // 'I'                                      5148   0,60%
        // '|'                                      5135   0,60%
        // '.'                                      5043   0,59%
        // ':'                                      4944   0,58%
        // ','                                      4661   0,55%
        // '_'                                      4546   0,53%
        // '6'                                      4489   0,53%
        // 'T'                                      4477   0,52%
        // '8'                                      4396   0,52%
        // 'S'                                      4392   0,51%
        // '5'                                      4351   0,51%
        // 'x'                                      4153   0,49%
        // 'y'                                      3890   0,46%
        // '7'                                      3701   0,43%
        // 'u'                                      3336   0,39%
        // 'b'                                      3168   0,37%
        // '>'                                      3125   0,37%
        // '9'                                      2970   0,35%
        // 'E'                                      2842   0,33%
        // 'P'                                      2734   0,32%
        // 'D'                                      2721   0,32%
        // 'G'                                      2364   0,28%
        // '?'                                      2325   0,27%
        // '<'                                      2281   0,27%
        // 'h'                                      2094   0,25%
        // 'v'                                      1925   0,23%
        // '$'                                      1882   0,22%
        // '#'                                      1879   0,22%
        // '+'                                      1819   0,21%
        // 'k'                                      1779   0,21%
        // '\t'                                     1705   0,20%
        // 'O'                                      1689   0,20%
        // 'F'                                      1593   0,19%
        // '{'                                      1397   0,16%
        // '}'                                      1397   0,16%
        // '@'                                      1319   0,15%
        // '\'                                      1306   0,15%
        // 'M'                                      1267   0,15%
        // 'H'                                      1144   0,13%
        // 'U'                                      797    0,09%
        // 'w'                                      779    0,09%
        // 'K'                                      710    0,08%
        // '/'                                      664    0,08%
        // 'X'                                      422    0,05%
        // 'z'                                      420    0,05%
        // ';'                                      376    0,04%
        // 'W'                                      320    0,04%
        // '''                                      292    0,03%
        // 'J'                                      273    0,03%
        // 'j'                                      255    0,03%
        // '"'                                      238    0,03%
        // '`'                                      234    0,03%
        // 'q'                                      173    0,02%
        // 'Y'                                      71     0,01%
        // '%'                                      48     0,01%
        // 'Z'                                      31     0,00%
        // 'Q'                                      27     0,00%
        // '!'                                      18     0,00%
        [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Deliberate use of statements for ease of future modification.")]
        private static Token Peek(char* start, char* end, out nint width)
        {
            Debug.Assert(end - start >= 0);

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
                case '"': (token, rawWidth) = (new(TokenKind.DoubleQuote), 1); break;
                case ',': (token, rawWidth) = (new(TokenKind.Comma), 1); break;
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
                case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9': return PeekInteger(out width);
                default: (token, rawWidth) = (new(TokenKind.Unknown), 1); break;
            }

            width = padding + rawWidth;
            return token;
        }

        private static Token PeekInteger(out nint width)
        {
            width = 1;

            return new(TokenKind.Integer);
        }
    }
}
