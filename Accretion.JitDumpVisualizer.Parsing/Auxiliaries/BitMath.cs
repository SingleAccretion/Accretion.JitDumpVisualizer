namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    internal static class BitMath
    {
        public static bool IsPow2(nint value) => (value & (value - 1)) == 0 && value != 0;
    }
}
