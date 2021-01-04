using Accretion.JitDumpVisualizer.Parsing.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    internal static unsafe class Assert
    {
        public const string DebugMode = "DEBUG";
        private static readonly StreamWriter _dumper = new StreamWriter("debug.txt");

        [Conditional(DebugMode)]
        public static void True(bool condition, string? message = null) => Debug.Assert(condition, message);

        [Conditional(DebugMode)]
        public static void NotEqual(char* start, string invalid, string? message = null) =>
            True(new string(start, 0, invalid.Length) != invalid, $"Unexpected string '{Expand(start)}'.");

        [Conditional(DebugMode)]
        public static void Equal(char* start, string expected, string? message = null) =>
            Equal(new string(start, 0, expected.Length), expected, $"Unexpected string: '{Expand(start)}'.\r\nExpected: '{expected}'");

        [Conditional(DebugMode)]
        public static void Equal<T>(T actual, T expected, string? message = null) =>
            True(EqualityComparer<T>.Default.Equals(actual, expected), message ?? $"Unexpected {typeof(T).Name}: '{actual}'.\r\nExpected: '{expected}'");

        [Conditional(DebugMode)]
        public static void FormatEqual(char* start, string expected, bool hex = false, char wildcard = '0', params char[] valid)
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
        public static void Impossible(char* start) => Debug.Fail($"{Expand(start)} is impossible.");

        [Conditional(DebugMode)]
        public static void Dump(Token token)
        {
            if (token.Kind == TokenKind.EndOfLine)
            {
                _dumper.WriteLine();
            }
            else
            {
                _dumper.Write(token + " ");
            }

            _dumper.Flush();
        }

        [Conditional(DebugMode)]
        public static void Dump(params TokenKind[] tokenKinds)
        {
            foreach (var kind in tokenKinds)
            {
                Dump(new Token(kind));
            }
        }

        private static string Expand(char* start)
        {
            // Avoid going out of bounds
            var i = 0;
            while (start[i] is not '\0' && i < 100)
            {
                i++;
            }

            return new string(start, 0, i);
        }
    }
}
