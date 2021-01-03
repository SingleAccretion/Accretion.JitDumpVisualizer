using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static GenTreeNodeType ParseGenTreeNodeType(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<GenTreeNodeType>() is 1);
            // This enusres that we have already handled "nullcheck" and "help"
            Assert.True(start[0] is not 'n' or 'h');

            var result = (start[0]) switch
            {
                '<' => Result(GenTreeNodeType.Undefined, "<UNDEF>", start),
                'b' => (start[2]) switch
                {
                    'k' => Result(GenTreeNodeType.Blk, "blk", start),
                    'o' => Result(GenTreeNodeType.Bool, "bool", start),
                    'r' => Result(GenTreeNodeType.Byref, "byref", start),
                    _ => Result(GenTreeNodeType.Byte, "byte", start)
                },
                'd' => Result(GenTreeNodeType.Double, "double", start),
                'f' => Result(GenTreeNodeType.Float, "float", start),
                'i' => Result(GenTreeNodeType.Int, "int", start),
                'l' => (start[1]) switch
                {
                    'c' => Result(GenTreeNodeType.LclBlk, "lclBlk", start),
                    _ => Result(GenTreeNodeType.Long, "long", start)
                },
                'r' => Result(GenTreeNodeType.Ref, "ref", start),
                's' => (start[4]) switch
                {
                    't' => Result(GenTreeNodeType.Short, "short", start),
                    '1' => (start[5]) switch
                    {
                        '2' => Result(GenTreeNodeType.Simd12, "simd12", start),
                        _ => Result(GenTreeNodeType.Simd16, "simd16", start)
                    },
                    '3' => Result(GenTreeNodeType.Simd32, "simd32", start),
                    '8' => Result(GenTreeNodeType.Simd8, "simd8", start),
                    _ => Result(GenTreeNodeType.Struct, "struct", start)
                },
                'u' => (start[1]) switch
                {
                    'b' => Result(GenTreeNodeType.UByte, "ubyte", start),
                    'i' => Result(GenTreeNodeType.UInt, "uint", start),
                    'l' => Result(GenTreeNodeType.ULong, "ulong", start),
                    _ => Result(GenTreeNodeType.UShort, "ushort", start)
                },
                _ => Result(GenTreeNodeType.Void, "void", start)
            };

            return Result<GenTreeNodeType>(result, out width);
        }
    }
}
