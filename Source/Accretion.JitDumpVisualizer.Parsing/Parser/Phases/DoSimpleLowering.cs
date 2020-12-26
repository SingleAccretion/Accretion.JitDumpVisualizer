using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class DoSimpleLowering : CompilationPhase
    {
        internal DoSimpleLowering(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(DoSimpleLowering);
    }
}
