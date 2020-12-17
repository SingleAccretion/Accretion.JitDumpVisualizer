using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class MergeCallfinallyChains : CompilationPhase
    {
        internal MergeCallfinallyChains(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(MergeCallfinallyChains);
    }
}
