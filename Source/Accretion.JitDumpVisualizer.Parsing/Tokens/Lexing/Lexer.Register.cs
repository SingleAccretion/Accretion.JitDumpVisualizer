using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static Register ParseRegister(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<Register>() is 1);
            
            var result = (start[1]) switch
            {
                'c' => Result(Register.Rcx, "rcx", start),
                'd' => Result(Register.Rdx, "rdx", start),
                '8' => Result(Register.R8, "r8", start),
                _ => Result(Register.R9, "r9", start),
            };
            
            return Result<Register>(result, out width);
        }
    }
}
