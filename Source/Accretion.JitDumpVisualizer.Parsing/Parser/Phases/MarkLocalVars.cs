using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class MarkLocalVars : CompilationPhase
    {
        internal MarkLocalVars(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(MarkLocalVars);
    }
}
