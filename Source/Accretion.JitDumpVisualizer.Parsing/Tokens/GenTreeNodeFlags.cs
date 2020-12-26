using System;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    [Flags]
    public enum GenTreeNodeFlags
    {
        None = 0b0,
        // First flag
        I = 0b1,
        H = 0b10,
        Hash = 0b100,
        D = 0b1000,
        n = 0b10000,
        J = 0b100000,
        Star = 0b1000000,
        // Second flag
        A = 0b10000000,
        c = 0b100000000,
        // Third flag
        C = 0b1000000000,
        // Fourth flag
        X = 0b10000000000,
        // Fifth flag
        G = 0b100000000000,
        // Sixth flag
        O = 0b1000000000000,
        Plus = 0b10000000000000,
        // Eighth flag
        N = 0b100000000000000,
        // Ninth flag
        R = 0b1000000000000000,
        // Eleventh flag
        L = 0b10000000000000000
    }
}
