using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal static unsafe class IntegersParser
    {
        public static int ParseGenericInteger(char* start, out nint digitCount)
        {
            digitCount = 0;
            var value = 0;

            const int MaxDigitCount = 17;
            // Largest numbers in the dump have 17 digits (including the leading zero in "0x")
            // We overestimate here for safety
            var digits = stackalloc int[32];

            var integerBase = 10;
            while (true)
            {
                var delta = 0;
                var ch = *start;
                switch (ch)
                {
                    // This is correct even taking into account the "0x" prefix (as it will be counted as a leading digit)
                    case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                        delta = '0';
                        break;
                    case 'a' or 'b' or 'c' or 'd' or 'e' or 'f':
                        delta = 'a' - 10;
                        integerBase = 16;
                        break;
                    case 'B' or 'C' or 'D' or 'E' or 'F':
                        delta = 'A' - 10;
                        integerBase = 16;
                        break;
                    case 'x' or 'h':
                        delta = 0;
                        integerBase = 16;
                        break;
                    default:
                        Debug.Assert(digitCount <= MaxDigitCount);

                        var multiplier = 1;
                        for (nint i = digitCount - 1; i >= 0; i--)
                        {
                            value += digits[i] * multiplier;
                            multiplier *= integerBase;
                        }

                        return value;
                }

                Debug.Assert(digitCount < MaxDigitCount);
                if (delta != 0)
                {
                    digits[digitCount] = ch - delta;
                    digitCount++;
                }
                start++;
            }
        }

        public static int ParseIntegerTwoDigits(char* start)
        {
            VerifyAll(start, 2);

            var d1 = start[0] - '0';
            var d2 = start[1] - '0';

            return d2 * 10 + d1;
        }

        public static int ParseIntegerFourDigits(char* start)
        {
            VerifyAll(start, 4);

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
            VerifyAll(start, 5);

            var d1 = start[0] - '0';
            var d2 = start[1] - '0';
            var d3 = start[2] - '0';
            var d4 = start[3] - '0';
            var d5 = start[4] - '0';

            return d1 * 10_000 + d2 * 1000 + d3 * 100 + d4 * 10 + d5;
        }

        [Conditional("DEBUG")]
        private static void VerifyAll(char* start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var ch = start[i];
                Debug.Assert(char.IsDigit(ch), $"{ch} is not a digit.");
            }
        }
    }
}
