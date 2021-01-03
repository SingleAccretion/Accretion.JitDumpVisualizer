using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static BasicBlockFlag ParseBasicBlockFlag(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<BasicBlockFlag>() is 1);
            
            var result = (*start) switch
            {
                'i' => (start[1]) switch
                {
                    'd' => Result(BasicBlockFlag.IdxLen, "idxlen", start),
                    'n' => Result(BasicBlockFlag.Internal, "internal", start),
                    _ => Result(BasicBlockFlag.I, "i", start)
                },
                'l' => Result(BasicBlockFlag.Label, "label", start),
                't' => Result(BasicBlockFlag.Target, "target", start),
                'h' => Result(BasicBlockFlag.HasCall, "hascall", start),
                'n' => (start[1]) switch
                {
                    'e' => Result(BasicBlockFlag.NewObj, "newobj", start),
                    _ => Result(BasicBlockFlag.NullCheck, "nullcheck", start)
                },
                'g' => Result(BasicBlockFlag.GCSafe, "gcsafe", start),
                _ => Result(BasicBlockFlag.LIR, "LIR", start)
            };

            return Result<BasicBlockFlag>(result, out width);
        }
    }
}
