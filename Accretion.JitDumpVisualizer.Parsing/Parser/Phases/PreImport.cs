using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class PreImport : CompilationPhase
    {
        internal PreImport(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(PreImport);
    }
}
