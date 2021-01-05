using Accretion.JitDumpVisualizer.Parsing.IO;
using Accretion.JitDumpVisualizer.Parsing.Parser;
using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.Tracing.Parsers.JScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Accretion.JitDumpVisualizer.CLI
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
#if RELEASE
            BenchmarkRunner.Run<LexerBenchmarks>();
            return;
            
            MeasureNextThroughput();
            return;
#endif
            Tokenizer.Tokenize(new FileReader(File.OpenRead("dump.txt")));
        }

        public unsafe static void MeasureNextThroughput()
        {
            var count = 0;
            double avg = 0L;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (true)
            {
                var watch = Stopwatch.StartNew();
                long n = 0;
                const int Multiplier = 10;
                for (int i = 0; i < Multiplier; i++)
                {
                    using var dumpReader = File.OpenRead("dump.txt");
                    var tokens = Tokenizer.Tokenize(new FileReader(dumpReader));
                    foreach (var token in tokens)
                    {
                        n++;
                    }
                }
                watch.Stop();
                var elapsed = (double)watch.ElapsedMilliseconds;

                n /= Multiplier;
                elapsed /= Multiplier;
                count++;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"It took {elapsed:0.00} ms to process {n} tokens.");

                var throughput = n / elapsed;
                avg = ((count - 1) * avg + throughput) / count;
                var nsPerToken = 1000_000d / avg;

                Console.SetCursorPosition(0, 1);
                Console.WriteLine($"The average throughput is {nsPerToken:N0} ns/token.");
                watch.Reset();
            }
        }

        public static void CollectDumpStats()
        {
            var dumpReader = File.OpenRead("dump.txt");
            var tokenStats = new Dictionary<TokenKind, long>();
            var tokens = Tokenizer.Tokenize(new FileReader(dumpReader));

            foreach (var token in tokens)
            {
                if (tokenStats.ContainsKey(token.Kind))
                {
                    tokenStats[token.Kind]++;
                }
                else
                {
                    tokenStats.Add(token.Kind, 1);
                }
            }

            DisplayStats(tokenStats);

            var rawDump = File.ReadAllText("dump.txt");
            var charStats = new Dictionary<string, long>();
            foreach (var ch in rawDump)
            {
                var normalized = ch == '\r' ? "\\r" :
                                 ch == '\n' ? "\\n" :
                                 ch == '\t' ? "\\t" :
                                 ch.ToString();

                if (charStats.ContainsKey(normalized))
                {
                    charStats[normalized]++;
                }
                else
                {
                    charStats.Add(normalized, 1);
                }
            }

            DisplayStats(charStats);
        }

        private static void CollectTypesInDumpStats()
        {
            var types = new List<string>();
            foreach (var rawDump in Directory.EnumerateFiles("Dumps", "*").Select(x => File.ReadAllText(x)))
            {
                var span = rawDump.AsSpan();
                while (span.IndexOf("struct<") is > 0 and var start)
                {
                    span = span.Slice(start + "struct<".Length);
                    var end = span.IndexOf('>');

                    types.Add(span[..end].ToString());
                }
            }

            types = types.Distinct().OrderBy(x => x.Length).ToList();

            foreach (var type in types)
            {
                Console.WriteLine(type.Length);
            }
        }

        private static void DisplayStats<T>(Dictionary<T, long> stats)
        {
            var totalCount = stats.Select(x => x.Value).Sum();
            foreach (var (category, count) in stats.OrderByDescending(x => x.Value))
            {
                var percentage = (double)count / totalCount * 100;
                Console.WriteLine($"{category,-40} {count,-7}{percentage:N2}%");
            }
            Console.WriteLine($"Total: {totalCount,40}");
            Console.WriteLine();
        }

        private static void MeasureAllocationStratsLatency(int byteCount)
        {
            var native = Stopwatch.StartNew();
            var a = (byte*)Marshal.AllocHGlobal(byteCount);
            native.Stop();

            var managed = Stopwatch.StartNew();
            var b = new byte[byteCount];
            managed.Stop();

            var managedUninitializedPinned = Stopwatch.StartNew();
            var c = GC.AllocateUninitializedArray<byte>(byteCount, pinned: true);
            managedUninitializedPinned.Stop();

            var managedUninitialized = Stopwatch.StartNew();
            var d = GC.AllocateUninitializedArray<byte>(byteCount);
            managedUninitialized.Stop();

            Console.WriteLine($"Native: {native.Elapsed.TotalMilliseconds}");
            Console.WriteLine($"Managed: {managed.Elapsed.TotalMilliseconds}");
            Console.WriteLine($"Managed uninitialized: {managedUninitialized.Elapsed.TotalMilliseconds}");
            Console.WriteLine($"Managed uninitialized pinned: {managedUninitializedPinned.Elapsed.TotalMilliseconds}");

            while (true)
            {
                for (int i = 0; i < byteCount; i++)
                {
                    a[i] += 1;
                    c[i] += 1;
                }
            }
        }
    }
}
