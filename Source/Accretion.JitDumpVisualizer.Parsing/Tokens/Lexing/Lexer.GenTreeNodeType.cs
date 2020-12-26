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

            ulong result;
            switch (start[0])
            {
                case '<':
                    Assert.Equal(start, "<UNDEF>");
                    result = (ulong)GenTreeNodeType.Undefined | ((ulong)"<UNDEF>".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 'b':
                    switch (start[2])
                    {
                        case 'k':
                            Assert.Equal(start, "blk");
                            result = (ulong)GenTreeNodeType.Blk | ((ulong)"blk".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case 'o':
                            Assert.Equal(start, "bool");
                            result = (ulong)GenTreeNodeType.Bool | ((ulong)"bool".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case 'r':
                            Assert.Equal(start, "byref");
                            result = (ulong)GenTreeNodeType.Byref | ((ulong)"byref".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        default:
                            Assert.Equal(start, "byte");
                            result = (ulong)GenTreeNodeType.Byte | ((ulong)"byte".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                    }
                    break;
                case 'd':
                    Assert.Equal(start, "double");
                    result = (ulong)GenTreeNodeType.Double | ((ulong)"double".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 'f':
                    Assert.Equal(start, "float");
                    result = (ulong)GenTreeNodeType.Float | ((ulong)"float".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 'i':
                    Assert.Equal(start, "int");
                    result = (ulong)GenTreeNodeType.Int | ((ulong)"int".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 'l':
                    switch (start[1])
                    {
                        case 'c':
                            Assert.Equal(start, "lclBlk");
                            result = (ulong)GenTreeNodeType.LclBlk | ((ulong)"lclBlk".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        default:
                            Assert.Equal(start, "long");
                            result = (ulong)GenTreeNodeType.Long | ((ulong)"long".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                    }
                    break;
                case 'r':
                    Assert.Equal(start, "ref");
                    result = (ulong)GenTreeNodeType.Ref | ((ulong)"ref".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
                case 's':
                    switch (start[4])
                    {
                        case 't':
                            Assert.Equal(start, "short");
                            result = (ulong)GenTreeNodeType.Short | ((ulong)"short".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case '1':
                            switch (start[5])
                            {
                                case '2':
                                    Assert.Equal(start, "simd12");
                                    result = (ulong)GenTreeNodeType.Simd12 | ((ulong)"simd12".Length << (sizeof(GenTreeNodeType) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "simd16");
                                    result = (ulong)GenTreeNodeType.Simd16 | ((ulong)"simd16".Length << (sizeof(GenTreeNodeType) * 8));
                                    break;
                            }
                            break;
                        case '3':
                            Assert.Equal(start, "simd32");
                            result = (ulong)GenTreeNodeType.Simd32 | ((ulong)"simd32".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case '8':
                            Assert.Equal(start, "simd8");
                            result = (ulong)GenTreeNodeType.Simd8 | ((ulong)"simd8".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        default:
                            Assert.Equal(start, "struct");
                            result = (ulong)GenTreeNodeType.Struct | ((ulong)"struct".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                    }
                    break;
                case 'u':
                    switch (start[1])
                    {
                        case 'b':
                            Assert.Equal(start, "ubyte");
                            result = (ulong)GenTreeNodeType.UByte | ((ulong)"ubyte".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case 'i':
                            Assert.Equal(start, "uint");
                            result = (ulong)GenTreeNodeType.UInt | ((ulong)"uint".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        case 'l':
                            Assert.Equal(start, "ulong");
                            result = (ulong)GenTreeNodeType.ULong | ((ulong)"ulong".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                        default:
                            Assert.Equal(start, "ushort");
                            result = (ulong)GenTreeNodeType.UShort | ((ulong)"ushort".Length << (sizeof(GenTreeNodeType) * 8));
                            break;
                    }
                    break;
                default:
                    Assert.Equal(start, "void");
                    result = (ulong)GenTreeNodeType.Void | ((ulong)"void".Length << (sizeof(GenTreeNodeType) * 8));
                    break;
            }

            width = (int)(result >> (sizeof(GenTreeNodeType) * 8));
            return (GenTreeNodeType)result;
        }
    }
}
