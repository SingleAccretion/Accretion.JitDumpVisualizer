using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser
{
    public abstract class CompilationPhase : DumpSection
    {
        private protected CompilationPhase(TokenSource tokens, bool noChanges) : base(tokens) => NoChanges = noChanges;

        public abstract string Name { get; }
        public bool NoChanges { get; }
    }
}
