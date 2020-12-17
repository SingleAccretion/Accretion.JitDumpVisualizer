using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class UpdateFlowGraphEarlyPass : CompilationPhase
    {
        internal UpdateFlowGraphEarlyPass(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(UpdateFlowGraphEarlyPass);
    }
}
