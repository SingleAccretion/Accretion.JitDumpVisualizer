using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Attributes;

namespace Accretion.JitDumpVisualizer.CLI
{
    public unsafe class IntegerParserBenchmarks
    {
        private const string FourDigitNumber = "9876";

        public const int IterationCount = 1000;

        [Benchmark(OperationsPerInvoke = IterationCount)]
        [Arguments(FourDigitNumber)]
        public void ParseFourDigitNumberGeneric(string  str)
        {
            fixed (char* ptr = str)
            {
                for (int i = 0; i < IterationCount; i++)
                {
                    IntegersParser.ParseGenericInteger(ptr, out _);
                }
            }
        }

        [Benchmark(OperationsPerInvoke = IterationCount)]
        [Arguments(FourDigitNumber)]
        public void ParseFourDigitNumberSpecialized(string str)
        {
            fixed (char* ptr = str)
            {
                for (int i = 0; i < IterationCount; i++)
                {
                    IntegersParser.ParseIntegerFourDigits(ptr);
                }
            }
        }

        [Benchmark(OperationsPerInvoke = IterationCount)]
        [Arguments(FourDigitNumber)]
        public void ParseFourDigitNumberVectorized(string str)
        {
            fixed (char* ptr = str)
            {
                for (int i = 0; i < IterationCount; i++)
                {
                    IntegersParser.ParseIntegerFourDigitsVectorized(ptr);
                }
            }
        }
    }
}
