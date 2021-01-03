using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static BasicBlockJumpTargetKind ParseBasicBlockJumpTargetKind(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<BasicBlockJumpTargetKind>() is 1);
            
            var result = (start[2]) switch
            {
                'c' => Result(BasicBlockJumpTargetKind.Conditional, "( cond )", start),
                'l' => Result(BasicBlockJumpTargetKind.Always, "(always)", start),
                'e' => Result(BasicBlockJumpTargetKind.Return, "(return)", start),
                _ => Result(BasicBlockJumpTargetKind.Conditional, "(cond)", start)
            };

            return Result<BasicBlockJumpTargetKind>(result, out width);
        }
    }
}
