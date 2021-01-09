using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System;
using System.Runtime.InteropServices;

namespace Accretion.JitDumpVisualizer.CLI
{
    // [MemoryDiagnoser]
    [DisassemblyDiagnoser(maxDepth: 3)]
    [HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.InstructionRetired, HardwareCounter.BranchMispredictions)]
    public unsafe class TokenizerBenchmarks
    {
        private const int IterationCount = 100;
        private static readonly char* _genTreeStart;
        private static readonly char* _tableRowStart;
        private static readonly Token* _tokens;

        static TokenizerBenchmarks()
        {
            _genTreeStart = (char*)Marshal.AllocHGlobal(GenTreeDumpSection.Length * 2 * sizeof(char));
            GenTreeDumpSection.AsSpan().CopyTo(new Span<char>(_genTreeStart, GenTreeDumpSection.Length));

            _tableRowStart = (char*)Marshal.AllocHGlobal(TableRowDumpSection.Length * 2 * sizeof(char));
            TableRowDumpSection.AsSpan().CopyTo(new Span<char>(_tableRowStart, TableRowDumpSection.Length));

            const int TokenCount = 8000;
            _tokens = (Token*)Marshal.AllocHGlobal(TokenCount * sizeof(Token));
            new Span<nint>(_tokens, TokenCount).Clear();
        }

        public static void DumpTokens()
        {
            for (int i = 0; _tokens[i] != default; i++)
            {
                Console.WriteLine(_tokens[i]);
            }
        }

        // [Benchmark(OperationsPerInvoke = IterationCount)]
        public void TokenizeBlock()
        {
            for (int i = 0; i < IterationCount; i++)
            {
                var start = _genTreeStart;
                var tokens = _tokens;
                Tokenizer.NextBlock(ref start, ref tokens);
            }
        }

        // [Benchmark(Baseline = true)]
        public long Baseline()
        {
            long a1 = 0;
            long a2 = 0;
            long a3 = 0;
            long counter = 2_000;
            while (counter >= 0)
            {
                a1++;
                a2++;
                a3++;
                a1 *= a2;
                a2 *= a3;
                a3 *= a1;
                counter--;
            }

            return a1 + a2 + a3 + counter;
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = IterationCount)]
        public void TokenizeTableRow()
        {
            var row = _tableRowStart;
            var tokens = _tokens;
            for (int i = 0; i < IterationCount; i++)
            {
                tokens = Tokenizer.ParseBasicBlockTableRow(row, tokens);
                tokens = Tokenizer.ParseBasicBlockTableRow(row + 135, tokens);
                tokens = Tokenizer.ParseBasicBlockTableRow(row + 242, tokens);
                tokens = Tokenizer.ParseBasicBlockTableRow(row + 379, tokens);
                tokens = Tokenizer.ParseBasicBlockTableRow(row + 501, tokens);
                Tokenizer.ParseBasicBlockTableRow(row + 638, tokens);
            }
        }

        [Benchmark(OperationsPerInvoke = IterationCount)]
        public void TokenizeTableRowManual()
        {
            var row = _tableRowStart;
            var tokens = _tokens;
            for (int i = 0; i < IterationCount; i++)
            {
                tokens = Tokenizer.ParseBasicBlockTableRowManualCalls(row, tokens);
                tokens = Tokenizer.ParseBasicBlockTableRowManualCalls(row + 135, tokens);
                tokens = Tokenizer.ParseBasicBlockTableRowManualCalls(row + 242, tokens);
                tokens = Tokenizer.ParseBasicBlockTableRowManualCalls(row + 379, tokens);
                tokens = Tokenizer.ParseBasicBlockTableRowManualCalls(row + 501, tokens);
                Tokenizer.ParseBasicBlockTableRowManualCalls(row + 638, tokens);
            }
        }

        private static string[] TableRowsDumpSections { get; } = new string[]
        {
            @"BB01 [0000]  1                             1        [000..012)-> BB03 ( cond )                     i label target hascall newobj LIR 
BB02 [0007]  1       BB01                  0.50     [006..007)-> BB06 (always)                     i LIR 
BB03 [0008]  1       BB01                  0.50     [006..007)-> BB05 ( cond )                     i label target idxlen nullcheck LIR 
BB04 [0014]  1       BB03                  0.25     [006..007)                                     i hascall gcsafe LIR 
BB05 [0015]  2       BB03,BB04             0.50     [006..007)                                     i label target idxlen nullcheck LIR 
BB06 [0009]  2       BB02,BB05             1        [???..???)        (return)                     internal label target hascall gcsafe newobj LIR 
",
            @"BB01 [0000]  1                             1        [000..012)-> BB03 ( cond )                     i label target hascall newobj LIR 
BB02 [0007]  1       BB01                  0.50     [006..007)-> BB06 (always)                     i LIR 
BB03 [0008]  1       BB01                  0.50     [006..007)-> BB05 ( cond )                     i label target idxlen nullcheck LIR 
BB04 [0014]  1       BB03                  0.25     [006..007)                                     i hascall gcsafe LIR 
BB05 [0015]  2       BB03,BB04             0.50     [006..007)                                     i label target idxlen nullcheck LIR 
BB06 [0009]  2       BB02,BB05             1        [???..???)        (return)                     internal label target hascall gcsafe newobj LIR 
",
            @"BB01 [0000]  1                             1        [000..012)-> BB03 ( cond )                     i label target hascall newobj 
BB02 [0007]  1       BB01                  0.50     [006..007)-> BB06 (always)                     i 
BB03 [0008]  1       BB01                  0.50     [006..007)-> BB05 ( cond )                     i label target idxlen nullcheck 
BB04 [0014]  1       BB03                  0.25     [006..007)                                     i hascall gcsafe 
BB05 [0015]  2       BB03,BB04             0.50     [006..007)                                     i label target idxlen nullcheck 
BB06 [0009]  2       BB02,BB05             1        [???..???)        (return)                     internal label target hascall gcsafe newobj 
",
            @"BB01 [0000]  1                             1        [000..012)-> BB03 ( cond )                     i label target hascall newobj 
BB02 [0007]  1       BB01                  0.50     [006..007)-> BB06 (always)                     i 
BB03 [0008]  1       BB01                  0.50     [006..007)-> BB05 ( cond )                     i label target idxlen nullcheck 
BB04 [0014]  1       BB03                  0.25     [006..007)                                     i hascall gcsafe 
BB05 [0015]  2       BB03,BB04             0.50     [006..007)                                     i label target idxlen nullcheck 
BB06 [0009]  2       BB02,BB05             1        [???..???)        (return)                     internal label target hascall gcsafe newobj 
",
            @"BB01 [0000]  1                             1       [000..02F)-> BB03 ( cond )                     i label target hascall gcsafe LIR 
BB02 [0004]  1       BB01                  0.25    [022..023)                                     i hascall gcsafe LIR 
BB03 [0005]  2       BB01,BB02             1       [022..023)        (return)                     i label target gcsafe LIR 
",
            @"BB01 [0001]  1                             1       [???..???)                                     i internal label target 
BB02 [0000]  1                             1       [000..02F)                                     i 
BB03 [0004]  1                             1       [022..023)-> BB05 ( cond )                     i 
BB04 [0005]  1                             0.50    [022..023)                                     i 
BB05 [0006]  2                             1       [022..023)                                     i 
BB06 [0007]  1                             1       [???..???)        (return)                     internal 
",
            @"BB01 [0001]  1                             1       [???..???)                                     i internal label target 
BB02 [0000]  1                             1       [000..02F)                                     i 
BB03 [0004]  1                             1       [022..023)-> BB05 ( cond )                     i 
BB04 [0005]  1                             0.50    [022..023)                                     i 
BB05 [0006]  2                             1       [022..023)                                     i 
BB06 [0007]  1                             1       [???..???)        (return)                     internal 
",
            @"BB01 [0001]  1                             1       [???..???)                                     i internal label target 
BB02 [0000]  1       BB01                  1       [000..02F)                                     i 
BB03 [0004]  1       BB02                  1       [022..023)-> BB05 ( cond )                     i 
BB04 [0005]  1       BB03                  0.50    [022..023)                                     i 
BB05 [0006]  2       BB03,BB04             1       [022..023)                                     i label target 
BB06 [0007]  1       BB05                  1       [???..???)        (return)                     internal 
",
            @"BB01 [0001]  1                             1       [???..???)                                     i internal label target 
BB02 [0000]  1       BB01                  1       [000..02F)                                     i 
BB03 [0004]  1       BB02                  1       [022..023)-> BB05 ( cond )                     i 
BB04 [0005]  1       BB03                  0.50    [022..023)                                     i 
BB05 [0006]  2       BB03,BB04             1       [022..023)                                     i label target 
BB06 [0007]  1       BB05                  1       [???..???)        (return)                     internal 
",
            @"BB02 [0001]  3       BB03,BB04,BB05        1       [???..???)                                     i label target hascall idxlen new[] newobj 
BB11 [0010]  1       BB02                  1       [???..???)                                     i 
BB10 [0009]  1       BB11                  1       [???..???)-> BB07 ( cond )                     i 
BB09 [0008]  1       BB10                  0.50    [???..???)-> BB07 ( cond )                     i 
BB08 [0007]  1       BB09                  0.25    [???..???)                                     i 
BB07 [0006]  3       BB08,BB09,BB10        1       [???..037)        (return)                     i label target hascall idxlen new[] newobj 
",
            @"BB01 [0000]  1                             1       [000..???)                                     i label target hascall idxlen new[] newobj 
BB06 [0005]  1       BB01                  1       [???..???)                                     i 
BB05 [0004]  1       BB06                  1       [???..???)-> BB02 ( cond )                     i 
BB04 [0003]  1       BB05                  0.50    [???..???)-> BB02 ( cond )                     i 
BB03 [0002]  1       BB04                  0.25    [???..???)                                     i 
BB02 [0001]  3       BB03,BB04,BB05        1       [???..???)                                     i label target hascall idxlen new[] newobj 
BB11 [0010]  1       BB02                  1       [???..???)                                     i 
BB10 [0009]  1       BB11                  1       [???..???)-> BB07 ( cond )                     i 
BB09 [0008]  1       BB10                  0.50    [???..???)-> BB07 ( cond )                     i 
BB08 [0007]  1       BB09                  0.25    [???..???)                                     i 
BB07 [0006]  3       BB08,BB09,BB10        1       [???..037)        (return)                     i label target hascall idxlen new[] newobj 
",
            @"BB01 [0000]  1                             1       [000..???)-> BB06 ( cond )                     i label target hascall idxlen new[] newobj 
BB04 [0003]  1       BB01                  0.50    [???..???)-> BB06 ( cond )                     i 
BB05 [0002]  1       BB04                  0.25    [???..???)                                     i 
BB06 [0001]  3       BB01,BB04,BB05        1       [???..???)-> BB11 ( cond )                     i label target hascall idxlen new[] newobj 
BB09 [0008]  1       BB06                  0.50    [???..???)-> BB11 ( cond )                     i 
BB10 [0007]  1       BB09                  0.25    [???..???)                                     i 
BB11 [0006]  3       BB06,BB09,BB10        1       [???..037)        (return)                     i label target hascall idxlen new[] newobj 
",
            @"BB01 [0000]  1                             1       [000..???)-> BB06 ( cond )                     i label target hascall idxlen new[] newobj 
BB04 [0003]  1       BB01                  0.50    [???..???)-> BB06 ( cond )                     i 
BB05 [0002]  1       BB04                  0.25    [???..???)                                     i 
BB06 [0001]  3       BB01,BB04,BB05        1       [???..???)-> BB11 ( cond )                     i label target hascall idxlen new[] newobj 
BB09 [0008]  1       BB06                  0.50    [???..???)-> BB11 ( cond )                     i 
BB10 [0007]  1       BB09                  0.25    [???..???)                                     i 
BB11 [0006]  3       BB06,BB09,BB10        1       [???..037)        (return)                     i label target hascall idxlen new[] newobj 
",
            @"BB01 [0000]  1                             1       [000..???)-> BB04 ( cond )                     i label target hascall idxlen new[] newobj LIR 
BB02 [0003]  1       BB01                  0.25    [???..???)-> BB04 ( cond )                     i LIR 
BB03 [0002]  1       BB02                  0.12    [???..???)                                     i LIR 
BB04 [0001]  3       BB01,BB02,BB03        1       [???..???)-> BB07 ( cond )                     i label target hascall idxlen new[] newobj LIR 
BB05 [0008]  1       BB04                  0.25    [???..???)-> BB07 ( cond )                     i LIR 
BB06 [0007]  1       BB05                  0.12    [???..???)                                     i LIR 
BB07 [0006]  3       BB04,BB05,BB06        1       [???..037)        (return)                     i label target hascall idxlen new[] newobj LIR 
",
            @"BB01 [0000]  1                             1       [000..011)                                     i 
BB36 [0002]  1                             1       [00B..00C)-> BB37 (always)                     i 
BB37 [0004]  1                             0.50    [00B..00C)                                     i 
BB38 [0005]  1                             0.50    [00B..00C)-> BB39 (always)                     i 
BB39 [0010]  1                             0.50    [00B..00C)-> BB40 (always)                     i 
BB40 [0013]  1                             0.50    [00B..00C)-> BB41 (always)                     i 
BB41 [0015]  2                             0.50    [00B..00C)-> BB42 (always)                     i 
BB42 [0017]  2                             0.50    [00B..00C)-> BB43 (always)                     i 
BB43 [0019]  2                             0.50    [00B..00C)-> BB44 (always)                     i 
BB44 [0021]  2                             0.50    [00B..00C)-> BB45 (always)                     i 
BB45 [0023]  2                             0.50    [00B..00C)-> BB46 (always)                     i 
BB46 [0025]  2                             0.50    [00B..00C)-> BB47 (always)                     i 
BB47 [0027]  2                             0.50    [00B..00C)-> BB48 (always)                     i 
BB48 [0029]  2                             0.50    [00B..00C)-> BB49 (always)                     i 
BB49 [0031]  2                             0.50    [00B..00C)-> BB50 (always)                     i 
BB50 [0034]  4                             1       [00B..00C)                                     i newobj 
BB56 [0037]  1                             1       [00B..00C)-> BB57 (always)                     i 
BB57 [0039]  1                             1       [00B..00C)                                     i 
BB58 [0040]  1                             1       [???..???)                                     internal newobj 
BB51 [0035]  1                             1       [???..???)        (return)                     internal newobj 
",
            @"BB01 [0000]  1                             1       [000..011)                                     i 
BB36 [0002]  1                             1       [00B..00C)-> BB37 (always)                     i 
BB37 [0004]  1                             0.50    [00B..00C)                                     i 
BB38 [0005]  1                             0.50    [00B..00C)-> BB39 (always)                     i 
BB39 [0010]  1                             0.50    [00B..00C)-> BB40 (always)                     i 
BB40 [0013]  1                             0.50    [00B..00C)-> BB41 (always)                     i 
BB41 [0015]  2                             0.50    [00B..00C)-> BB42 (always)                     i 
BB42 [0017]  2                             0.50    [00B..00C)-> BB43 (always)                     i 
BB43 [0019]  2                             0.50    [00B..00C)-> BB44 (always)                     i 
BB44 [0021]  2                             0.50    [00B..00C)-> BB45 (always)                     i 
BB45 [0023]  2                             0.50    [00B..00C)-> BB46 (always)                     i 
BB46 [0025]  2                             0.50    [00B..00C)-> BB47 (always)                     i 
BB47 [0027]  2                             0.50    [00B..00C)-> BB48 (always)                     i 
BB48 [0029]  2                             0.50    [00B..00C)-> BB49 (always)                     i 
BB49 [0031]  2                             0.50    [00B..00C)-> BB50 (always)                     i 
BB50 [0034]  4                             1       [00B..00C)                                     i newobj 
BB56 [0037]  1                             1       [00B..00C)-> BB57 (always)                     i 
BB57 [0039]  1                             1       [00B..00C)                                     i 
BB58 [0040]  1                             1       [???..???)                                     internal newobj 
BB51 [0035]  1                             1       [???..???)        (return)                     internal newobj 
",
            @"BB01 [0000]  1                             1       [000..011)                                     i 
BB02 [0002]  1                             1       [00B..00C)-> BB03 (always)                     i 
BB03 [0004]  1                             0.50    [00B..00C)                                     i 
BB04 [0005]  1                             0.50    [00B..00C)-> BB05 (always)                     i 
BB05 [0010]  1                             0.50    [00B..00C)-> BB06 (always)                     i 
BB06 [0013]  1                             0.50    [00B..00C)-> BB07 (always)                     i 
BB07 [0015]  2                             0.50    [00B..00C)-> BB08 (always)                     i 
BB08 [0017]  2                             0.50    [00B..00C)-> BB09 (always)                     i 
BB09 [0019]  2                             0.50    [00B..00C)-> BB10 (always)                     i 
BB10 [0021]  2                             0.50    [00B..00C)-> BB11 (always)                     i 
BB11 [0023]  2                             0.50    [00B..00C)-> BB12 (always)                     i 
BB12 [0025]  2                             0.50    [00B..00C)-> BB13 (always)                     i 
BB13 [0027]  2                             0.50    [00B..00C)-> BB14 (always)                     i 
BB14 [0029]  2                             0.50    [00B..00C)-> BB15 (always)                     i 
BB15 [0031]  2                             0.50    [00B..00C)-> BB16 (always)                     i 
BB16 [0034]  4                             1       [00B..00C)                                     i newobj 
BB17 [0037]  1                             1       [00B..00C)-> BB18 (always)                     i 
BB18 [0039]  1                             1       [00B..00C)                                     i 
BB19 [0040]  1                             1       [???..???)                                     internal newobj 
BB20 [0035]  1                             1       [???..???)        (return)                     internal newobj 
",
            @"BB01 [0000]  1                             1       [000..037)        (return)                     
",
            @"BB08 [0007]  1                             1       [000..003)-> BB10 ( cond )                     i 
BB09 [0008]  1                             1       [003..00A)                                     i 
BB10 [0009]  2                             1       [00A..00B)        (return)                     i 
",
            @"BB08 [0007]  1                             1       [017..018)-> BB10 ( cond )                     i 
BB09 [0008]  1                             0.50    [017..018)                                     i 
BB10 [0009]  2                             1       [017..018)                                     i 
",
            @"BB19 [0018]  1                             1       [000..000)-> BB21 ( cond )                     i internal 
BB20 [0019]  1                             0.50    [000..000)                                     i internal 
BB21 [0020]  2                             1       [000..000)                                     i internal 
",
            @"BB23 [0022]  1                             0       [000..00B)        (throw )                     rare 
",
            @"BB01 [0000]  1                             1       [000..06D)                                     i newobj nullcheck 
BB02 [0007]  1                             1       [017..018)-> BB04 ( cond )                     i 
BB03 [0008]  1                             0.50    [017..018)                                     i 
BB04 [0009]  2                             1       [017..018)                                     i 
BB05 [0010]  1                             1       [???..???)                                     i internal newobj nullcheck 
BB06 [0018]  1                             1       [000..000)-> BB08 ( cond )                     i internal 
BB07 [0019]  1                             0.50    [000..000)                                     i internal 
BB08 [0020]  2                             1       [000..000)                                     i internal 
BB09 [0021]  1                             1       [???..???)        (return)                     internal newobj nullcheck 
",
            @"BB01 [0000]  1                             1       [000..06D)                                     i label target newobj nullcheck 
BB02 [0007]  1       BB01                  1       [017..018)-> BB04 ( cond )                     i 
BB03 [0008]  1       BB02                  0.50    [017..018)                                     i 
BB04 [0009]  2       BB02,BB03             1       [017..018)                                     i label target 
BB05 [0010]  1       BB04                  1       [???..???)                                     i internal newobj nullcheck 
BB06 [0018]  1       BB05                  1       [000..000)-> BB08 ( cond )                     i internal 
BB07 [0019]  1       BB06                  0.50    [000..000)                                     i internal 
BB08 [0020]  2       BB06,BB07             1       [000..000)                                     i internal label target 
BB09 [0021]  1       BB08                  1       [???..???)        (return)                     internal newobj nullcheck 
",
            @"BB01 [0000]  1                             1       [000..06D)                                     i label target newobj nullcheck 
BB02 [0007]  1       BB01                  1       [017..018)-> BB04 ( cond )                     i 
BB03 [0008]  1       BB02                  0.50    [017..018)                                     i 
BB04 [0009]  2       BB02,BB03             1       [017..018)                                     i label target 
BB05 [0010]  1       BB04                  1       [???..???)                                     i internal newobj nullcheck 
BB06 [0018]  1       BB05                  1       [000..000)-> BB08 ( cond )                     i internal 
BB07 [0019]  1       BB06                  0.50    [000..000)                                     i internal 
BB08 [0020]  2       BB06,BB07             1       [000..000)                                     i internal label target 
BB09 [0021]  1       BB08                  1       [???..???)        (return)                     internal newobj nullcheck 
",
            @"BB01 [0000]  1                             1       [000..06D)-> BB04 ( cond )                     i label target newobj nullcheck 
BB03 [0008]  1       BB01                  0.50    [017..018)                                     i 
BB04 [0009]  2       BB01,BB03             1       [000..018)-> BB08 ( cond )                     i label target newobj nullcheck 
BB07 [0019]  1       BB04                  0.50    [000..000)                                     i internal 
BB08 [0020]  2       BB04,BB07             1       [000..000)        (return)                     i internal label target newobj nullcheck 
",
            @"BB01 [0000]  1                             1       [000..06D)-> BB04 ( cond )                     i label target hascall gcsafe newobj nullcheck 
BB03 [0008]  1       BB01                  0.50    [017..018)                                     i hascall gcsafe 
BB04 [0009]  2       BB01,BB03             1       [000..018)-> BB07 ( cond )                     i label target hascall gcsafe newobj nullcheck 
BB08 [0020]  1       BB04                  1       [000..000)        (return)                     i internal label target hascall gcsafe newobj nullcheck 
BB07 [0019]  1       BB04                  0       [000..000)        (throw )                     i internal rare label target hascall gcsafe 
",
            @"BB01 [0000]  1                             1       [000..06D)-> BB04 ( cond )                     i label target hascall gcsafe newobj nullcheck 
BB03 [0008]  1       BB01                  0.50    [017..018)                                     i hascall gcsafe 
BB04 [0009]  2       BB01,BB03             1       [000..018)-> BB07 ( cond )                     i label target hascall gcsafe newobj nullcheck 
BB08 [0020]  1       BB04                  1       [000..000)        (return)                     i internal label target hascall gcsafe newobj nullcheck 
BB07 [0019]  1       BB04                  0       [000..000)        (throw )                     i internal rare label target hascall gcsafe 
",
            @"BB01 [0000]  1                             1       [000..06D)-> BB03 ( cond )                     i label target hascall gcsafe newobj nullcheck 
BB02 [0008]  1       BB01                  0.25    [017..018)                                     i hascall gcsafe 
BB03 [0009]  2       BB01,BB02             1       [000..018)-> BB05 ( cond )                     i label target hascall gcsafe newobj nullcheck 
BB04 [0020]  1       BB03                  1       [000..000)        (return)                     i internal label target hascall gcsafe newobj nullcheck 
BB05 [0019]  1       BB03                  0       [000..000)        (throw )                     i internal rare label target hascall gcsafe 
",
            @"BB04 [0003]  1                             1       [000..009)-> BB06 ( cond )                     i 
BB05 [0004]  1                             1       [009..010)                                     i 
BB06 [0005]  2                             1       [010..011)        (return)                     i 
",
            @"BB01 [0000]  1                             1       [000..06A)                                     i 
BB04 [0003]  1                             1       [05D..05E)-> BB06 ( cond )                     i 
BB05 [0004]  1                             0.50    [05D..05E)                                     i 
BB06 [0005]  2                             1       [05D..05E)                                     i 
BB07 [0006]  1                             1       [???..???)        (return)                     internal 
",
            @"BB01 [0000]  1                             1       [000..06A)                                     i 
BB02 [0003]  1                             1       [05D..05E)-> BB04 ( cond )                     i 
BB03 [0004]  1                             0.50    [05D..05E)                                     i 
BB04 [0005]  2                             1       [05D..05E)                                     i 
BB05 [0006]  1                             1       [???..???)        (return)                     internal 
"
        };

        private const string TableRowDumpSection = @"BB01 [0000]  1                             1        [000..012)-> BB03 ( cond )                     i label target hascall newobj LIR 
