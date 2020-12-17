using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class Importation : CompilationPhase
    {
        internal Importation(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(Importation);
    }
}
