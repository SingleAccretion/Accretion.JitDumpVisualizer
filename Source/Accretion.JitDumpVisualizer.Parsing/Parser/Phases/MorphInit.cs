using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class MorphInit : CompilationPhase
    {
        internal MorphInit(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(ExpandPatchpoints);
    }
}
