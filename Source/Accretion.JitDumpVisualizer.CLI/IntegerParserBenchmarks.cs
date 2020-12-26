using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Attributes;

namespace Accretion.JitDumpVisualizer.CLI
{
    public unsafe class IntegerParserBenchmarks
    {
        private const string FourDigitNumber = "9876";

        public const int IterationCount = 1000;

        [Benchmark(Baseline = true, OperationsPerInvoke = IterationCount)]
        [Arguments(FourDigitNumber)]
        public void ParseFourDigitNumberBaseline(string str)
        {
            fixed (char* ptr = str)
            {
                for (int i = 0; i < IterationCount; i++)
                {
                    ParseIntegerFourDigitsBaseline(ptr);
                }
            }
        }

        [Benchmark(OperationsPerInvoke = IterationCount)]
        [Arguments(FourDigitNumber)]
        public void ParseFourDigitNumber(string str)
        {
            fixed (char* ptr = str)
            {
                for (int i = 0; i < IterationCount; i++)
                {
                    IntegersParser.ParseIntegerFourDigits(ptr);
                }
            }
        }

        private static int ParseIntegerFourDigitsBaseline(char* start)
        {
            var d1 = start[0] - '0';
            var d2 = start[1] - '0';
            var d3 = start[2] - '0';
            var d4 = start[3] - '0';

            return d1 * 1000 + d2 * 100 + d3 * 10 + d4;
        }
    }
}
