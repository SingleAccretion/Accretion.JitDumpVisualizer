﻿using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class MorphPromoteStructs : CompilationPhase
    {
        internal MorphPromoteStructs(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(MorphPromoteStructs);
    }
}