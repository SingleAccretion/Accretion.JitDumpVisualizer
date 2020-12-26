using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class ExpandPatchpoints : CompilationPhase
    {
        internal ExpandPatchpoints(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(ExpandPatchpoints);
    }
}
