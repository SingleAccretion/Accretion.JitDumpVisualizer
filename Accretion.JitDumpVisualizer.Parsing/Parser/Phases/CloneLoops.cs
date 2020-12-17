using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class CloneLoops : CompilationPhase
    {
        internal CloneLoops(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(CloneLoops);
    }
}
