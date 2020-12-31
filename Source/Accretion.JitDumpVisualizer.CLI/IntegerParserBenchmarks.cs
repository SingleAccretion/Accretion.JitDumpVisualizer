using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Attributes;

namespace Accretion.JitDumpVisualizer.CLI
{
    public unsafe class IntegerParserBenchmarks
    {
        private const string FourDigitNumber = "9876";
        private const string SixDigitNumber = "987654";

        public const int IterationCount = 1000;

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

        [Benchmark(Baseline = true, OperationsPerInvoke = IterationCount)]
        [Arguments(SixDigitNumber)]
        public void ParseSixDigitNumberBaseline(string str)
        {
            fixed (char* ptr = str)
            {
                for (int i = 0; i < IterationCount; i++)
                {
                    ParseIntegerSixDigitsBaseline(ptr);
                }
            }
        }

        [Benchmark(OperationsPerInvoke = IterationCount)]
        [Arguments(SixDigitNumber)]
        public void ParseSixDigitNumber(string str)
        {
            fixed (char* ptr = str)
            {
                for (int i = 0; i < IterationCount; i++)
                {
                    IntegersParser.ParseIntegerSixDigits(ptr);
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

        private static int ParseIntegerSixDigitsBaseline(char* start)
        {
            var d1 = start[0] - '0';
            var d2 = start[1] - '0';
            var d3 = start[2] - '0';
            var d4 = start[3] - '0';
            var d5 = start[4] - '0';
            var d6 = start[5] - '0';
            
            return d1 * 100_000 + d2 * 10_000 + d3 * 1000 + d4 * 100 + d5 * 10 + d6;
        }
    }
}
