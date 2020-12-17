using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal readonly struct TokenSource
    {
        private readonly StringSegment _text;

        public TokenSource(string dump) => _text = dump;
        public TokenSource(StringSegment dump) => _text = dump;

        public ReadOnlyArray<(TokenSource Header, TokenSource Body, TokenSource Footer)> SplitIntoSections(ReadOnlySpan<Token> headerPattern, ReadOnlySpan<Token> footerPattern)
        {
            var pieces = new ArrayBuilder<(TokenSource, TokenSource, TokenSource)>();
            var stream = GetTokenStream();

            while (stream.NextTokensAfter(headerPattern) is not null &&
                   stream.NextLine() is var header &&
                   stream.NextTokensBefore(footerPattern) is TokenSource body &&
                   stream.NextLine() is var footer)
            {
                pieces.Add((header, body, footer));
            }

            return pieces.AsReadOnlyArray();
        }

        public bool Contains(ReadOnlySpan<Token> pattern) => GetTokenStream().NextTokensBefore(pattern) is not null;

        public TokenStream GetTokenStream() => new TokenStream(_text);

        public override string ToString() => _text.AsSpan().Trim().ToString();
    }
}