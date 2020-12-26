using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static RyuJitPhase ParseRyuJitPhase(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<RyuJitPhase>() is 1);

            ulong result;
            switch (start[0])
            {
                case 'A':
                    switch (start[1])
                    {
                        case 'l':
                            Assert.Equal(start, "Allocate Objects");
                            result = (ulong)RyuJitPhase.AllocateObjects | ((ulong)"Allocate Objects".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            Assert.Equal(start, "Assertion prop");
                            result = (ulong)RyuJitPhase.AssertionProp | ((ulong)"Assertion prop".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'B':
                    Assert.Equal(start, "Build SSA representation");
                    result = (ulong)RyuJitPhase.BuildSSARepresentation | ((ulong)"Build SSA representation".Length << (sizeof(RyuJitPhase) * 8));
                    break;
                case 'C':
                    switch (start[1])
                    {
                        case 'a':
                            Assert.Equal(start, "Calculate stack level slots");
                            result = (ulong)RyuJitPhase.CalculateStackLevelSlots | ((ulong)"Calculate stack level slots".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'l':
                            switch (start[6])
                            {
                                case 'f':
                                    Assert.Equal(start, "Clone finally");
                                    result = (ulong)RyuJitPhase.CloneFinally | ((ulong)"Clone finally".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Clone loops");
                                    result = (ulong)RyuJitPhase.CloneLoops | ((ulong)"Clone loops".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                        case 'o':
                            switch (start[8])
                            {
                                case 'b':
                                    Assert.Equal(start, "Compute blocks reachability");
                                    result = (ulong)RyuJitPhase.ComputeBlocksReachability | ((ulong)"Compute blocks reachability".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                case 'e':
                                    Assert.Equal(start, "Compute edge weights");
                                    result = (ulong)RyuJitPhase.ComputeEdgeWeights | ((ulong)"Compute edge weights".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Compute preds");
                                    result = (ulong)RyuJitPhase.ComputePreds | ((ulong)"Compute preds".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                        default:
                            Assert.Equal(start, "Create EH funclets");
                            result = (ulong)RyuJitPhase.CreateEHFunclets | ((ulong)"Create EH funclets".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'D':
                    switch (start[1])
                    {
                        case 'e':
                            Assert.Equal(start, "Determine first cold block");
                            result = (ulong)RyuJitPhase.DetermineFirstColdBlock | ((ulong)"Determine first cold block".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            switch (start[3])
                            {
                                case '\'':
                                    Assert.Equal(start, "Do 'simple' lowering");
                                    result = (ulong)RyuJitPhase.DoSimpleLowering | ((ulong)"Do 'simple' lowering".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Do value numbering");
                                    result = (ulong)RyuJitPhase.DoValueNumbering | ((ulong)"Do value numbering".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'E':
                    switch (start[1])
                    {
                        case 'a':
                            Assert.Equal(start, "Early Value Propagation");
                            result = (ulong)RyuJitPhase.EarlyValuePropagation | ((ulong)"Early Value Propagation".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'x':
                            Assert.Equal(start, "Expand patchpoints");
                            result = (ulong)RyuJitPhase.ExpandPatchpoints | ((ulong)"Expand patchpoints".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            switch (start[5])
                            {
                                case 'c':
                                    Assert.Equal(start, "Emit code");
                                    result = (ulong)RyuJitPhase.EmitCode | ((ulong)"Emit code".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Emit GC+EH tables");
                                    result = (ulong)RyuJitPhase.EmitGCPlusEHTables | ((ulong)"Emit GC+EH tables".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'F':
                    Assert.Equal(start, "Find oper order");
                    result = (ulong)RyuJitPhase.FindOperOrder | ((ulong)"Find oper order".Length << (sizeof(RyuJitPhase) * 8));
                    break;
                case 'G':
                    switch (start[1])
                    {
                        case 'e':
                            Assert.Equal(start, "Generate code");
                            result = (ulong)RyuJitPhase.GenerateCode | ((ulong)"Generate code".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            Assert.Equal(start, "GS Cookie");
                            result = (ulong)RyuJitPhase.GSCookie | ((ulong)"GS Cookie".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'H':
                    Assert.Equal(start, "Hoist loop code");
                    result = (ulong)RyuJitPhase.HoistLoopCode | ((ulong)"Hoist loop code".Length << (sizeof(RyuJitPhase) * 8));
                    break;
                case 'I':
                    switch (start[1])
                    {
                        case 'm':
                            Assert.Equal(start, "Importation");
                            result = (ulong)RyuJitPhase.Importation | ((ulong)"Importation".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            switch (start[2])
                            {
                                case 'd':
                                    Assert.Equal(start, "Indirect call transform");
                                    result = (ulong)RyuJitPhase.IndirectCallTransform | ((ulong)"Indirect call transform".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Insert GC Polls");
                                    result = (ulong)RyuJitPhase.InsertGCPolls | ((ulong)"Insert GC Polls".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'L':
                    switch (start[1])
                    {
                        case 'i':
                            Assert.Equal(start, "Linear scan register alloc");
                            result = (ulong)RyuJitPhase.LinearScanRegisterAlloc | ((ulong)"Linear scan register alloc".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default: result = (ulong)RyuJitPhase.LoweringNodeInfo | ((ulong)"Lowering nodeinfo".Length << (sizeof(RyuJitPhase) * 8)); break;
                    }
                    break;
                case 'M':
                    switch (start[1])
                    {
                        case 'a':
                            Assert.Equal(start, "Mark local vars");
                            result = (ulong)RyuJitPhase.MarkLocalVars | ((ulong)"Mark local vars".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'e':
                            switch (start[6])
                            {
                                case 'c':
                                    Assert.Equal(start, "Merge callfinally chains");
                                    result = (ulong)RyuJitPhase.MergeCallfinallyChains | ((ulong)"Merge callfinally chains".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Merge throw blocks");
                                    result = (ulong)RyuJitPhase.MergeThrowBlocks | ((ulong)"Merge throw blocks".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                        default:
                            switch (start[8])
                            {
                                case 'A':
                                    Assert.Equal(start, "Morph - Add internal blocks");
                                    result = (ulong)RyuJitPhase.MorphAddInternalBlocks | ((ulong)"Morph - Add internal blocks".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                case 'B':
                                    Assert.Equal(start, "Morph - ByRefs");
                                    result = (ulong)RyuJitPhase.MorphByRefs | ((ulong)"Morph - ByRefs".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                case 'G':
                                    Assert.Equal(start, "Morph - Global");
                                    result = (ulong)RyuJitPhase.MorphGlobal | ((ulong)"Morph - Global".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                case 'I':
                                    switch (start[10])
                                    {
                                        case 'i':
                                            Assert.Equal(start, "Morph - Init");
                                            result = (ulong)RyuJitPhase.MorphInit | ((ulong)"Morph - Init".Length << (sizeof(RyuJitPhase) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "Morph - Inlining");
                                            result = (ulong)RyuJitPhase.MorphInlining | ((ulong)"Morph - Inlining".Length << (sizeof(RyuJitPhase) * 8));
                                            break;
                                    }
                                    break;
                                case 'P':
                                    Assert.Equal(start, "Morph - Promote Structs");
                                    result = (ulong)RyuJitPhase.MorphPromoteStructs | ((ulong)"Morph - Promote Structs".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Morph - Structs/AddrExp");
                                    result = (ulong)RyuJitPhase.MorphStructsAddressExposed | ((ulong)"Morph - Structs/AddrExp".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'O':
                    switch (start[9])
                    {
                        case 'b':
                            Assert.Equal(start, "Optimize bools");
                            result = (ulong)RyuJitPhase.OptimizeBools | ((ulong)"Optimize bools".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'i':
                            Assert.Equal(start, "Optimize index checks");
                            result = (ulong)RyuJitPhase.OptimizeIndexChecks | ((ulong)"Optimize index checks".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        case 'l':
                            switch (start[10])
                            {
                                case 'a':
                                    Assert.Equal(start, "Optimize layout");
                                    result = (ulong)RyuJitPhase.OptimizeLayout | ((ulong)"Optimize layout".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Optimize loops");
                                    result = (ulong)RyuJitPhase.OptimizeLoops | ((ulong)"Optimize loops".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                        default:
                            Assert.Equal(start, "Optimize Valnum CSEs");
                            result = (ulong)RyuJitPhase.OptimizeValnumCSEs | ((ulong)"Optimize Valnum CSEs".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'P':
                    switch (start[1])
                    {
                        case 'o':
                            Assert.Equal(start, "Post-import");
                            result = (ulong)RyuJitPhase.PostImport | ((ulong)"Post-import".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            Assert.Equal(start, "Pre-import");
                            result = (ulong)RyuJitPhase.PreImport | ((ulong)"Pre-import".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                case 'R':
                    switch (start[1])
                    {
                        case 'a':
                            Assert.Equal(start, "Rationalize IR");
                            result = (ulong)RyuJitPhase.RationalizeIR | ((ulong)"Rationalize IR".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            switch (start[13])
                            {
                                case 'f':
                                    Assert.Equal(start, "Remove empty finally");
                                    result = (ulong)RyuJitPhase.RemoveEmptyFinally | ((ulong)"Remove empty finally".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "Remove empty try");
                                    result = (ulong)RyuJitPhase.RemoveEmptyTry | ((ulong)"Remove empty try".Length << (sizeof(RyuJitPhase) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'S':
                    Assert.Equal(start, "Set block order");
                    result = (ulong)RyuJitPhase.SetBlockOrder | ((ulong)"Set block order".Length << (sizeof(RyuJitPhase) * 8));
                    break;
                case 'U':
                    switch (start[1])
                    {
                        case 'n':
                            Assert.Equal(start, "Unroll loops");
                            result = (ulong)RyuJitPhase.UnrollLoops | ((ulong)"Unroll loops".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                        default:
                            Assert.Equal(start, "Update flow graph early pass");
                            result = (ulong)RyuJitPhase.UpdateFlowGraphEarlyPass | ((ulong)"Update flow graph early pass".Length << (sizeof(RyuJitPhase) * 8));
                            break;
                    }
                    break;
                default:
                    Assert.Equal(start, "VN based copy prop");
                    result = (ulong)RyuJitPhase.VNBasedCopyPropagation | ((ulong)"VN based copy prop".Length << (sizeof(RyuJitPhase) * 8));
                    break;
            }

            width = (int)(result >> (sizeof(RyuJitPhase) * 8));
            return (RyuJitPhase)result;
        }
    }
}
