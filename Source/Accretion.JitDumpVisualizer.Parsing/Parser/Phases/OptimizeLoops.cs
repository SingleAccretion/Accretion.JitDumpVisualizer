using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class OptimizeLoops : CompilationPhase
    {
        internal OptimizeLoops(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(OptimizeLoops);
    }
}
