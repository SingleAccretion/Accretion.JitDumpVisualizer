using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static GenTreeNodeConstantIconHandle ParseGenTreeNodeConstantIconHandle(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<GenTreeNodeConstantIconHandle>() is 1);
            
            var result = start[2] switch
            {
                'K' => Result(GenTreeNodeConstantIconHandle.Unknown, "UNKNOWN", start),
                'a' => start[0] switch
                {
                    'c' => Result(GenTreeNodeConstantIconHandle.Class, "class", start),
                    _ => Result(GenTreeNodeConstantIconHandle.Static, "static", start)
                },
                'c' => Result(GenTreeNodeConstantIconHandle.BasicBlockPointer, "bbc", start),
                'd' => Result(GenTreeNodeConstantIconHandle.CidOrMid, "cid/mid", start),
                'n' => start[0] switch
                {
                    'c' => Result(GenTreeNodeConstantIconHandle.ConstantPointer, "constptr", start),
                    'p' => Result(GenTreeNodeConstantIconHandle.PInvoke, "pinvoke", start),
                    _ => Result(GenTreeNodeConstantIconHandle.Function, "ftn", start)
                },
                'k' => Result(GenTreeNodeConstantIconHandle.Token, "token", start),
                'e' => Result(GenTreeNodeConstantIconHandle.Field, "field", start),
                'o' => start[0] switch
                {
                    'g' => Result(GenTreeNodeConstantIconHandle.GlobalPointer, "globalptr", start),
                    _ => Result(GenTreeNodeConstantIconHandle.Scope, "scope", start)

                },
                't' => Result(GenTreeNodeConstantIconHandle.Method, "method", start),
                's' => Result(GenTreeNodeConstantIconHandle.ThreadLocalStorage, "tls", start),
                'C' => Result(GenTreeNodeConstantIconHandle.StringHandle, "[ICON_STR_HDL]", start),
                _ => Result(GenTreeNodeConstantIconHandle.Vararg, "vararg", start),
            };

            return Result<GenTreeNodeConstantIconHandle>(result, out width);
        }
    }
}
