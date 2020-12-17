using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class BuildSSARepresentation : CompilationPhase
    {
        internal BuildSSARepresentation(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(BuildSSARepresentation);
    }
}
