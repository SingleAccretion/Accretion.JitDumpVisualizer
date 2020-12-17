using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class PostImport : CompilationPhase
    {
        internal PostImport(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(PostImport);
    }
}