BB02 [0007]  1       BB01                  0.50     [006..007)-> BB06 (always)                     i LIR 
BB03 [0008]  1       BB01                  0.50     [006..007)-> BB05 ( cond )                     i label target idxlen nullcheck LIR 
BB04 [0014]  1       BB03                  0.25     [006..007)                                     i hascall gcsafe LIR 
BB05 [0015]  2       BB03,BB04             0.50     [006..007)                                     i label target idxlen nullcheck LIR 
BB06 [0009]  2       BB02,BB05             1        [???..???)        (return)                     internal label target hascall gcsafe newobj LIR 
";

        private const string GenTreeDumpSection = @"***** BB03
STMT00021 (IL 0x006...  ???)
N002 (  2,  2) [000086] ---X--------              *  NULLCHECK byte  
N001 (  1,  1) [000085] ------------              \--*  LCL_VAR   ref    V00 arg0         u:1

***** BB03
STMT00016 (IL 0x006...  ???)
N003 (  0,  0) [000168] ------------              *  COMMA     void  
N001 (  0,  0) [000164] ------------              +--*  NOP       void  
N002 (  0,  0) [000167] ------------              \--*  NOP       void  

***** BB03
STMT00026 (IL 0x006...  ???)
N005 (  7,  6) [000112] -A--G---R---              *  ASG       byref 
N004 (  3,  2) [000111] D------N----              +--*  LCL_VAR   byref  V09 tmp8         d:1
N003 (  3,  3) [000173] ------------              \--*  ADD       byref 
N001 (  1,  1) [000171] ------------                 +--*  LCL_VAR   ref    V00 arg0         u:1
N002 (  1,  1) [000172] ------------                 \--*  CNS_INT   long   12 field offset Fseq[_firstChar]

