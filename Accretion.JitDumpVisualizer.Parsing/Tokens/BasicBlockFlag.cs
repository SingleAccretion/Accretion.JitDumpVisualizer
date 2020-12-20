namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public enum BasicBlockFlag
    {
        Unknown,
        I,
        Label,
        Target,
        HasCall,
        NewObj,
        IdxLen,
        NullCheck,
        GCSafe,
        Internal,
        LIR
    }
}
