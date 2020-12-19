using Accretion.JitDumpVisualizer.Parsing.Tokens;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Accretion.JitDumpVisualizer.Tests
{
    public unsafe class IntegersParserTests
    {
        [InlineData("0000", 0)]
        [InlineData("0001", 1)]
        [InlineData("0011", 11)]
        [InlineData("0111", 111)]
        [InlineData("0671", 671)]
        [InlineData("1111", 1111)]
        [InlineData("6070", 6070)]
        [InlineData("8907", 8907)]
        [Theory]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "False positive.")]
        public void ParsesFourDigitIntegersCorrectly(string str, int expected)
        {
            fixed (char* ptr = str)
            {
                Assert.Equal(expected, IntegersParser.ParseIntegerFourDigits(ptr));
            }
        }
    }
}