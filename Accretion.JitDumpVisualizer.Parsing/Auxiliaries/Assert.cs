using System.Diagnostics;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    internal static class Assert
    {
        public const string DebugMode = "DEBUG";

        [Conditional(DebugMode)]
        public static void True(bool condition, string? message = null) => System.Diagnostics.Debug.Assert(condition, message);

        [Conditional(DebugMode)]
        public static unsafe void Equal(char* start, string expected, string? message = null)
        {
            var actual = new string(start, 0, expected.Length);
            True(actual == expected, $"Unexpected string '{actual}'. Expected: '{expected}'");
        }
    }
}
