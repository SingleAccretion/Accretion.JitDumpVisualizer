using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class LoweringNodeInfo : CompilationPhase
    {
        internal LoweringNodeInfo(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(LoweringNodeInfo);
    }
}
