namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public enum BasicBlockJumpTargetKind : byte
    {
        Unknown,
        Conditional,
        Always,
        Return
    }
}
