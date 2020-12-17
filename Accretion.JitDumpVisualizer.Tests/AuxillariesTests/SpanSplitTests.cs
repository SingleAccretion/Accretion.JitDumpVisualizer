using System;
using Xunit;
using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Collections.Generic;
using System.Linq;

namespace Accretion.JitDumpVisualizer.Tests
{
    public class SpanSplitTests
    {
        [Theory]
        [InlineData("", new string[] { })]
        [InlineData("a", new[] { "a" })]
        [InlineData(" a", new[] { "a" })]
        [InlineData("a ", new[] { "a" })]
        [InlineData(" a ", new[] { "a" })]
        [InlineData("Two  words", new[] { "Two", "words" })]
        [InlineData(" Two  words", new[] { "Two", "words" })]
        [InlineData(" Two  words ", new[] { "Two", "words" })]
        [InlineData(" One two three four ", new[] { "One", "two", "three", "four" })]
        [InlineData(" [123] ... ZZZ --- ", new[] { "[123]", "...", "ZZZ", "---" })]
        public void SplitsStringsCorrectly(string input, string[] expected)
        {
            var actual = new List<string>();
            foreach (var substring in input.AsSpan().Split())
            {
                actual.Add(substring.ToString());
            }

            Assert.Equal(expected.AsEnumerable(), actual.AsEnumerable());
        }
    }
}