***** BB03
STMT00027 (IL 0x006...  ???)
N004 (  3,  3) [000114] -A-X----R---              *  ASG       int   
N003 (  1,  1) [000113] D------N----              +--*  LCL_VAR   int    V08 tmp7         d:1
N002 (  3,  3) [000057] ---X--------              \--*  ARR_LENGTH int   
N001 (  1,  1) [000056] ------------                 \--*  LCL_VAR   ref    V00 arg0         u:1 (last use)

***** BB03
STMT00031 (IL 0x006...  ???)
N004 (  8, 15) [000133] -A--G---R---              *  ASG       ref   
N003 (  3,  2) [000132] D------N----              +--*  LCL_VAR   ref    V13 tmp12        d:1
N002 (  4, 12) [000119] #---G-------              \--*  IND       ref   
N001 (  2, 10) [000118] H-----------                 \--*  CNS_INT(h) long   0xD1FFAB1E [ICON_STR_HDL]

***** BB03
STMT00032 (IL 0x006...  ???)
N004 (  8, 15) [000135] -A--G---R---              *  ASG       ref   
N003 (  3,  2) [000134] D------N----              +--*  LCL_VAR   ref    V14 tmp13        d:1
N002 (  4, 12) [000121] #---G-------              \--*  IND       ref   
N001 (  2, 10) [000120] H-----------                 \--*  CNS_INT(h) long   0xD1FFAB1E [ICON_STR_HDL]

