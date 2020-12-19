using System;
using System.Diagnostics;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal readonly struct Token : IEquatable<Token>
    {
        // This hack ensures we get good codegen
        private readonly ulong _token;

        public Token(TokenKind kind)
        {
            Debug.Assert(IsConstant(kind), $"Token of type {kind} is not constant.");

            _token = (ulong)kind;
        }

        public Token(TokenKind kind, int value)
        {
            Debug.Assert(kind is TokenKind.Integer or TokenKind.BasicBlock or TokenKind.Statement);

            _token = (ulong)value << 32 | (ulong)kind;
        }

        public Token(TokenKind kind, RuyJitPhase phase)
        {
            Debug.Assert(kind is TokenKind.StartingPhase or TokenKind.FinishingPhase);

            _token = (ulong)phase << 32 | (ulong)kind;
        }

        public TokenKind Kind => (TokenKind)_token;
        private ulong RawValue => _token >> 32;

        public override string? ToString()
        {
            var stringification = $"{Kind}";
            object? value = Kind switch
            {
                TokenKind.Integer or TokenKind.BasicBlock or TokenKind.Statement => RawValue,
                _ => null
            };
            if (value is not null)
            {
                stringification += $": {value}";
            }

            return stringification;
        }

        public static int GetWidth(TokenKind kind) => kind switch
        {
            TokenKind.OpenBracket => 1,
            TokenKind.CloseBracket => 1,
            TokenKind.OpenCurly => 1,
            TokenKind.CloseCurly => 1,
            TokenKind.OpenParen => 1,
            TokenKind.CloseParen => 1,
            TokenKind.LessThan => 1,
            TokenKind.GreaterThan => 1,
            TokenKind.Colon => 1,
            TokenKind.Semicolon => 1,
            TokenKind.EqualsSign => 1,
            TokenKind.SingleQuote => 1,
            TokenKind.DoubleQuote => 1,
            TokenKind.Comma => 1,
            TokenKind.Hash => 1,
            TokenKind.QuestionMark => 1,
            TokenKind.Star => 1,
            TokenKind.FourStars => 4,
            TokenKind.SixStars => 6,
            TokenKind.FifteenStars => 15,
            TokenKind.Dot => 1,
            TokenKind.TwoDots => 2,
            TokenKind.ThreeDots => 3,
            TokenKind.LineOfOneHundredAndThirtySevenDashes => 137,
            TokenKind.EndOfLine => 1,
            TokenKind.EndOfFile => 0,
            _ => throw new ArgumentOutOfRangeException($"Token of kind {kind} has no constant width.")
        };

        public override bool Equals(object? obj) => obj is Token token && Equals(token);
        public bool Equals(Token other) => _token == other._token;
        public override int GetHashCode() => HashCode.Combine(_token);

        public static bool operator ==(Token left, Token right) => left.Equals(right);
        public static bool operator !=(Token left, Token right) => !(left == right);

        private static bool IsConstant(TokenKind kind) => kind is not
           (TokenKind.Word or
            TokenKind.Integer or
            TokenKind.Identifier);
    }
}
