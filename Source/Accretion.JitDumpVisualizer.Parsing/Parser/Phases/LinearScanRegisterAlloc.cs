using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class LinearScanRegisterAlloc : CompilationPhase
    {
        internal LinearScanRegisterAlloc(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(LinearScanRegisterAlloc);
    }
}
