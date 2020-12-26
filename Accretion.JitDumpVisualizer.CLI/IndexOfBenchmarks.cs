using BenchmarkDotNet.Attributes;

namespace Accretion.JitDumpVisualizer.CLI
{
    public class IndexOfBenchmarks
    {
        [Benchmark]
        [Arguments("N001 (  1,  1) [000088] ------------                 +--*  LCL_VAR   int    V08 tmp7         u:1\r\n")]
        public int IndexOfEndOfLine(string str)
        {
            return str.IndexOf('\n');
        }
    }
}
