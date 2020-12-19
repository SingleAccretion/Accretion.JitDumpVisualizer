﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public enum RyuJitFunction
    {
        Unknown,
        CompInitDebuggingInfo,
        EmitEndCodeGen,
        EmitJumpDistBind,
        FgComputeBlockAndEdgeWeights,
        FgComputeDoms,
        FgComputePreds,
        FgComputeReachability,
        FgCreateFunclets,
        FgDebugCheckBBlist,
        FgDetermineFirstColdBlock,
        FgExpandRarelyRunBlocks,
        FgFindBasicBlocks,
        FgFindOperOrder,
        FgInterBlockLocalVarLiveness,
        FgLocalVarLiveness,
        FgMarkAddressExposedLocals,
        FgMorphBlocks,
        FgPerBlockLocalVarLiveness,
        FgPromoteStructs,
        FgRemoveEmptyBlocks,
        FgRemoveEmptyTry,
        FgReorderBlocks,
        FgResetImplicitByRefRefCount,
        FgRetypeImplicitByRefArgs,
        FgSetBlockOrder,
        FgTailMergeThrows,
        FgUpdateFlowGraph,
        FgValueNumber,
        GcInfoBlockHdrSave,
        GenEnregisterIncomingStackArgs,
        GenFnEpilog,
        GenFnProlog,
        GenFnPrologCalleeRegArgs,
        GenGenerateCode,
        GenIPmappingGen,
        GenSetScopeInfo,
        ImpImport,
        LinearScan_allocateRegisters,
        LvaAssignFrameOffsets,
        LvaMarkLocalVars,
        OptAddCopies,
        OptAssertionPropMain,
        OptCloneLoops,
        OptEarlyProp,
        OptimizeRangeChecks,
        OptOptimizeBools,
        OptOptimizeCSEs,
        OptOptimizeLayout,
        OptOptimizeLoops,
        OptOptimizeValnumCSEs,
        OptRemoveRedundantZeroInits,
        OptVnCopyProp,
        SsaBuilder_Build,
        SsaBuilder_InsertPhiFunctions,
        SsaBuilder_RenameVariables,
    }
}