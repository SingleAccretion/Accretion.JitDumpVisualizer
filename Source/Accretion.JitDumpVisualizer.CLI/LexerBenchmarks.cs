using Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing;
using BenchmarkDotNet.Attributes;

namespace Accretion.JitDumpVisualizer.CLI
{
    public unsafe class LexerBenchmarks
    {
        [Benchmark]
        [Arguments("HELPER.CORINFO_HELP_ARE_TYPES_EQUIVALENT")]
        public void ParseHelper(string str)
        {
            fixed (char* pointer = &str.GetPinnableReference())
            {
                Lexer.ParseRyuJitHelperMethod(pointer, out _);
            }
        }
    }
}
