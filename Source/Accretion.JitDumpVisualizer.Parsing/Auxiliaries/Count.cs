using System;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    public static unsafe class Count
    {
        public static int OfLeading(char* start, char character)
        {
            int count = 0;
            while (start[count] == character)
            {
                count++;
            }

            Assert.True(start[count] != character);
            return count;
        }

        public static int IndexOf(char* start, char character)
        {
            if (Sse2.IsSupported)
            {
                var mask1 = 0u;
                var mask2 = 0u;
                var index = 0;
                var v = Vector128.Create(character);
                while ((mask1 | mask2) == 0)
                {
                    var v1 = Sse2.LoadVector128((ushort*)(start + index));
                    var v2 = Sse2.LoadVector128((ushort*)(start + index + Vector128<ushort>.Count));

                    v1 = Sse2.CompareEqual(v1, v);
                    v2 = Sse2.CompareEqual(v2, v);

                    mask1 = (uint)Sse2.MoveMask(v1.AsByte());
                    mask2 = (uint)Sse2.MoveMask(v2.AsByte());

                    index += 2 * Vector128<ushort>.Count;
                }

                return index + ((BitOperations.TrailingZeroCount(mask1) + BitOperations.TrailingZeroCount(mask2)) >> 1);
            }

            return new Span<char>(start, int.MaxValue).IndexOf(character);
        }
    }
}
