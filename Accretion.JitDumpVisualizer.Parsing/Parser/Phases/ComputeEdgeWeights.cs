using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class ComputeEdgeWeights : CompilationPhase
    {
        internal ComputeEdgeWeights(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(ComputeEdgeWeights);
    }
}
