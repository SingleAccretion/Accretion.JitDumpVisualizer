using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static GenTreeConstantHandleKind ParseGenTreeConstantHandleKind(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<GenTreeConstantHandleKind>() is 1);
            
            var result = (*start) switch
            {
                't' => Result(GenTreeConstantHandleKind.Token, "token", start),
                'm' => Result(GenTreeConstantHandleKind.Method, "method", start),
                'c' => Result(GenTreeConstantHandleKind.Class, "class", start),
                _ => Result(GenTreeConstantHandleKind.ICON_STR_HDL, "[ICON_STR_HDL]", start)
            };

            return Result<GenTreeConstantHandleKind>(result, out width);
        }
    }
}
