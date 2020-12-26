using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class IndirectCallTransform : CompilationPhase
    {
        internal IndirectCallTransform(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(IndirectCallTransform);
    }
}
