using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class DoValueNumbering : CompilationPhase
    {
        internal DoValueNumbering(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(DoValueNumbering);
    }
}
