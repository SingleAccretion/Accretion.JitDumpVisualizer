using Accretion.JitDumpVisualizer.Parsing.Parser;
using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
            // 
            // MeasureNextThroughput();
            // return;
            // 
            // CollectDumpStats();
            // return;
            var rawDump = File.ReadAllText("dump.txt");
           
            // var list = new List<string>();
            // foreach (var line in rawDump.Split("\r\n").Where(x => x.Contains("*************** In ")))
            // {
            //     var index = line.IndexOf("*************** In ") + "*************** In ".Length;
            //     var lastIndex = line.IndexOf(')');
            //     if (lastIndex < 0)
            //     {
            //         lastIndex = line.Length - 1;
            //     }
            //     var phaseName = line[index..(lastIndex + 1)].Trim();
            //     list.Add(phaseName);
            // }
            // 
            // list = list.Distinct().OrderBy(x => x).ToList();
            // for (int i = 0; i < list.Count; i++)
            // {
            //     Console.WriteLine(list[i]);
            // }
            // return;

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
            var avg = 0L;
            fixed (char* start = rawDump)
            {
                while (true)
                {
                    var tokens = new TokenStream(start, rawDump.Length);

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
