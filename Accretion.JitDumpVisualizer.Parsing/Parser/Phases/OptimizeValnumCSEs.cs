﻿using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class OptimizeValnumCSEs : CompilationPhase
    {
        internal OptimizeValnumCSEs(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(OptimizeValnumCSEs);
    }
}