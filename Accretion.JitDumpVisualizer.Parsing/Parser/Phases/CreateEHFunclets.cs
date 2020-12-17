using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class CreateEHFunclets : CompilationPhase
    {
        internal CreateEHFunclets(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(CreateEHFunclets);
    }
}
