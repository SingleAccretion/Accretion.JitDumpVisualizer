using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal static unsafe class IntegersParser
    {
        public static int ParseGenericInteger(char* start, out int width)
        {
            width = 0;
            while (*start is ' ')
            {
                start++;
                width++;
            }

            nint digitCount = 0;
            var digits = stackalloc int[32];
            while ((uint)(*start - '0') is <= 9 and var digit)
            {
                digits[digitCount] = (int)digit;
                digitCount++;
                width++;
                start++;
            }

            Assert.True(digitCount < 32);
            Assert.True(digitCount > 0);

            var result = 0;
            var multiplier = 1;
            for (nint i = digitCount - 1; i >= 0; i--)
            {
                result += digits[i] * multiplier;
                multiplier *= 10;
            }

            return result;
        }

        public static float ParseGenericFloat(char* start, out int digitCount)
        {
            digitCount = 0;
            bool isFraction = false;
            while (true)
            {
                switch (start[digitCount])
                {
                    case <= '9' and >= '0': break;
                    case '.':
                        Assert.True(!isFraction, "There cannot be two");
                        isFraction = true;
                        break;
                    default:
                        Assert.True(digitCount != 0);
                        return float.Parse(new ReadOnlySpan<char>(start, (int)digitCount));
                }

                digitCount++;
            }
        }

        public static int ParseIntegerTwoDigits(char* start)
        {
            VerifyAllDecimal(start, 2);

            var d1 = start[0] - '0';
            var d2 = start[1] - '0';

            return d1 * 10 + d2;
        }

        public static int ParseIntegerThreeDigits(char* start)
        {
            VerifyAllDecimal(start, 3);

            var d1 = start[0] - '0';
            var d2 = start[1] - '0';
            var d3 = start[2] - '0';

            return d1 * 100 + d2 * 10 + d3;
        }

        public static int ParseIntegerFourDigits(char* start)
        {
            VerifyAllDecimal(start, 4);

            if (Sse2.IsSupported)
            {
                var characters = Sse2.LoadScalarVector128((long*)start).AsInt16();
                var values = Sse2.Subtract(characters, Vector128.Create('0').AsInt16());
                var result = Sse2.MultiplyAddAdjacent(values, Vector128.Create(1000, 100, 10, 1, 0, 0, 0, 0));

                return result.GetElement(0) + result.GetElement(1);
            }

            return ParseGenericInteger(start, out _);
        }

        public static int ParseIntegerFiveDigits(char* start)
        {
            VerifyAllDecimal(start, 5);

            var d1 = start[0] - '0';
            var d2 = start[1] - '0';
            var d3 = start[2] - '0';
            var d4 = start[3] - '0';
            var d5 = start[4] - '0';

            return d1 * 10_000 + d2 * 1000 + d3 * 100 + d4 * 10 + d5;
        }

        public static int ParseIntegerSixDigits(char* start)
        {
            VerifyAllDecimal(start, 6);

            var d1 = start[0] - '0';
            var d2 = start[1] - '0';
            var d3 = start[2] - '0';
            var d4 = start[3] - '0';
            var d5 = start[4] - '0';
            var d6 = start[5] - '0';

            return d1 * 100_000 + d2 * 10_000 + d3 * 1000 + d4 * 100 + d5 * 10 + d6;
        }

        internal static int ParseHexIntegerThreeDigits(char* start)
        {
            VerifyAllHex(start, 3);

            var d1 = ToHexDigit(start[0]);
            var d2 = ToHexDigit(start[1]);
            var d3 = ToHexDigit(start[2]);

            return d1 * 16 * 16 + d2 * 16 + d3;
        }

        private static int ToHexDigit(char ch) => ch switch
        {
            >= 'a' and <= 'f' => ch - 'a',
            >= 'A' and <= 'F' => ch - 'A',
            _ => ch - '0'
        };

        [Conditional(Assert.DebugMode)]
        private static void VerifyAllDecimal(char* start, int count) => VerifyAll(start, count, "0123456789");

        [Conditional(Assert.DebugMode)]
        private static void VerifyAllHex(char* start, int count) => VerifyAll(start, count, "0123456789abcedfABCDEF");

        private static void VerifyAll(char* start, int count, string set)
        {
            for (int i = 0; i < count; i++)
            {
                var ch = start[i];
                Assert.True(set.Contains(ch), $"'{ch}' in '{new string(start, 0, count)}' is not a digit.");
            }
        }
    }
}