***** BB03
STMT00029 (IL 0x006...  ???)
N004 (  5,  5) [000128] ------------              *  JTRUE     void  
N003 (  3,  3) [000090] J------N----              \--*  GE        int   
N001 (  1,  1) [000088] ------------                 +--*  LCL_VAR   int    V08 tmp7         u:1
N002 (  1,  1) [000089] ------------                 \--*  CNS_INT   int    0

------------ BB04 [006..007), preds={BB03} succs={BB05}

***** BB04
STMT00030 (IL 0x006...  ???)
N005 ( 20, 11) [000131] --CXG-------              *  CALL      void   System.Diagnostics.Debug.Fail
N003 (  3,  2) [000129] ------------ arg0 in rcx  +--*  LCL_VAR   ref    V13 tmp12        u:1 (last use)
N004 (  3,  2) [000130] ------------ arg1 in rdx  \--*  LCL_VAR   ref    V14 tmp13        u:1 (last use)

------------ BB05 [006..007), preds={BB03,BB04} succs={BB06}

***** BB05
STMT00023 (IL 0x006...  ???)
N003 (  7,  5) [000100] -A------R---              *  ASG       byref 
N002 (  3,  2) [000099] D------N----              +--*  LCL_VAR   byref  V23 tmp22        d:1
N001 (  3,  2) [000096] ------------              \--*  LCL_VAR   byref  V09 tmp8         u:1 (last use)

