﻿namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public enum GenTreeNodeType : byte
    {
        Unknown,
        Void,
        Bool,
        Byte,
        UByte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Float,
        Double,
        Ref,
        Byref,
        Struct,
        Blk,
        LclBlk,
        Simd8,
        Simd12,
        Simd16,
        Simd32,
        Undefined
    }
}