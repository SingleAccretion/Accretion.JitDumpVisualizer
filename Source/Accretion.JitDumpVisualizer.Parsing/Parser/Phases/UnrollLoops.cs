using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class UnrollLoops : CompilationPhase
    {
        internal UnrollLoops(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(UnrollLoops);
    }
}
