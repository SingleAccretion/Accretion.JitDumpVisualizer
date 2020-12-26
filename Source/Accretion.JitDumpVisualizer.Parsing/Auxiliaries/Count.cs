using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    public static class Count
    {
        public static unsafe int OfLeading(char* start, char* end, char character)
        {
            // Note: the expectation is that most counts will be less than 16
            // However, there are cases (and they show up in profiles) where that is not true and the much slower is used for a long chain
            // There is a need for an investigation here
            // The best solution would be solve it semantically, so there is no need for lookahead at all
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
                    var result = BitOperations.TrailingZeroCount(mask) >> 1;
                    Assert.Equal(start, new string(character, result));

                    return result;
                }
            }

            return OfLeadingSoftwareFallback(start, character);
        }

        private static unsafe int OfLeadingSoftwareFallback(char* start, char character)
        {
            int count = 0;
            while (start[count] == character)
            {
                count++;
            }

            Assert.True(start[count] != character);
            return count;
        }
    }
}
