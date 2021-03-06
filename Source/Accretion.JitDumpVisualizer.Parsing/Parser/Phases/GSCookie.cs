﻿using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser.Phases
{
    public sealed class GSCookie : CompilationPhase
    {
        internal GSCookie(TokenSource tokens, bool noChanges) : base(tokens, noChanges) { }

        public override string Name => nameof(GSCookie);
    }
}
