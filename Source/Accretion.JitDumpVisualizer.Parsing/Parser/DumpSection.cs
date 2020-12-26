using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser
{
    public abstract class DumpSection
    {
        private protected DumpSection(Tokens.TokenSource tokens) => Tokens = tokens;

        private protected Tokens.TokenSource Tokens { get; }
    }
}
