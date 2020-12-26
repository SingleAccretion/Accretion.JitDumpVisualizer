using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static BasicBlockJumpTargetKind ParseBasicBlockJumpTargetKind(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<BasicBlockJumpTargetKind>() is 1);

            ulong result;
            switch (start[2])
            {
                case 'c':
                    Assert.Equal(start, "( cond )");
                    result = (ulong)BasicBlockJumpTargetKind.Conditional | ((ulong)"( cond )".Length << (sizeof(BasicBlockJumpTargetKind) * 8));
                    break;
                case 'l':
                    Assert.Equal(start, "(always)");
                    result = (ulong)BasicBlockJumpTargetKind.Always | ((ulong)"(always)".Length << (sizeof(BasicBlockJumpTargetKind) * 8));
                    break;
                case 'e':
                    Assert.Equal(start, "(return)");
                    result = (ulong)BasicBlockJumpTargetKind.Return | ((ulong)"(return)".Length << (sizeof(BasicBlockJumpTargetKind) * 8));
                    break;
                default:
                    Assert.Equal(start, "(cond)");
                    result = (ulong)BasicBlockJumpTargetKind.Conditional | ((ulong)"(cond)".Length << (sizeof(BasicBlockJumpTargetKind) * 8));
                    break;
            }

            width = (int)(result >> (sizeof(BasicBlockJumpTargetKind) * 8));
            return (BasicBlockJumpTargetKind)result;
        }
    }
}
