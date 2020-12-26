using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class ComputeBlocksReachability : CompilationPhase
    {
        internal ComputeBlocksReachability(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(ComputeBlocksReachability);
    }
}
