using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class RationalizeIR : CompilationPhase
    {
        internal RationalizeIR(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(RationalizeIR);
    }
}