***** BB05
STMT00024 (IL 0x006...  ???)
N003 (  7,  5) [000179] -A------R---              *  ASG       byref 
N002 (  3,  2) [000177] D------N----              +--*  LCL_VAR   byref  V19 tmp18        d:1
N001 (  3,  2) [000178] -------N----              \--*  LCL_VAR   byref  V23 tmp22        u:1 (last use)

***** BB05
STMT00025 (IL 0x006...  ???)
N003 (  5,  4) [000110] -A------R---              *  ASG       int   
N002 (  3,  2) [000109] D------N----              +--*  LCL_VAR   int    V20 tmp19        d:1
N001 (  1,  1) [000108] ------------              \--*  LCL_VAR   int    V08 tmp7         u:1 (last use)

***** BB05
STMT00018 (IL 0x006...  ???)
N007 ( 14, 10) [000186] -A----------              *  COMMA     void  
N003 (  7,  5) [000182] -A------R---              +--*  ASG       byref 
N002 (  3,  2) [000180] D------N----              |  +--*  LCL_VAR   byref  V17 tmp16        d:2
N001 (  3,  2) [000181] ------------              |  \--*  LCL_VAR   byref  V19 tmp18        u:1 (last use)
N006 (  7,  5) [000185] -A------R---              \--*  ASG       int   
N005 (  3,  2) [000183] D------N----                 +--*  LCL_VAR   int    V18 tmp17        d:2
N004 (  3,  2) [000184] ------------                 \--*  LCL_VAR   int    V20 tmp19        u:1 (last use)

