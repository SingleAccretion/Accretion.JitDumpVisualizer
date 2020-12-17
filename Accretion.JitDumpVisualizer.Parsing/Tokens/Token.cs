using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using System.Diagnostics;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal readonly struct Token : IEquatable<Token>
    {
        public const string OpenBracket = "[";
        public const string CloseBracket = "]";
        public const string OpenCurly = "{";
        public const string CloseCurly = "}";
        public const string OpenParen = "(";
        public const string CloseParen = ")";
        public const string LessThan = "<";
        public const string GreaterThan = ">";
        public const string Colon = ":";
        public const string Semicolon = ";";
        public const string EqualsSign = "=";
        public const string SingleQuote = "\'";
        public const string DoubleQuote = "\"";
        public const string Comma = ",";
        public const string Hash = "#";
        public const string QuestionMark = "?";
        public const string Star = "*";
        public const string FourStars = "****";
        public const string SixStars = "******";
        public const string FifteenStars = "***************";
        public const string Dot = ".";
        public const string TwoDots = "..";
        public const string ThreeDots = "...";
        public const string EndOfLine = "\n";
        public const string EndOfFile = "";
        public const string LineOfOneHundredAndThirtySevenDashes = "-----------------------------------------------------------------------------------------------------------------------------------------";

        private readonly int _value;

        public Token(TokenKind kind)
        {
            Debug.Assert(IsConstant(kind));

            Kind = kind;
            _value = 0;
        }

        public Token(TokenKind kind, int value)
        {
            Debug.Assert(kind == TokenKind.Integer);

            Kind = kind;
            _value = value;
        }

        public TokenKind Kind { get; }

        public override string? ToString() => $"{Kind}";

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

        public static string GetText(TokenKind kind) => kind switch
        {
            TokenKind.OpenBracket => OpenBracket,
            TokenKind.CloseBracket => CloseBracket,
            TokenKind.OpenCurly => OpenCurly,
            TokenKind.CloseCurly => CloseCurly,
            TokenKind.OpenParen => OpenParen,
            TokenKind.CloseParen => CloseParen,
            TokenKind.LessThan => LessThan,
            TokenKind.GreaterThan => GreaterThan,
            TokenKind.Colon => Colon,
            TokenKind.Semicolon => Semicolon,
            TokenKind.EqualsSign => EqualsSign,
            TokenKind.SingleQuote => SingleQuote,
            TokenKind.Comma => Comma,
            TokenKind.Hash => Hash,
            TokenKind.QuestionMark => QuestionMark,
            TokenKind.Star => Star,
            TokenKind.FourStars => FourStars,
            TokenKind.SixStars => SixStars,
            TokenKind.FifteenStars => FifteenStars,
            TokenKind.Dot => Dot,
            TokenKind.TwoDots => TwoDots,
            TokenKind.ThreeDots => ThreeDots,
            TokenKind.LineOfOneHundredAndThirtySevenDashes => LineOfOneHundredAndThirtySevenDashes,
            TokenKind.EndOfLine => EndOfLine,
            TokenKind.EndOfFile => EndOfFile,
            _ => throw new ArgumentException($"No constant string exists for {kind}.")
        };

        public override bool Equals(object? obj) => obj is Token token && Equals(token);
        public bool Equals(Token other) => Kind == other.Kind && _value == other._value;
        public override int GetHashCode() => HashCode.Combine(Kind, _value);

        public static bool operator ==(Token left, Token right) => left.Equals(right);
        public static bool operator !=(Token left, Token right) => !(left == right);

        private static bool IsConstant(TokenKind kind) => kind is not
           (TokenKind.Word or
            TokenKind.Integer or
            TokenKind.Identifier or
            TokenKind.Unknown or
            TokenKind.EndOfLine);
    }
}
