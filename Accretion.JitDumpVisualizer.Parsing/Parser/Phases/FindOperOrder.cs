﻿using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class FindOperOrder : CompilationPhase
    {
        internal FindOperOrder(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(FindOperOrder);
    }
}