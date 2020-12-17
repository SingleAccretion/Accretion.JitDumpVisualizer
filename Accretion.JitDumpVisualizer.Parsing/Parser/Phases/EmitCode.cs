using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class EmitCode : CompilationPhase
    {
        internal EmitCode(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(EmitCode);
    }
}
