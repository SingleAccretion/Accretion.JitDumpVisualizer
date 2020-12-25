using Accretion.JitDumpVisualizer.Parsing.Tokens;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Accretion.JitDumpVisualizer.Tests
{
    [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "False positive.")]
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
        public void ParsesFourDigitIntegersCorrectly(string str, int expected)
        {
            fixed (char* ptr = str)
            {
                Assert.Equal(expected, IntegersParser.ParseIntegerFourDigits(ptr));
            }
        }

        [InlineData("00", 0)]
        [InlineData("01", 1)]
        [InlineData("07", 07)]
        [InlineData("10", 10)]
        [InlineData("11", 11)]
        [InlineData("71", 71)]
        [InlineData("70", 70)]
        [InlineData("99", 99)]
        [Theory]
        public void ParsesTwoDigitIntegersCorrectly(string str, int expected)
        {
            fixed (char* ptr = str)
            {
                Assert.Equal(expected, IntegersParser.ParseIntegerTwoDigits(ptr));
            }
        }

        [InlineData("0000", 0, 4)]
        [InlineData("0001", 1, 4)]
        [InlineData("0011", 11, 4)]
        [InlineData("0111", 111, 4)]
        [InlineData("0671", 671, 4)]
        [InlineData("1111", 1111, 4)]
        [InlineData("6070", 6070, 4)]
        [InlineData("8907", 8907, 4)]
        [InlineData("    1", 1, 5)]
        [Theory]
        public void ParsesGenericIntegersCorrectly(string str, int expected, int expectedWidth)
        {
            fixed (char* ptr = str)
            {
                Assert.Equal(expected, IntegersParser.ParseGenericInteger(ptr, out var width));
                Assert.Equal(expectedWidth, width);
            }
        }
    }
}