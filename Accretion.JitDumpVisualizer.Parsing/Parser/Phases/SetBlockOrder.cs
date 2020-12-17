using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class SetBlockOrder : CompilationPhase
    {
        internal SetBlockOrder(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(SetBlockOrder);
    }
}
