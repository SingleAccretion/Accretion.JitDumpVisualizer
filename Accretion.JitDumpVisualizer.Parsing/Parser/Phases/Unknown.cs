using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class Unknown : CompilationPhase
    {
        internal Unknown(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(Unknown);
    }
}
