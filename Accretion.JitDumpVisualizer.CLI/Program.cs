using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using Accretion.JitDumpVisualizer.Parsing.Parser;
using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Running;
using ObjectLayoutInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Accretion.JitDumpVisualizer.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            MeasureNextThroughput();
            CollectDumpStats();

            MeasureNextPerformance();
            return;

            var rawDump = File.ReadAllText("dump.txt");

            using var sw = new StreamWriter("output.txt");
            var dump = new Dump(rawDump);
            foreach (var phase in dump.Phases)
            {
                sw.Write(phase.Name);
                if (phase.NoChanges)
                {
                    sw.Write(" - no changes");
                }
                sw.Write(Environment.NewLine);
            }
        }

        public static void MeasureNextThroughput()
        {
            var rawDump = new StreamReader("dump.txt").ReadToEnd();
            var count = 0;
            var avg = 0L;
            while (true)
            {
                var tokens = new TokenStream(rawDump);

                var watch = Stopwatch.StartNew();
                var n = 0;
                while (tokens.Next().Kind != TokenKind.EndOfFile)
                {
                    n++;
                }
                watch.Stop();
                count++;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"It took {watch.ElapsedMilliseconds} ms to process {n} tokens.");

                var throughput = n / watch.ElapsedMilliseconds;
                avg = ((count - 1) * avg + throughput) / count;
                var nsPerToken = 1000_000d / avg;

                Console.SetCursorPosition(0, 1);
                Console.WriteLine($"The average throughput is {nsPerToken:N0} ns/token.");
                watch.Reset();
            }
        }

        private static void MeasureNextPerformance() => BenchmarkRunner.Run<TokenStreamBenchmarks>();

        public static void CollectDumpStats()
        {
            var rawDump = File.ReadAllText("dump.txt");
            var tokenStats = new Dictionary<TokenKind, long>();
            var stream = new TokenStream(rawDump);

            while (stream.NextRaw() is { Kind: not TokenKind.EndOfFile } token)
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
    }
}
