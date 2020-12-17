using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class RemoveEmptyFinally : CompilationPhase
    {
        internal RemoveEmptyFinally(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(RemoveEmptyFinally);
    }
}
