using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class CalculateStackLevelSlots : CompilationPhase
    {
        internal CalculateStackLevelSlots(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(CalculateStackLevelSlots);
    }
}
