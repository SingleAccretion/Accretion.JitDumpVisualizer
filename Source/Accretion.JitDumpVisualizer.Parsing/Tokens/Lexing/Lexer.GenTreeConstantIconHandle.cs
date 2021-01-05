using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static GenTreeConstantIconHandle ParseGenTreeConstantIconHandle(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<GenTreeConstantIconHandle>() is 1);
            
            var result = start[2] switch
            {
                'K' => Result(GenTreeConstantIconHandle.Unknown, "Unknown", start),
                'a' => start[0] switch
                {
                    'c' => Result(GenTreeConstantIconHandle.Class, "class", start),
                    _ => Result(GenTreeConstantIconHandle.Static, "static", start)
                },
                'b' => Result(GenTreeConstantIconHandle.BasicBlockPointer, "bbc", start),
                'd' => Result(GenTreeConstantIconHandle.CidOrMid, "cid/mid", start),
                'n' => start[0] switch
                {
                    'c' => Result(GenTreeConstantIconHandle.ConstantPointer, "constptr", start),
                    'p' => Result(GenTreeConstantIconHandle.PInvoke, "pinvoke", start),
                    _ => Result(GenTreeConstantIconHandle.Function, "ftn", start)
                },
                'k' => Result(GenTreeConstantIconHandle.Token, "token", start),
                'e' => Result(GenTreeConstantIconHandle.Field, "field", start),
                'o' => start[0] switch
                {
                    'g' => Result(GenTreeConstantIconHandle.GlobalPointer, "globalptr", start),
                    _ => Result(GenTreeConstantIconHandle.Scope, "scope", start)

                },
                't' => Result(GenTreeConstantIconHandle.Method, "method", start),
                's' => Result(GenTreeConstantIconHandle.ThreadLocalStorage, "tls", start),
                'C' => Result(GenTreeConstantIconHandle.StringHandle, "[ICON_STR_HDL]", start),
                _ => Result(GenTreeConstantIconHandle.Vararg, "vararg", start),
            };

            return Result<GenTreeConstantIconHandle>(result, out width);
        }
    }
}
