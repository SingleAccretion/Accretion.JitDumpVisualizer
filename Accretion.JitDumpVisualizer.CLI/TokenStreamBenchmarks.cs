﻿using Accretion.JitDumpVisualizer.Parsing.Tokens;
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
        private readonly GCHandle _dumpHandle;

        public TokenStreamBenchmarks()
        {
            _dump = File.ReadAllText("dump.txt");
            _dumpHandle = GCHandle.Alloc(_dump, GCHandleType.Pinned);
        }

        [Benchmark(OperationsPerInvoke = 1000)]
        public void Next()
        {
            var stream = new TokenStream((char*)_dumpHandle.AddrOfPinnedObject(), _dump.Length);

            for (int i = 0; i < 1000; i++)
            {
                stream.Next();
            }
        }
    }
}