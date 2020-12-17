using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class OptimizeLayout : CompilationPhase
    {
        internal OptimizeLayout(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(OptimizeLayout);
    }
}
