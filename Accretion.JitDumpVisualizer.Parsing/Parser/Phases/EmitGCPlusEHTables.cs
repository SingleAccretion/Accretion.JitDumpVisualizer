using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class EmitGCPlusEHTables : CompilationPhase
    {
        internal EmitGCPlusEHTables(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(EmitGCPlusEHTables);
    }
}