------------ BB06 [???..???) (return), preds={BB02,BB05} succs={}

***** BB06
STMT00034 (IL   ???...  ???)
N005 (  0,  0) [000223] -A------R---              *  ASG       int   
N004 (  0,  0) [000221] D------N----              +--*  LCL_VAR   int    V18 tmp17        d:1
N003 (  0,  0) [000222] ------------              \--*  PHI       int   
N001 (  0,  0) [000226] ------------ pred BB02       +--*  PHI_ARG   int    V18 tmp17        u:3
N002 (  0,  0) [000224] ------------ pred BB05       \--*  PHI_ARG   int    V18 tmp17        u:2

***** BB06
STMT00033 (IL   ???...  ???)
N005 (  0,  0) [000220] -A------R---              *  ASG       byref 
N004 (  0,  0) [000218] D------N----              +--*  LCL_VAR   byref  V17 tmp16        d:1
N003 (  0,  0) [000219] ------------              \--*  PHI       byref 
N001 (  0,  0) [000227] ------------ pred BB02       +--*  PHI_ARG   byref  V17 tmp16        u:3
N002 (  0,  0) [000225] ------------ pred BB05       \--*  PHI_ARG   byref  V17 tmp16        u:2

***** BB06
STMT00003 (IL   ???...  ???)
N007 ( 14, 10) [000193] -A----------              *  COMMA     void  
N003 (  7,  5) [000189] -A------R---              +--*  ASG       byref 
N002 (  3,  2) [000187] D------N----              |  +--*  LCL_VAR   byref  V15 tmp14        d:1
N001 (  3,  2) [000188] ------------              |  \--*  LCL_VAR   byref  V17 tmp16        u:1 (last use)
N006 (  7,  5) [000192] -A------R---              \--*  ASG       int   
N005 (  3,  2) [000190] D------N----                 +--*  LCL_VAR   int    V16 tmp15        d:1
N004 (  3,  2) [000191] ------------                 \--*  LCL_VAR   int    V18 tmp17        u:1 (last use)

