using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using Xunit;

namespace Accretion.JitDumpVisualizer.Tests
{
    public class CountLeadingTests
    {
        [InlineData("", '*', 0)]
        [InlineData(" *", '*', 0)]
        [InlineData("*", '*', 1)]
        [InlineData("*** ***", '^', 0)]
        [InlineData("***    ", '*', 3)]
        [InlineData("*** ***", '*', 3)]
        [InlineData("* * * * * *", '*', 1)]
        [InlineData("********************", '*', 20)]
        [Theory]
        public unsafe void CountsCorrectly(string str, char character, int expected)
        {
            fixed (char* start = str)
            {
                Xunit.Assert.Equal(expected, Count.OfLeading(start, character));
            }
        }
    }
}
