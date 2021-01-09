using System;
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

            Print("The item count is ", matrix.Height);
            var bitCount = (int)Math.Ceiling(Math.Log2(matrix.Height));

            Print("Started evaluating sets for bit count ", bitCount);
            List<BitColumn[]> sets;
            do
            {
                sets = Choose(matrix.Columns, bitCount);
                sets.RemoveAll(HasDuplicateRows);
                if (sets.Count is not 0)
                {
                    Print($"Found some unique sets for bit count ", bitCount);
                    break;
                }

                Print($"Failed to find a unique set, incrementing the bit count to ", ++bitCount);

            } while (true);

            var groupedSets = sets.GroupBy(CountNumberOfHoles).OrderBy(x => x.Key);
            Print("Grouped sets by the number of holes: ", string.Join(", ", groupedSets.Select(x => x.Key)));

            sets = groupedSets.First().ToList();
            Print("Group of sets with the least number of number of holes:\r\n", Stringify(sets));

            sets.Sort((x, y) => x.Max(z => z.OriginalIndex).CompareTo(y.Max(z => z.OriginalIndex)));
            Print("Sorted sets by proximity to the start of the input...");

            var winner = sets.First().Select(x => x.OriginalIndex).ToArray();
            Print("The winning bits are: ", string.Join(' ', winner));

            var values = ComputeEnumerationValues(originalMatrix.Columns, winner);
            Print($"The final values are: ", string.Join(", ", values.OrderBy(x => x)));

            return winner;
        }

        private static int CountNumberOfHoles(BitColumn[] columns)
        {
            var indecies = Enumerable.Range(0, columns.Length);
            var values = ComputeEnumerationValues(columns, indecies);

            var holes = -1;
            int? last = null;
            foreach (var item in values.OrderBy(x => x))
            {
                if (last != item - 1)
                {
                    holes++;
                }
                last = item;
            }

            return holes;
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

        static IEnumerable<int> ComputeEnumerationValues(IReadOnlyList<BitColumn> columns, IEnumerable<int> bitIndecies)
        {
            return ConvertToRows(columns).Select(x => ComputeEnumerationValue(x, bitIndecies));
        }

        private static int ComputeEnumerationValue(byte?[] row, IEnumerable<int> bitIndecies)
        {
            Debug.Assert(bitIndecies.Distinct().Count() == bitIndecies.Count());

            // We reverse string because the parsing is big-endian, while we will be reading bits as little-endian
            var binaryString = string.Join("", bitIndecies.Select(x => row[x]).Reverse());
            return Convert.ToInt32(binaryString, 2);
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

        private static void Print(string message, object value = null)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(message);
            Console.ResetColor();
            Console.Write(value);
            Console.WriteLine();
        }
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
