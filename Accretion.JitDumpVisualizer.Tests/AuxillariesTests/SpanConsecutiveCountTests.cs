using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using Xunit;

namespace Accretion.JitDumpVisualizer.Tests
{
    public class SpanConsecutiveCountTests
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
        public void CountsCorrectly(string str, char character, int expected) => Assert.Equal(expected, str.AsSpan().ConsecutiveCount(character));
    }
}
