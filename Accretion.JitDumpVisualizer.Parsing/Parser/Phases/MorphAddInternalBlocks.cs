using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class MorphAddInternalBlocks : CompilationPhase
    {
        internal MorphAddInternalBlocks(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(MorphAddInternalBlocks);
    }
}
