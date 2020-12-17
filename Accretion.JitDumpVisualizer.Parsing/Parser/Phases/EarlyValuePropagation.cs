using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class EarlyValuePropagation : CompilationPhase
    {
        internal EarlyValuePropagation(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(EarlyValuePropagation);
    }
}
