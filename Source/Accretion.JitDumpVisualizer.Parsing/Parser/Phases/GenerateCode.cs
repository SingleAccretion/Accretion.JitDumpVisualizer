using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class GenerateCode : CompilationPhase
    {
        internal GenerateCode(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(GenerateCode);
    }
}
