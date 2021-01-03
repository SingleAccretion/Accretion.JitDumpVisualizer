using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static RyuJitFunction ParseRyuJitFunction(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<RyuJitFunction>() is 1);
            
            var result = (*start) switch
            {
                'c' => Result(RyuJitFunction.CompInitDebuggingInfo, "compInitDebuggingInfo()", start),
                'e' => (start[4]) switch
                {
                    'E' => Result(RyuJitFunction.EmitEndCodeGen, "emitEndCodeGen()", start),
                    _ => Result(RyuJitFunction.EmitJumpDistBind, "emitJumpDistBind()", start),
                },
                'f' => (start[2]) switch
                {
                    'C' => (start[3]) switch
                    {
                        'o' => (start[9]) switch
                        {
                            'B' => Result(RyuJitFunction.FgComputeBlockAndEdgeWeights, "fgComputeBlockAndEdgeWeights()", start),
                            'D' => Result(RyuJitFunction.FgComputeDoms, "fgComputeDoms", start),
                            'P' => Result(RyuJitFunction.FgComputePreds, "fgComputePreds()", start),
                            _ => Result(RyuJitFunction.FgComputeReachability, "fgComputeReachability", start),
                        },
                        _ => Result(RyuJitFunction.FgCreateFunclets, "fgCreateFunclets()", start),
                    },
                    'D' => (start[4]) switch
                    {
                        'b' => Result(RyuJitFunction.FgDebugCheckBBlist, "fgDebugCheckBBlist", start),
                        _ => Result(RyuJitFunction.FgDetermineFirstColdBlock, "fgDetermineFirstColdBlock()", start),
                    },
                    'E' => Result(RyuJitFunction.FgExpandRarelyRunBlocks, "fgExpandRarelyRunBlocks()", start),
                    'F' => (start[6]) switch
                    {
                        'B' => Result(RyuJitFunction.FgFindBasicBlocks, "fgFindBasicBlocks()", start),
                        _ => Result(RyuJitFunction.FgFindOperOrder, "fgFindOperOrder()", start),
                    },
                    'I' => Result(RyuJitFunction.FgInterBlockLocalVarLiveness, "fgInterBlockLocalVarLiveness()", start),
                    'L' => Result(RyuJitFunction.FgLocalVarLiveness, "fgLocalVarLiveness()", start),
                    'M' => (start[3]) switch
                    {
                        'a' => Result(RyuJitFunction.FgMarkAddressExposedLocals, "fgMarkAddressExposedLocals()", start),
                        _ => Result(RyuJitFunction.FgMorphBlocks, "fgMorphBlocks()", start),
                    },
                    'P' => (start[3]) switch
                    {
                        'e' => Result(RyuJitFunction.FgPerBlockLocalVarLiveness, "fgPerBlockLocalVarLiveness()", start),
                        _ => Result(RyuJitFunction.FgPromoteStructs, "fgPromoteStructs()", start),
                    },
                    'R' => (start[4]) switch
                    {
                        'm' => (start[13]) switch
                        {
                            'B' => Result(RyuJitFunction.FgRemoveEmptyBlocks, "fgRemoveEmptyBlocks", start),
                            _ => Result(RyuJitFunction.FgRemoveEmptyTry, "fgRemoveEmptyTry()", start),
                        },
                        'o' => Result(RyuJitFunction.FgReorderBlocks, "fgReorderBlocks()", start),
                        's' => Result(RyuJitFunction.FgResetImplicitByRefRefCount, "fgResetImplicitByRefRefCount()", start),
                        _ => Result(RyuJitFunction.FgRetypeImplicitByRefArgs, "fgRetypeImplicitByRefArgs()", start),
                    },
                    'S' => Result(RyuJitFunction.FgSetBlockOrder, "fgSetBlockOrder()", start),
                    'T' => Result(RyuJitFunction.FgTailMergeThrows, "fgTailMergeThrows", start),
                    'U' => Result(RyuJitFunction.FgUpdateFlowGraph, "fgUpdateFlowGraph()", start),
                    _ => Result(RyuJitFunction.FgValueNumber, "fgValueNumber()", start),
                },
                'g' => (start[1]) switch
                {
                    'c' => Result(RyuJitFunction.GcInfoBlockHdrSave, "gcInfoBlockHdrSave()", start),
                    _ => (start[3]) switch
                    {
                        'E' => Result(RyuJitFunction.GenEnregisterIncomingStackArgs, "genEnregisterIncomingStackArgs()", start),
                        'F' => (start[5]) switch
                        {
                            'E' => Result(RyuJitFunction.GenFnEpilog, "genFnEpilog()", start),
                            _ => (start[11]) switch
                            {
                                '(' => Result(RyuJitFunction.GenFnProlog, "genFnProlog()", start),
                                _ => Result(RyuJitFunction.GenFnPrologCalleeRegArgs, "genFnPrologCalleeRegArgs()", start),
                            },
                        },
                        'G' => Result(RyuJitFunction.GenGenerateCode, "genGenerateCode()", start),
                        'I' => Result(RyuJitFunction.GenIPmappingGen, "genIPmappingGen()", start),
                        _ => Result(RyuJitFunction.GenSetScopeInfo, "genSetScopeInfo()", start),
                    },
                },
                'i' => Result(RyuJitFunction.ImpImport, "impImport()", start),
                'L' => Result(RyuJitFunction.LinearScan_allocateRegisters, "LinearScan::allocateRegisters()", start),
                'l' => (start[3]) switch
                {
                    'A' => Result(RyuJitFunction.LvaAssignFrameOffsets, "lvaAssignFrameOffsets(FINAL_FRAME_LAYOUT)", start),
                    _ => Result(RyuJitFunction.LvaMarkLocalVars, "lvaMarkLocalVars()", start),
                },
                'o' => (start[3]) switch
                {
                    'A' => (start[4]) switch
                    {
                        'd' => Result(RyuJitFunction.OptAddCopies, "optAddCopies()", start),
                        _ => Result(RyuJitFunction.OptAssertionPropMain, "optAssertionPropMain()", start),
                    },
                    'C' => Result(RyuJitFunction.OptCloneLoops, "optCloneLoops()", start),
                    'E' => Result(RyuJitFunction.OptEarlyProp, "optEarlyProp()", start),
                    'O' => (start[11]) switch
                    {
                        'B' => Result(RyuJitFunction.OptOptimizeBools, "optOptimizeBools()", start),
                        'C' => Result(RyuJitFunction.OptOptimizeCSEs, "optOptimizeCSEs()", start),
                        'L' => (start[12]) switch
                        {
                            'a' => Result(RyuJitFunction.OptOptimizeLayout, "optOptimizeLayout()", start),
                            _ => Result(RyuJitFunction.OptOptimizeLoops, "optOptimizeLoops()", start),
                        },
                        _ => Result(RyuJitFunction.OptOptimizeValnumCSEs, "optOptimizeValnumCSEs()", start),
                    },
                    'R' => Result(RyuJitFunction.OptRemoveRedundantZeroInits, "optRemoveRedundantZeroInits()", start),
                    _ => Result(RyuJitFunction.OptVnCopyProp, "optVnCopyProp()", start),
                },
                'O' => Result(RyuJitFunction.OptimizeRangeChecks, "OptimizeRangeChecks()", start),
                _ => (start[12]) switch
                {
                    'B' => Result(RyuJitFunction.SsaBuilder_Build, "SsaBuilder::Build()", start),
                    'I' => Result(RyuJitFunction.SsaBuilder_InsertPhiFunctions, "SsaBuilder::InsertPhiFunctions()", start),
                    _ => Result(RyuJitFunction.SsaBuilder_RenameVariables, "SsaBuilder::RenameVariables()", start),
                },
            };

            return Result<RyuJitFunction>(result, out width);
        }
    }
}