***** BB06
STMT00004 (IL   ???...  ???)
N023 ( 40, 30) [000011] -ACXG-------              *  CALL      void   Accretion.JitDumpVisualizer.Parsing.Tokens.Tokenizer.TokenizeLine
N003 (  1,  3) [000214] -A------R-L- this SETUP   +--*  ASG       ref   
N002 (  1,  1) [000213] D------N----              |  +--*  LCL_VAR   ref    V26 tmp25        d:1
N001 (  1,  1) [000007] ------------              |  \--*  LCL_VAR   ref    V02 tmp1         u:1 (last use)
N019 ( 18, 15) [000212] -A--------L- arg1 SETUP   +--*  COMMA     void  
N012 ( 10,  8) [000205] -A----------              |  +--*  COMMA     void  
N007 (  3,  3) [000200] -A------R---              |  |  +--*  ASG       byref 
N006 (  1,  1) [000199] D------N----              |  |  |  +--*  LCL_VAR   byref  V25 tmp24        d:1
N005 (  3,  3) [000197] ------------              |  |  |  \--*  ADDR      byref 
N004 (  3,  2) [000198] -------N----              |  |  |     \--*  LCL_VAR   struct<System.ReadOnlySpan`1[Char], 16>(AX) V24 tmp23        
N011 (  7,  5) [000204] -A----------              |  |  \--*  ASG       byref 
N009 (  3,  2) [000202] *------N----              |  |     +--*  IND       byref 
N008 (  1,  1) [000201] ------------              |  |     |  \--*  LCL_VAR   byref  V25 tmp24        u:1 Zero Fseq[_pointer]
N010 (  3,  2) [000203] -------N----              |  |     \--*  LCL_VAR   byref  V15 tmp14        u:1 (last use)
N018 (  8,  7) [000211] -A----------              |  \--*  ASG       int   
N016 (  4,  4) [000209] *------N----              |     +--*  IND       int   
N015 (  2,  2) [000208] -------N----              |     |  \--*  ADD       byref 
N013 (  1,  1) [000206] ------------              |     |     +--*  LCL_VAR   byref  V25 tmp24        u:1 (last use)
N014 (  1,  1) [000207] ------------              |     |     \--*  CNS_INT   long   8 Fseq[_length]
N017 (  3,  2) [000210] -------N----              |     \--*  LCL_VAR   int    V16 tmp15        u:1 (last use)
N020 (  1,  1) [000215] ------------ this in rcx  +--*  LCL_VAR   ref    V26 tmp25        u:1 (last use)
N022 (  3,  3) [000217] ------------ arg1 in rdx  \--*  ADDR      byref 
N021 (  3,  2) [000216] -------N----                 \--*  LCL_VAR   struct<System.ReadOnlySpan`1[Char], 16>(AX) V24 tmp23        
";
    }
}