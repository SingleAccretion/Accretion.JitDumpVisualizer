using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using BenchmarkDotNet.Attributes;

namespace Accretion.JitDumpVisualizer.CLI
{
    public unsafe class IndexOfBenchmarks
    {
        [Benchmark]
        [Arguments("System.Collections.Generic.Queue`1[Token][Accretion.JitDumpVisualizer.Parsing.Tokens.Token]..ctor (exactContextHnd=0x00000000D1FFAB1E)")]
        public int IndexOfBCL(string str)
        {
            return str.IndexOf(' ');
        }

        [Benchmark]
        [Arguments("System.Collections.Generic.Queue`1[Token][Accretion.JitDumpVisualizer.Parsing.Tokens.Token]..ctor (exactContextHnd=0x00000000D1FFAB1E)")]
        public int IndexOf(string str)
        {
            fixed (char* strPtr = str)
            {
                return Count.IndexOf(strPtr, ' ');
            }
        }
    }
}
