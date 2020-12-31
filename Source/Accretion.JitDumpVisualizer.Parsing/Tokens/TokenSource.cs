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
            throw new NotImplementedException();
        }

        public bool Contains(ReadOnlySpan<Token> pattern) => throw new NotImplementedException();

        public override string ToString() => _text.AsSpan().Trim().ToString();
    }
}