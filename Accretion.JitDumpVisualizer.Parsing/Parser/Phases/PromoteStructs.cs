using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class PromoteStructs : CompilationPhase
    {
        internal PromoteStructs(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(PromoteStructs);
    }
}
