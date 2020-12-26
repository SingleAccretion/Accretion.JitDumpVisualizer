using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static GenTreeNodeFlags ParseGenTreeNodeFlags(char* start)
        {
            var flags = GenTreeNodeFlags.None;

            switch (start[0])
            {
                case 'I': flags |= GenTreeNodeFlags.I; break;
                case 'H': flags |= GenTreeNodeFlags.H; break;
                case '#': flags |= GenTreeNodeFlags.Hash; break;
                case 'D': flags |= GenTreeNodeFlags.D; break;
                case 'n': flags |= GenTreeNodeFlags.n; break;
                case 'J': flags |= GenTreeNodeFlags.J; break;
                case '*': flags |= GenTreeNodeFlags.Star; break;
                default: Assert.Equal(start, "-"); break;
            }
            switch (start[1])
            {
                case 'A': flags |= GenTreeNodeFlags.A; break;
                case 'c': flags |= GenTreeNodeFlags.c; break;
                default: Assert.Equal(start + 1, "-"); break;
            }
            switch (start[2])
            {
                case 'C': flags |= GenTreeNodeFlags.C; break;
                default: Assert.Equal(start + 2, "-"); break;
            }
            switch (start[3])
            {
                case 'X': flags |= GenTreeNodeFlags.X; break;
                default: Assert.Equal(start + 3, "-"); break;
            }
            switch (start[4])
            {
                case 'G': flags |= GenTreeNodeFlags.G; break;
                default: Assert.Equal(start + 4, "-"); break;
            }
            switch (start[5])
            {
                case 'O': flags |= GenTreeNodeFlags.O; break;
                case '+': flags |= GenTreeNodeFlags.Plus; break;
                default: Assert.Equal(start + 5, "-"); break;
            }
            Assert.Equal(start + 6, "-");
            switch (start[7])
            {
                case 'N': flags |= GenTreeNodeFlags.N; break;
                default: Assert.Equal(start + 7, "-"); break;
            }
            switch (start[8])
            {
                case 'R': flags |= GenTreeNodeFlags.R; break;
                default: Assert.Equal(start + 8, "-"); break;
            }
            Assert.Equal(start + 9, "-");
            switch (start[10])
            {
                case 'L': flags |= GenTreeNodeFlags.L; break;
                default: Assert.Equal(start + 10, "-"); break;
            }
            Assert.Equal(start + 11, "-");

            return flags;

        }
    }
}
