using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class OptimizeIndexChecks : CompilationPhase
    {
        internal OptimizeIndexChecks(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(OptimizeIndexChecks);
    }
}
