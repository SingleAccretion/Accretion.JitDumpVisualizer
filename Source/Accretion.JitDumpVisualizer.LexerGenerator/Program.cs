﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Accretion.JitDumpVisualizer.LexerGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ComputeIndentifyingBitIndecies(new[]
            {
                "idx",
                "int",
                "lab",
                "tar",
                "has",
                "new",
                "nul",
                "gcs",
                "LIR",
            });
        }

        private static IReadOnlyList<int> ComputeIndentifyingBitIndecies(string[] strings)
        {
            Debug.Assert(strings.Select(x => x.Length).Distinct().Count() is 1);

            var items = strings.Select(Encoding.Unicode.GetBytes).ToArray();
            var originalMatrix = new BitMatrix(items);
            var matrix = new BitMatrix(items);

            Print("Raw bits:\r\n", matrix);
            matrix.Columns.RemoveAll(x => x.Bits.Distinct().Count() is 1);
            Print("Removed duplicate columns:\r\n", matrix);

            Print("The item count is: ", matrix.Height);
            var bitCount = (int)Math.Ceiling(Math.Log2(matrix.Height));

            Print("Started evaluating sets for bit count ", bitCount);
            List<BitColumn[]> sets;
            do
            {
                sets = Choose(matrix.Columns, bitCount);
                sets.RemoveAll(HasDuplicateRows);
                if (sets.Count is not 0)
                {
                    Print($"Found a unique set for bit count ", bitCount);
                    break;
                }

                Print($"Failed to find a unique set, incrementing the bit count to ", ++bitCount);

            } while (true);
            
            sets.Sort((x, y) => x.Max(z => z.OriginalIndex).CompareTo(y.Max(z => z.OriginalIndex)));
            Print("Sorted sets by proximity to the start of the input...");

            var winner = sets.First().Select(x => x.OriginalIndex).ToArray();
            Print("The winning bits are: ", string.Join(' ', winner));

            var values = ComputeEnumerationValues(originalMatrix, winner);
            Print($"The final values are: ", string.Join(", ", values));

            return winner;
        }

        private static bool HasDuplicateRows(BitColumn[] columns)
        {
            var rows = ConvertToRows(columns);

            return rows.Any(x => rows.Any(y => y.SequenceEqual(x) && !ReferenceEquals(y, x)));
        }

        private static List<T[]> Choose<T>(IReadOnlyList<T> values, int setSize)
        {
            Debug.Assert(values.Count >= setSize);
            var sets = new List<T[]>();

            foreach (var indexSet in ChooseIndecies(values.Count, setSize))
            {
                sets.Add(indexSet.Select(x => values[x]).ToArray());
            }

            return sets;
        }

        private static IEnumerable<int[]> ChooseIndecies(int n, int k)
        {
            Debug.Assert(n >= k);

            if (k == n)
            {
                yield return Enumerable.Range(0, n).ToArray();
            }
            else if (k is 1)
            {
                foreach (var index in Enumerable.Range(0, n))
                {
                    yield return new[] { index };
                }
            }
            else
            {
                foreach (var set in ChooseIndecies(n - 1, k - 1))
                {
                    yield return set.Append(n - 1).ToArray();
                }

                foreach (var set in ChooseIndecies(n - 1, k))
                {
                    yield return set;
                }
            }
        }

        private static void Print(string message, object value = null)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(message);
            Console.ResetColor();
            Console.Write(value);
            Console.WriteLine();
        }

        private static IEnumerable<int> ComputeEnumerationValues(BitMatrix matrix, int[] bitIndecies)
        {
            var rows = ConvertToRows(matrix.Columns);
            var binaryValues = rows.Select(r => string.Join("", bitIndecies.Select(x => r[x])));
            
            return binaryValues.Select(x => Convert.ToInt32(string.Join("", x.Reverse()), 2)).OrderBy(x => x);
        }

        private static List<byte?[]> ConvertToRows(IReadOnlyList<BitColumn> columns)
        {
            Debug.Assert(columns.All(x => x.Bits.Length == columns[0].Bits.Length));

            var height = columns[0].Bits.Length;
            var width = columns.Count;

            var rows = new List<byte?[]>();
            for (int i = 0; i < height; i++)
            {
                rows.Add(new byte?[width]);
                for (int j = 0; j < columns.Count; j++)
                {
                    rows[i][j] = columns[j].Bits[i];
                }
            }

            return rows;
        }

        private static string Stringify(IEnumerable<BitColumn[]> sets) => string.Join(' ', sets.Select(x => $"({string.Join<BitColumn>(',', x)})"));
    }

    public class BitMatrix
    {
        public BitMatrix(byte[][] rows)
        {
            var width = rows.Max(x => x.Length); ;
            var height = rows.Length;

            var tempColumn = new byte?[height];
            Columns = new List<BitColumn>(rows.Length);
            for (int j = 0; j < width; j++)
            {
                for (int s = 0; s < 8; s++)
                {
                    for (int i = 0; i < height; i++)
                    {
                        var row = rows[i];
                        tempColumn[i] = row.Length <= j ? null : (byte)((row[j] >> s) & 1);
                    }
                    Columns.Add(new BitColumn(tempColumn, 8 * j + s));
                }
            }
            Height = height;
        }

        public List<BitColumn> Columns { get; }
        public int Height { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var columnWidth = Columns[^1].OriginalIndex.ToString().Length + 1;
            var format = $"{{0,-{columnWidth}}}";

            var columns = Columns;
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < columns.Count; j++)
                {
                    var bit = columns[j].Bits[i] is byte real ? $"{real}" : "x";
                    builder.AppendFormat(format, bit);
                }
                builder.AppendLine();
            }
            for (int j = 0; j < columns.Count; j++)
            {
                builder.AppendFormat(format, columns[j].OriginalIndex);
            }

            return builder.ToString();
        }
    }

    public class BitColumn
    {
        public BitColumn(byte?[] bits, int originalIndex)
        {
            Bits = (byte?[])bits.Clone();
            OriginalIndex = originalIndex;
        }

        public int OriginalIndex { get; }
        public byte?[] Bits { get; }

        public override string ToString() => $"{OriginalIndex}";
    }
}