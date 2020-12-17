using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class MergeThrowBlocks : CompilationPhase
    {
        internal MergeThrowBlocks(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(MergeThrowBlocks);
    }
}
