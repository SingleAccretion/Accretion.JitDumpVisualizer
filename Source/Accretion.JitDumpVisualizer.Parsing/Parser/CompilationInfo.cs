using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser
{
    public sealed class CompilationInfo : DumpSection
    {
        internal CompilationInfo(TokenSource tokens) : base(tokens)
        {
        }
    }
}