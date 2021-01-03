using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static RyuJitPhase ParseRyuJitPhase(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<RyuJitPhase>() is 1);

            var result = (start[0]) switch
            {
                'A' => (start[1]) switch
                {
                    'l' => Result(RyuJitPhase.AllocateObjects, "Allocate Objects", start),
                    _ => Result(RyuJitPhase.AssertionProp, "Assertion prop", start)
                },
                'B' => Result(RyuJitPhase.BuildSSARepresentation, "Build SSA representation", start),
                'C' => (start[1]) switch
                {
                    'a' => Result(RyuJitPhase.CalculateStackLevelSlots, "Calculate stack level slots", start),
                    'l' => (start[6]) switch
                    {
                        'f' => Result(RyuJitPhase.CloneFinally, "Clone finally", start),
                        _ => Result(RyuJitPhase.CloneLoops, "Clone loops", start)
                    },
                    'o' => (start[8]) switch
                    {
                        'b' => Result(RyuJitPhase.ComputeBlocksReachability, "Compute blocks reachability", start),
                        'e' => Result(RyuJitPhase.ComputeEdgeWeights, "Compute edge weights", start),
                        _ => Result(RyuJitPhase.ComputePreds, "Compute preds", start)
                    },
                    _ => Result(RyuJitPhase.CreateEHFunclets, "Create EH funclets", start)
                },
                'D' => (start[1]) switch
                {
                    'e' => Result(RyuJitPhase.DetermineFirstColdBlock, "Determine first cold block", start),
                    _ => (start[3]) switch
                    {
                        '\'' => Result(RyuJitPhase.DoSimpleLowering, "Do 'simple' lowering", start),
                        _ => Result(RyuJitPhase.DoValueNumbering, "Do value numbering", start)
                    }
                },
                'E' => (start[1]) switch
                {
                    'a' => Result(RyuJitPhase.EarlyValuePropagation, "Early Value Propagation", start),
                    'x' => Result(RyuJitPhase.ExpandPatchpoints, "Expand patchpoints", start),
                    _ => (start[5]) switch
                    {
                        'c' => Result(RyuJitPhase.EmitCode, "Emit code", start),
                        _ => Result(RyuJitPhase.EmitGCPlusEHTables, "Emit GC+EH tables", start)
                    }
                },
                'F' => Result(RyuJitPhase.FindOperOrder, "Find oper order", start),
                'G' => (start[1]) switch
                {
                    'e' => Result(RyuJitPhase.GenerateCode, "Generate code", start),
                    _ => Result(RyuJitPhase.GSCookie, "GS Cookie", start)
                },
                'H' => Result(RyuJitPhase.HoistLoopCode, "Hoist loop code", start),
                'I' => (start[1]) switch
                {
                    'm' => Result(RyuJitPhase.Importation, "Importation", start),
                    _ => (start[2]) switch
                    {
                        'd' => Result(RyuJitPhase.IndirectCallTransform, "Indirect call transform", start),
                        _ => Result(RyuJitPhase.InsertGCPolls, "Insert GC Polls", start)
                    }
                },
                'L' => (start[1]) switch
                {
                    'i' => Result(RyuJitPhase.LinearScanRegisterAlloc, "Linear scan register alloc", start),
                    _ => Result(RyuJitPhase.LoweringNodeInfo, "Lowering nodeinfo", start)
                },
                'M' => (start[1]) switch
                {
                    'a' => Result(RyuJitPhase.MarkLocalVars, "Mark local vars", start),
                    'e' => (start[6]) switch
                    {
                        'c' => Result(RyuJitPhase.MergeCallfinallyChains, "Merge callfinally chains", start),
                        _ => Result(RyuJitPhase.MergeThrowBlocks, "Merge throw blocks", start)
                    },
                    _ => (start[8]) switch
                    {
                        'A' => Result(RyuJitPhase.MorphAddInternalBlocks, "Morph - Add internal blocks", start),
                        'B' => Result(RyuJitPhase.MorphByRefs, "Morph - ByRefs", start),
                        'G' => Result(RyuJitPhase.MorphGlobal, "Morph - Global", start),
                        'I' => (start[10]) switch
                        {
                            'i' => Result(RyuJitPhase.MorphInit, "Morph - Init", start),
                            _ => Result(RyuJitPhase.MorphInlining, "Morph - Inlining", start)
                        },
                        'P' => Result(RyuJitPhase.MorphPromoteStructs, "Morph - Promote Structs", start),
                        _ => Result(RyuJitPhase.MorphStructsAddressExposed, "Morph - Structs/AddrExp", start)
                    }
                },
                'O' => (start[9]) switch
                {
                    'b' => Result(RyuJitPhase.OptimizeBools, "Optimize bools", start),
                    'i' => Result(RyuJitPhase.OptimizeIndexChecks, "Optimize index checks", start),
                    'l' => (start[10]) switch
                    {
                        'a' => Result(RyuJitPhase.OptimizeLayout, "Optimize layout", start),
                        _ => Result(RyuJitPhase.OptimizeLoops, "Optimize loops", start)
                    },
                    _ => Result(RyuJitPhase.OptimizeValnumCSEs, "Optimize Valnum CSEs", start)
                },
                'P' => (start[1]) switch
                {
                    'o' => Result(RyuJitPhase.PostImport, "Post-import", start),
                    _ => Result(RyuJitPhase.PreImport, "Pre-import", start)
                },
                'R' => (start[1]) switch
                {
                    'a' => Result(RyuJitPhase.RationalizeIR, "Rationalize IR", start),
                    _ => (start[13]) switch
                    {
                        'f' => Result(RyuJitPhase.RemoveEmptyFinally, "Remove empty finally", start),
                        _ => Result(RyuJitPhase.RemoveEmptyTry, "Remove empty try", start)
                    }
                },
                'S' => Result(RyuJitPhase.SetBlockOrder, "Set block order", start),
                'U' => (start[1]) switch
                {
                    'n' => Result(RyuJitPhase.UnrollLoops, "Unroll loops", start),
                    _ => Result(RyuJitPhase.UpdateFlowGraphEarlyPass, "Update flow graph early pass", start)
                },
                _ => Result(RyuJitPhase.VNBasedCopyPropagation, "VN based copy prop", start)
            };

            return Result<RyuJitPhase>(result, out width);
        }
    }
}
