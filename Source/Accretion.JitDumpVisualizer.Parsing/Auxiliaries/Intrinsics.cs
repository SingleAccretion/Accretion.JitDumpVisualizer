using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    internal static class Intrinsics
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int HorizontalSum(Vector128<int> vector)
        {
            var a_b_c_d = vector;
            var c_d_c_d = Sse2.UnpackHigh(vector.AsInt64(), vector.AsInt64()).AsInt32();
            var ac_bd_cc_dd = Sse2.Add(a_b_c_d, c_d_c_d);

            // A mental model for the mask is:
            // For this value in position {i} (little endian in code)
            // How much do I need to shift the original vector's values to get the desired element of the original vector be the first?
            // The answer is in the mask at position {i} (big endian in code)
            // For example, say we want to to (1, 2, 3, 4) -> (4, 3, 2, 3)
            // First desired element: 4, shifting by: 3, mask bits: 11
            // Second desired element: 3, shifting by: 2, mask bits: 10
            // Third desired element: 2, shifting by: 1, mask bits: 01
            // Fourth desired element: 3, shifting by: 2, mask bits: 10
            // Final mask: 0b_10_01_10_11
            var bd_ac_cc_dd = Sse2.ShuffleLow(ac_bd_cc_dd.AsUInt16(), 0b01_00_11_10).AsInt32();

            return Sse2.Add(ac_bd_cc_dd, bd_ac_cc_dd).GetElement(0);
        }
    }
}
