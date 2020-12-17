using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System.IO;

namespace Accretion.JitDumpVisualizer.CLI
{
    // [MemoryDiagnoser]
    [DisassemblyDiagnoser(maxDepth: 3)]
    // [HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.InstructionRetired, HardwareCounter.BranchMispredictions)]
    public class TokenStreamBenchmarks
    {
        private TokenStream _stream;
        private readonly string _dump;

        public TokenStreamBenchmarks()
        {
            _dump = File.ReadAllText("dump.txt");
            _stream = new(_dump);
        }

        [Benchmark]
        public void Next()
        {
            if (_stream.Next().Kind == TokenKind.EndOfFile)
            {
                _stream = new(_dump);
            }
        }
    }
}