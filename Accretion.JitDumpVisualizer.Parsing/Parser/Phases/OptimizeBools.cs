using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class OptimizeBools : CompilationPhase
    {
        internal OptimizeBools(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(OptimizeBools);
    }
}
