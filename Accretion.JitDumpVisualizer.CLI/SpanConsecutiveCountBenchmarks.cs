using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Accretion.JitDumpVisualizer.CLI
{
    [DisassemblyDiagnoser(maxDepth: 3)]
    public class SpanConsecutiveCountBenchmarks
    {
        public IEnumerable<object[]> Data { get; } = new object[][]
        {
            new object[]{ "*************** ", '*' },
            new object[]{ "******          ", '*' },
            new object[]{ "***             ", '*' },
            new object[]{ "*               ", '*' },
        };

        public const int Count = 100;

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public unsafe int ConsecutiveCount(string str, char ch)
        {
            var sum = 0;
            var span = str.AsSpan();
            for (int i = 0; i < Count; i++)
            {
                sum += span.ConsecutiveCount(ch);
            }

            return sum;
        }
    }
}
