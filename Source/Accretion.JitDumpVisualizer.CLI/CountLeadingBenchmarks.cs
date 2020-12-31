using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace Accretion.JitDumpVisualizer.CLI
{
    [DisassemblyDiagnoser(maxDepth: 3)]
    public class CountLeadingBenchmarks
    {
        public IEnumerable<object[]> Data { get; } = new object[][]
        {
            new object[]{ "*************** ", '*' },
            new object[]{ "******          ", '*' },
            new object[]{ "***             ", '*' },
            new object[]{ "*               ", '*' },
        };

        public const int IterationCount = 100;

        [Benchmark(OperationsPerInvoke = IterationCount)]
        [ArgumentsSource(nameof(Data))]
        public unsafe nint ConsecutiveCount(string str, char ch)
        {
            fixed (char* start = str)
            {
                nint sum = 0;
                for (int i = 0; i < IterationCount; i++)
                {
                    sum += Count.OfLeading(start, ch);
                }

                return sum;
            }
        }
    }
}
