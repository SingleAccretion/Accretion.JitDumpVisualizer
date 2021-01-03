using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static DetalizationState ParseStatementDetalizationState(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<DetalizationState>() is 4);

            if (start[1] is 'a')
            {
                Assert.Equal(start, "(after)");
                width = "(after)".Length;
                return DetalizationState.After;
            }
            else
            {
                Assert.Equal(start, "(before)");
                width = "(before)".Length;
                return DetalizationState.Before;
            }
        }
    }
}
