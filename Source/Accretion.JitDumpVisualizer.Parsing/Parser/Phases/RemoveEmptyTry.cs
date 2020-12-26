using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class RemoveEmptyTry : CompilationPhase
    {
        internal RemoveEmptyTry(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(RemoveEmptyTry);
    }
}
