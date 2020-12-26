using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class DetermineFirstColdBlock : CompilationPhase
    {
        internal DetermineFirstColdBlock(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(DetermineFirstColdBlock);
    }
}
