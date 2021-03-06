﻿namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public enum RyuJitPhase : byte
    {
        Unknown,
        PreImport,
        Importation,
        IndirectCallTransform,
        ExpandPatchpoints,
        PostImport,
        MorphInit,
        MorphInlining,
        AllocateObjects,
        MorphAddInternalBlocks,
        RemoveEmptyTry,
        RemoveEmptyFinally,
        MergeCallfinallyChains,
        CloneFinally,
        ComputePreds,
        MergeThrowBlocks,
        UpdateFlowGraphEarlyPass,
        MorphPromoteStructs,
        MorphStructsAddressExposed,
        MorphByRefs,
        MorphGlobal,
        GSCookie,
        ComputeEdgeWeights,
        CreateEHFunclets,
        OptimizeLayout,
        ComputeBlocksReachability,
        OptimizeLoops,
        CloneLoops,
        UnrollLoops,
        MarkLocalVars,
        OptimizeBools,
        FindOperOrder,
        SetBlockOrder,
        BuildSSARepresentation,
        EarlyValuePropagation,
        DoValueNumbering,
        HoistLoopCode,
        VNBasedCopyPropagation,
        OptimizeValnumCSEs,
        AssertionProp,
        OptimizeIndexChecks,
        InsertGCPolls,
        DetermineFirstColdBlock,
        RationalizeIR,
        DoSimpleLowering,
        LoweringNodeInfo,
        CalculateStackLevelSlots,
        LinearScanRegisterAlloc,
        GenerateCode,
        EmitCode,
        EmitGCPlusEHTables
    }
}