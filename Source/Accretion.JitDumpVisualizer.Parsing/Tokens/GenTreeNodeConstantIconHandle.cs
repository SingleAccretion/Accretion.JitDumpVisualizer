namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public enum GenTreeNodeConstantIconHandle : byte
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
