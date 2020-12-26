using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static GenTreeNodeExactTypeHandle ParseGenTreeNodeExactType(char* start, out int width)
        {
            Assert.NotEqual(start + 1, "-");

            var index = new Span<char>(start, int.MaxValue).IndexOf('>');

            width = index + ">".Length;

            // We do not yet have an implementation for string pools
            return new GenTreeNodeExactTypeHandle(0);
        }
    }
}
