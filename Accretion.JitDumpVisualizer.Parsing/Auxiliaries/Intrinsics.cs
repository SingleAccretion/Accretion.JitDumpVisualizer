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
            // For this group of vector values in position {i} (little endian in code)
            // How much do I need to shift the original vector's values?
            // The answer is in the mask at position {i} (big endian in code)
            var bd_ac_cc_dd = Sse2.ShuffleLow(ac_bd_cc_dd.AsUInt16(), 0b01_00_11_10).AsInt32();

            return Sse2.Add(ac_bd_cc_dd, bd_ac_cc_dd).GetElement(0);
        }
    }
}
