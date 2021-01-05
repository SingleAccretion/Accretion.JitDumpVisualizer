namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public enum GenTreeConstantIconHandle : byte
    {
        Unknown,
        Token,
        Method,
        Class,
        StringHandle,
        Scope,
        Field,
        Static,
        ConstantPointer,
        GlobalPointer,
        Vararg,
        PInvoke,
        ThreadLocalStorage,
        Function,
        CidOrMid,
        BasicBlockPointer
    }
}
