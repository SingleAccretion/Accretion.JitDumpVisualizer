using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static BasicBlockFlag ParseBasicBlockFlag(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<BasicBlockFlag>() is 1);

            ulong result;
            switch (*start)
            {
                case 'i':
                    switch (start[1])
                    {
                        case 'd':
                            Assert.Equal(start, "idxlen");
                            result = (ulong)BasicBlockFlag.IdxLen | ((ulong)"idxlen".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                        case 'n':
                            Assert.Equal(start, "internal");
                            result = (ulong)BasicBlockFlag.Internal | ((ulong)"internal".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                        default:
                            Assert.Equal(start, "i");
                            result = (ulong)BasicBlockFlag.I | ((ulong)"i".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                    }
                    break;
                case 'l':
                    Assert.Equal(start, "label");
                    result = (ulong)BasicBlockFlag.Label | ((ulong)"label".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
                case 't':
                    Assert.Equal(start, "target");
                    result = (ulong)BasicBlockFlag.Target | ((ulong)"target".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
                case 'h':
                    Assert.Equal(start, "hascall");
                    result = (ulong)BasicBlockFlag.HasCall | ((ulong)"hascall".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
                case 'n':
                    switch (start[1])
                    {
                        case 'e':
                            Assert.Equal(start, "newobj");
                            result = (ulong)BasicBlockFlag.NewObj | ((ulong)"newobj".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                        default:
                            Assert.Equal(start, "nullcheck");
                            result = (ulong)BasicBlockFlag.NullCheck | ((ulong)"nullcheck".Length << (sizeof(BasicBlockFlag) * 8));
                            break;
                    }
                    break;
                case 'g':
                    Assert.Equal(start, "gcsafe");
                    result = (ulong)BasicBlockFlag.GCSafe | ((ulong)"gcsafe".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
                default:
                    Assert.Equal(start, "LIR");
                    result = (ulong)BasicBlockFlag.LIR | ((ulong)"LIR".Length << (sizeof(BasicBlockFlag) * 8));
                    break;
            }

            width = (int)(result >> (sizeof(BasicBlockFlag) * 8));
            return (BasicBlockFlag)result;
        }
    }
}
