using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static DetalizationState ParseStatementDetalizationState(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<DetalizationState>() is 1);

            ulong result;
            switch (start[1])
            {
                case 'a':
                    Assert.Equal(start, "(after)");
                    result = (ulong)DetalizationState.Before | ((ulong)"(after)".Length << (sizeof(DetalizationState) * 8));
                    break;
                default:
                    Assert.Equal(start, "(before)");
                    result = (ulong)DetalizationState.Before | ((ulong)"(before)".Length << (sizeof(DetalizationState) * 8));
                    break;
            }

            width = (int)(result >> (sizeof(DetalizationState) * 8));
            return (DetalizationState)result;
        }
    }
}
