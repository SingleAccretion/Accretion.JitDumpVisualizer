using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    internal static class Assert
    {
        public const string DebugMode = "DEBUG";

        [Conditional(DebugMode)]
        public static void True(bool condition, string? message = null) => Debug.Assert(condition, message);

        [Conditional(DebugMode)]
        public static unsafe void Equal(char* start, string expected, string? message = null)
        {
            var actual = new string(start, 0, expected.Length);
            True(actual == expected, $"Unexpected string '{actual}'. Expected: '{expected}'");
        }

        [Conditional(DebugMode)]
        public static unsafe void FormatEqual(char* start, string expected, string? message = null, bool hex = false, char wildcard = '0', params char[] valid)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < expected.Length; i++)
            {
                var ch = start[i];
                if ("0123456789".Contains(ch) || ("abcedfABCDEF".Contains(ch) && hex) || valid.Contains(ch))
                {
                    ch = '0';
                }

                builder.Append(ch);
            }

            var actual = builder.ToString();
            True(actual == expected, $"Unexpected string '{actual}'. Expected: '{expected}'");
        }

        [Conditional(DebugMode)]
        public static unsafe void Impossible(char* start)
        {
            // Avoid going out of bounds
            var i = 0;
            while (start[i] is not '\0' && i < 100)
            {
                i++;
            }

            var actual = new string(start, 0, i);
            Debug.Fail($"{actual} is impossible.");
        }
    }
}
