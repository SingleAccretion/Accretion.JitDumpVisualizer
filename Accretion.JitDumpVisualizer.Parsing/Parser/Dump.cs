using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using Accretion.JitDumpVisualizer.Parsing.Parser.Phases;
using Accretion.JitDumpVisualizer.Parsing.Tokens;

namespace Accretion.JitDumpVisualizer.Parsing.Parser
{
    public sealed class Dump : DumpSection
    {
        private static readonly Token[] _phaseStart = new Token[] { new(TokenKind.StartingPhase) };
        private static readonly Token[] _phaseStartEnd = new Token[] { new(TokenKind.EndOfLine), new(TokenKind.OpenParen) };

        private static readonly Token[] _phaseEnd = new Token[] { new(TokenKind.FifteenStars), new(TokenKind.FinishingPhase) };
        private static readonly Token[] _noChanges = new Token[] { new(TokenKind.OpenBracket) };

        private ReadOnlyArray<CompilationPhase>? _phases;

        public Dump(string dump) : this(new TokenSource(dump.Replace("\r\n", "\n"))) { }

        private Dump(TokenSource tokens) : base(tokens)
        {
            CompilationInfo = null;
        }

        public ReadOnlyArray<CompilationPhase> Phases => _phases ??= ParsePhases(Tokens);
        public CompilationInfo CompilationInfo { get; }

        private static ReadOnlyArray<CompilationPhase> ParsePhases(TokenSource tokens)
        {
            var phases = new ArrayBuilder<CompilationPhase>();

            // var phaseTokensSources = tokens.Dice(_phaseStartTokens, _phaseEndTokens);
            var phaseSections = tokens.SplitIntoSections(_phaseStart, _phaseEnd);

            foreach (var (header, body, footer) in phaseSections)
            {
                var headerStream = header.GetTokenStream();
                headerStream.Skip(_phaseStart.Length);
                var name = headerStream.NextTokensBeforeAny(_phaseStartEnd).ToString();
                var noChanges = footer.Contains(_noChanges);

                phases.Add(name switch
                {
                    "Pre-import" => new PreImport(body, noChanges),
                    "Importation" => new Importation(body, noChanges),
                    "Indirect call transform" => new IndirectCallTransform(body, noChanges),
                    "Expand patchpoints" => new ExpandPatchpoints(body, noChanges),
                    "Post-import" => new PostImport(body, noChanges),
                    "Morph - Init" => new MorphInit(body, noChanges),
                    "Morph - Inlining" => new MorphInlining(body, noChanges),
                    "Allocate Objects" => new AllocateObjects(body, noChanges),
                    "Morph - Add internal blocks" => new MorphAddInternalBlocks(body, noChanges),
                    "Remove empty try" => new RemoveEmptyTry(body, noChanges),
                    "Remove empty finally" => new RemoveEmptyFinally(body, noChanges),
                    "Merge callfinally chains" => new MergeCallfinallyChains(body, noChanges),
                    "Clone finally" => new CloneFinally(body, noChanges),
                    "Compute preds" => new ComputePreds(body, noChanges),
                    "Merge throw blocks" => new MergeThrowBlocks(body, noChanges),
                    "Update flow graph early pass" => new UpdateFlowGraphEarlyPass(body, noChanges),
                    "Morph - Promote Structs" => new MorphPromoteStructs(body, noChanges),
                    "Morph - Structs/AddrExp" => new MorphStructsAddressExposed(body, noChanges),
                    "Morph - ByRefs" => new MorphByRefs(body, noChanges),
                    "Morph - Global" => new MorphGlobal(body, noChanges),
                    "GS Cookie" => new GSCookie(body, noChanges),
                    "Compute edge weights" => new ComputeEdgeWeights(body, noChanges),
                    "Create EH funclets" => new CreateEHFunclets(body, noChanges),
                    "Optimize layout" => new OptimizeLayout(body, noChanges),
                    "Compute blocks reachability" => new ComputeBlocksReachability(body, noChanges),
                    "Optimize loops" => new OptimizeLoops(body, noChanges),
                    "Clone loops" => new CloneLoops(body, noChanges),
                    "Unroll loops" => new UnrollLoops(body, noChanges),
                    "Mark local vars" => new MarkLocalVars(body, noChanges),
                    "Optimize bools" => new OptimizeBools(body, noChanges),
                    "Find oper order" => new FindOperOrder(body, noChanges),
                    "Set block order" => new SetBlockOrder(body, noChanges),
                    "Build SSA representation" => new BuildSSARepresentation(body, noChanges),
                    "Early Value Propagation" => new EarlyValuePropagation(body, noChanges),
                    "Do value numbering" => new DoValueNumbering(body, noChanges),
                    "Hoist loop code" => new HoistLoopCode(body, noChanges),
                    "VN based copy prop" => new VNBasedCopyProp(body, noChanges),
                    "Optimize Valnum CSEs" => new OptimizeValnumCSEs(body, noChanges),
                    "Assertion prop" => new AssertionProp(body, noChanges),
                    "Optimize index checks" => new OptimizeIndexChecks(body, noChanges),
                    "Insert GC Polls" => new InsertGCPolls(body, noChanges),
                    "Determine first cold block" => new DetermineFirstColdBlock(body, noChanges),
                    "Rationalize IR" => new RationalizeIR(body, noChanges),
                    "Do 'simple' lowering" => new DoSimpleLowering(body, noChanges),
                    "Lowering nodeinfo" => new LoweringNodeInfo(body, noChanges),
                    "Calculate stack level slots" => new CalculateStackLevelSlots(body, noChanges),
                    "Linear scan register alloc" => new LinearScanRegisterAlloc(body, noChanges),
                    "Generate code" => new GenerateCode(body, noChanges),
                    "Emit code" => new EmitCode(body, noChanges),
                    "Emit GC+EH tables" => new EmitGCPlusEHTables(body, noChanges),
                    _ => new Unknown(body, noChanges)
                });
            }

            return phases.AsReadOnlyArray();
        }
    }
}
