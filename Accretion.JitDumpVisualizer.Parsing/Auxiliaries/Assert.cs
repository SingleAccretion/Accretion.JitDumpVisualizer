using System.Diagnostics;

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
