using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class HoistLoopCode : CompilationPhase
    {
        internal HoistLoopCode(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(HoistLoopCode);
    }
}
