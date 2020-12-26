using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class MorphByRefs : CompilationPhase
    {
        internal MorphByRefs(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(MorphByRefs);
    }
}
