using Accretion.JitDumpVisualizer.Parsing.Parser;
using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Accretion.JitDumpVisualizer.CLI
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
#if RELEASE
            BenchmarkRunner.Run<TokenStreamBenchmarks>();
            return;
#endif
            // MeasureNextThroughput();
            // return;
            // 
            // CollectDumpStats();
            // return;
            var rawDump = File.ReadAllText("dump.txt");

            using var sw = new StreamWriter("output.txt");
            var dump = new Dump(rawDump);
            var stream = new TokenStream(rawDump);
            while (stream.Next() is { Kind: not TokenKind.EndOfFile } token)
            {
                if (token.Kind == TokenKind.EndOfLine)
                {
                    sw.Write(Environment.NewLine);
                }
                else
                {
                    sw.Write($"{token} ");
                    sw.Flush();
                }
            }
        }

        public unsafe static void MeasureNextThroughput()
        {
            var rawDump = new StreamReader("dump.txt").ReadToEnd();
            var count = 0;
            double avg = 0L;
            fixed (char* start = rawDump)
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                while (true)
                {
                    var watch = Stopwatch.StartNew();
                    long n = 0;
                    const int Multiplier = 10;
                    for (int i = 0; i < Multiplier; i++)
                    {
                        var tokens = new TokenStream(start, rawDump.Length);
                        while (tokens.Next().Kind != TokenKind.EndOfFile)
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
        }

        public static void CollectDumpStats()
        {
            var rawDump = File.ReadAllText("dump.txt");
            var tokenStats = new Dictionary<TokenKind, long>();
            var stream = new TokenStream(rawDump);

            while (stream.Next() is { Kind: not TokenKind.EndOfFile } token)
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
    }
}
