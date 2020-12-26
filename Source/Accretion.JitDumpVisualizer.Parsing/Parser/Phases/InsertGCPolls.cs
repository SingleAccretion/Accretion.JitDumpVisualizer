using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class InsertGCPolls : CompilationPhase
    {
        internal InsertGCPolls(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(InsertGCPolls);
    }
}
