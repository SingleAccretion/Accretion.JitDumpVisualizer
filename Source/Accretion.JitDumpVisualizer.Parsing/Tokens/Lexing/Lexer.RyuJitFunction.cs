using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static RyuJitFunction ParseRyuJitFunction(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<RyuJitFunction>() is 1);

            ulong result;
            switch (*start)
            {
                case 'c':
                    Assert.Equal(start, "compInitDebuggingInfo()");
                    result = (ulong)RyuJitFunction.CompInitDebuggingInfo | ((ulong)"compInitDebuggingInfo()".Length << (sizeof(RyuJitFunction) * 8));
                    break;
                case 'e':
                    switch (start[4])
                    {
                        case 'E':
                            Assert.Equal(start, "emitEndCodeGen()");
                            result = (ulong)RyuJitFunction.EmitEndCodeGen | ((ulong)"emitEndCodeGen()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "emitJumpDistBind()");
                            result = (ulong)RyuJitFunction.EmitJumpDistBind | ((ulong)"emitJumpDistBind()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                    }
                    break;
                case 'f':
                    switch (start[2])
                    {
                        case 'C':
                            switch (start[3])
                            {
                                case 'o':
                                    switch (start[9])
                                    {
                                        case 'B':
                                            Assert.Equal(start, "fgComputeBlockAndEdgeWeights()");
                                            result = (ulong)RyuJitFunction.FgComputeBlockAndEdgeWeights | ((ulong)"fgComputeBlockAndEdgeWeights()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        case 'D':
                                            Assert.Equal(start, "fgComputeDoms");
                                            result = (ulong)RyuJitFunction.FgComputeDoms | ((ulong)"fgComputeDoms".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        case 'P':
                                            Assert.Equal(start, "fgComputePreds()");
                                            result = (ulong)RyuJitFunction.FgComputePreds | ((ulong)"fgComputePreds()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "fgComputeReachability");
                                            result = (ulong)RyuJitFunction.FgComputeReachability | ((ulong)"fgComputeReachability".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "fgCreateFunclets()");
                                    result = (ulong)RyuJitFunction.FgCreateFunclets | ((ulong)"fgCreateFunclets()".Length << (sizeof(RyuJitFunction) * 8));
                                    break; ;
                            }
                            break;
                        case 'D':
                            switch (start[4])
                            {
                                case 'b':
                                    Assert.Equal(start, "fgDebugCheckBBlist");
                                    result = (ulong)RyuJitFunction.FgDebugCheckBBlist | ((ulong)"fgDebugCheckBBlist".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgDetermineFirstColdBlock()");
                                    result = (ulong)RyuJitFunction.FgDetermineFirstColdBlock | ((ulong)"fgDetermineFirstColdBlock()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'E':
                            Assert.Equal(start, "fgExpandRarelyRunBlocks()");
                            result = (ulong)RyuJitFunction.FgExpandRarelyRunBlocks | ((ulong)"fgExpandRarelyRunBlocks()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'F':
                            switch (start[6])
                            {
                                case 'B':
                                    Assert.Equal(start, "fgFindBasicBlocks()");
                                    result = (ulong)RyuJitFunction.FgFindBasicBlocks | ((ulong)"fgFindBasicBlocks()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgFindOperOrder()");
                                    result = (ulong)RyuJitFunction.FgFindOperOrder | ((ulong)"fgFindOperOrder()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'I':
                            Assert.Equal(start, "fgInterBlockLocalVarLiveness()");
                            result = (ulong)RyuJitFunction.FgInterBlockLocalVarLiveness | ((ulong)"fgInterBlockLocalVarLiveness()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'L':
                            Assert.Equal(start, "fgLocalVarLiveness()");
                            result = (ulong)RyuJitFunction.FgLocalVarLiveness | ((ulong)"fgLocalVarLiveness()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'M':
                            switch (start[3])
                            {
                                case 'a':
                                    Assert.Equal(start, "fgMarkAddressExposedLocals()");
                                    result = (ulong)RyuJitFunction.FgMarkAddressExposedLocals | ((ulong)"fgMarkAddressExposedLocals()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgMorphBlocks()");
                                    result = (ulong)RyuJitFunction.FgMorphBlocks | ((ulong)"fgMorphBlocks()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'P':
                            switch (start[3])
                            {
                                case 'e':
                                    Assert.Equal(start, "fgPerBlockLocalVarLiveness()");
                                    result = (ulong)RyuJitFunction.FgPerBlockLocalVarLiveness | ((ulong)"fgPerBlockLocalVarLiveness()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgPromoteStructs()");
                                    result = (ulong)RyuJitFunction.FgPromoteStructs | ((ulong)"fgPromoteStructs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'R':
                            switch (start[4])
                            {
                                case 'm':
                                    switch (start[13])
                                    {
                                        case 'B':
                                            Assert.Equal(start, "fgRemoveEmptyBlocks");
                                            result = (ulong)RyuJitFunction.FgRemoveEmptyBlocks | ((ulong)"fgRemoveEmptyBlocks".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "fgRemoveEmptyTry()");
                                            result = (ulong)RyuJitFunction.FgRemoveEmptyTry | ((ulong)"fgRemoveEmptyTry()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                    }
                                    break;
                                case 'o':
                                    Assert.Equal(start, "fgReorderBlocks()");
                                    result = (ulong)RyuJitFunction.FgReorderBlocks | ((ulong)"fgReorderBlocks()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 's':
                                    Assert.Equal(start, "fgResetImplicitByRefRefCount()");
                                    result = (ulong)RyuJitFunction.FgResetImplicitByRefRefCount | ((ulong)"fgResetImplicitByRefRefCount()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "fgRetypeImplicitByRefArgs()");
                                    result = (ulong)RyuJitFunction.FgRetypeImplicitByRefArgs | ((ulong)"fgRetypeImplicitByRefArgs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'S':
                            Assert.Equal(start, "fgSetBlockOrder()");
                            result = (ulong)RyuJitFunction.FgSetBlockOrder | ((ulong)"fgSetBlockOrder()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'T':
                            Assert.Equal(start, "fgTailMergeThrows");
                            result = (ulong)RyuJitFunction.FgTailMergeThrows | ((ulong)"fgTailMergeThrows".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'U':
                            Assert.Equal(start, "fgUpdateFlowGraph()");
                            result = (ulong)RyuJitFunction.FgUpdateFlowGraph | ((ulong)"fgUpdateFlowGraph()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "fgValueNumber()");
                            result = (ulong)RyuJitFunction.FgValueNumber | ((ulong)"fgValueNumber()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                    }
                    break;
                case 'g':
                    switch (start[1])
                    {
                        case 'c':
                            Assert.Equal(start, "gcInfoBlockHdrSave()");
                            result = (ulong)RyuJitFunction.GcInfoBlockHdrSave | ((ulong)"gcInfoBlockHdrSave()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            switch (start[3])
                            {
                                case 'E':
                                    Assert.Equal(start, "genEnregisterIncomingStackArgs()");
                                    result = (ulong)RyuJitFunction.GenEnregisterIncomingStackArgs | ((ulong)"genEnregisterIncomingStackArgs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 'F':
                                    switch (start[5])
                                    {
                                        case 'E':
                                            Assert.Equal(start, "genFnEpilog()");
                                            result = (ulong)RyuJitFunction.GenFnEpilog | ((ulong)"genFnEpilog()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        default:
                                            switch (start[11])
                                            {
                                                case '(':
                                                    Assert.Equal(start, "genFnProlog()");
                                                    result = (ulong)RyuJitFunction.GenFnProlog | ((ulong)"genFnProlog()".Length << (sizeof(RyuJitFunction) * 8));
                                                    break;
                                                default:
                                                    Assert.Equal(start, "genFnPrologCalleeRegArgs()");
                                                    result = (ulong)RyuJitFunction.GenFnPrologCalleeRegArgs | ((ulong)"genFnPrologCalleeRegArgs()".Length << (sizeof(RyuJitFunction) * 8));
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case 'G':
                                    Assert.Equal(start, "genGenerateCode()");
                                    result = (ulong)RyuJitFunction.GenGenerateCode | ((ulong)"genGenerateCode()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 'I':
                                    Assert.Equal(start, "genIPmappingGen()");
                                    result = (ulong)RyuJitFunction.GenIPmappingGen | ((ulong)"genIPmappingGen()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "genSetScopeInfo()");
                                    result = (ulong)RyuJitFunction.GenSetScopeInfo | ((ulong)"genSetScopeInfo()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'i':
                    Assert.Equal(start, "impImport()");
                    result = (ulong)RyuJitFunction.ImpImport | ((ulong)"impImport()".Length << (sizeof(RyuJitFunction) * 8));
                    break;
                case 'L':
                    Assert.Equal(start, "LinearScan::allocateRegisters()");
                    result = (ulong)RyuJitFunction.LinearScan_allocateRegisters | ((ulong)"LinearScan::allocateRegisters()".Length << (sizeof(RyuJitFunction) * 8));
                    break;
                case 'l':
                    switch (start[3])
                    {
                        case 'A':
                            Assert.Equal(start, "lvaAssignFrameOffsets(FINAL_FRAME_LAYOUT)");
                            result = (ulong)RyuJitFunction.LvaAssignFrameOffsets | ((ulong)"lvaAssignFrameOffsets(FINAL_FRAME_LAYOUT)".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "lvaMarkLocalVars()");
                            result = (ulong)RyuJitFunction.LvaMarkLocalVars | ((ulong)"lvaMarkLocalVars()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                    }
                    break;
                case 'o':
                    switch (start[3])
                    {
                        case 'A':
                            switch (start[4])
                            {
                                case 'd':
                                    Assert.Equal(start, "optAddCopies()");
                                    result = (ulong)RyuJitFunction.OptAddCopies | ((ulong)"optAddCopies()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "optAssertionPropMain()");
                                    result = (ulong)RyuJitFunction.OptAssertionPropMain | ((ulong)"optAssertionPropMain()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'C':
                            Assert.Equal(start, "optCloneLoops()");
                            result = (ulong)RyuJitFunction.OptCloneLoops | ((ulong)"optCloneLoops()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'E':
                            Assert.Equal(start, "optEarlyProp()");
                            result = (ulong)RyuJitFunction.OptEarlyProp | ((ulong)"optEarlyProp()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'O':
                            switch (start[11])
                            {
                                case 'B':
                                    Assert.Equal(start, "optOptimizeBools()");
                                    result = (ulong)RyuJitFunction.OptOptimizeBools | ((ulong)"optOptimizeBools()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 'C':
                                    Assert.Equal(start, "optOptimizeCSEs()");
                                    result = (ulong)RyuJitFunction.OptOptimizeCSEs | ((ulong)"optOptimizeCSEs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                                case 'L':
                                    switch (start[12])
                                    {
                                        case 'a':
                                            Assert.Equal(start, "optOptimizeLayout()");
                                            result = (ulong)RyuJitFunction.OptOptimizeLayout | ((ulong)"optOptimizeLayout()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "optOptimizeLoops()");
                                            result = (ulong)RyuJitFunction.OptOptimizeLoops | ((ulong)"optOptimizeLoops()".Length << (sizeof(RyuJitFunction) * 8));
                                            break;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "optOptimizeValnumCSEs()");
                                    result = (ulong)RyuJitFunction.OptOptimizeValnumCSEs | ((ulong)"optOptimizeValnumCSEs()".Length << (sizeof(RyuJitFunction) * 8));
                                    break;
                            }
                            break;
                        case 'R':
                            Assert.Equal(start, "optRemoveRedundantZeroInits()");
                            result = (ulong)RyuJitFunction.OptRemoveRedundantZeroInits | ((ulong)"optRemoveRedundantZeroInits()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "optVnCopyProp()");
                            result = (ulong)RyuJitFunction.OptVnCopyProp | ((ulong)"optVnCopyProp()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                    }
                    break;
                case 'O':
                    Assert.Equal(start, "OptimizeRangeChecks()");
                    result = (ulong)RyuJitFunction.OptimizeRangeChecks | ((ulong)"OptimizeRangeChecks()".Length << (sizeof(RyuJitFunction) * 8));
                    break;
                default:
                    switch (start[12])
                    {
                        case 'B':
                            Assert.Equal(start, "SsaBuilder::Build()");
                            result = (ulong)RyuJitFunction.SsaBuilder_Build | ((ulong)"SsaBuilder::Build()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        case 'I':
                            Assert.Equal(start, "SsaBuilder::InsertPhiFunctions()");
                            result = (ulong)RyuJitFunction.SsaBuilder_InsertPhiFunctions | ((ulong)"SsaBuilder::InsertPhiFunctions()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                        default:
                            Assert.Equal(start, "SsaBuilder::RenameVariables()");
                            result = (ulong)RyuJitFunction.SsaBuilder_RenameVariables | ((ulong)"SsaBuilder::RenameVariables()".Length << (sizeof(RyuJitFunction) * 8));
                            break;
                    }
                    break;
            }

            width = (int)(result >> (sizeof(RyuJitFunction) * 8));
            return (RyuJitFunction)result;
        }
    }
}
