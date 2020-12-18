using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    public static class Count
    {
        public static unsafe nint OfLeading(char* start, char* end, char character)
        {
            if ((nint)end - (nint)start >= 2 * Vector128<byte>.Count && Sse2.IsSupported)
            {
                var ptr = (ushort*)start;

                var vChar = Vector128.Create(character);

                var firstVector = Sse2.LoadVector128(ptr);
                var firstEqualityVector = Sse2.CompareEqual(firstVector, vChar);
                var firstMask = Sse2.MoveMask(firstEqualityVector.AsByte());

                var secondVector = Sse2.LoadVector128(ptr + Vector128<ushort>.Count);
                var secondEqualityVector = Sse2.CompareEqual(secondVector, vChar);
                var secondMask = Sse2.MoveMask(secondEqualityVector.AsByte());

                var mask = ~((secondMask << 16) | firstMask);
                if (mask != 0)
                {
                    return BitOperations.TrailingZeroCount(mask) >> 1;
                }
            }

            return OfLeadingSoftwareFallback(start, end, character);
        }

        private static unsafe nint OfLeadingSoftwareFallback(char* start, char* end, char character)
        {
            nint count = 0;
            while (start < end)
            {
                if (*start != character)
                {
                    return count;
                }

                count++;
                start++;
            }

            return count;
        }
    }
}
