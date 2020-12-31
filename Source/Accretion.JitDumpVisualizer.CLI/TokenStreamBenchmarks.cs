using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System.IO;
using System.Runtime.InteropServices;

namespace Accretion.JitDumpVisualizer.CLI
{
    // [MemoryDiagnoser]
    [DisassemblyDiagnoser(maxDepth: 3)]
    // [HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.InstructionRetired, HardwareCounter.BranchMispredictions)]
    public unsafe class TokenStreamBenchmarks
    {
        private readonly string _dump;

        public TokenStreamBenchmarks()
        {
            _dump = File.ReadAllText("dump.txt");
        }

        [Benchmark]
        public void TokenizeWholeDump()
        {
            Tokenizer.Tokenize(_dump);
        }

        [Benchmark(Baseline = true)]
        public long Baseline()
        {
            long a1 = 0;
            long a2 = 0;
            long a3 = 0;
            long counter = 2_000_000;
            while (counter >= 0)
            {
                a1++;
                a2++;
                a3++;
                a1 *= a2;
                a2 *= a3;
                a3 *= a1;
                counter--;
            }

            return a1 + a2 + a3 + counter;
        }
    }
}