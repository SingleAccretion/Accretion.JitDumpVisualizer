using System;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    internal static class Throw
    {
        public static T InvalidOperationException<T>(string messafe) => throw new InvalidOperationException(messafe);
    }
}
