using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class AllocateObjects : CompilationPhase
    {
        internal AllocateObjects(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(AllocateObjects);
    }
}