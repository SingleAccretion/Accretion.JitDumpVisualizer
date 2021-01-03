using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static GenTreeNodeKind ParseGenTreeNodeKind(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<GenTreeNodeKind>() is 1);

            var result = (start[0]) switch
            {
                'A' => (start[1]) switch
                {
                    'D' => (start[3]) switch
                    {
                        'R' => Result(GenTreeNodeKind.ADDR, "ADDR", start),
                        '_' => (start[4]) switch
                        {
                            'H' => Result(GenTreeNodeKind.ADD_HI, "ADD_HI", start),
                            _ => Result(GenTreeNodeKind.ADD_LO, "ADD_LO", start)
                        },
                        _ => Result(GenTreeNodeKind.ADD, "ADD", start)
                    },
                    'L' => Result(GenTreeNodeKind.ALLOCOBJ, "ALLOCOBJ", start),
                    'N' => Result(GenTreeNodeKind.AND, "AND", start),
                    'R' => (start[5]) switch
                    {
                        'A' => Result(GenTreeNodeKind.ARGPLACE, "ARGPLACE", start),
                        'O' => Result(GenTreeNodeKind.ARR_BOUNDS_CHECK, "ARR_BOUNDS_CHECK", start),
                        'L' => Result(GenTreeNodeKind.ARR_ELEM, "ARR_ELEM", start),
                        'N' => Result(GenTreeNodeKind.ARR_INDEX, "ARR_INDEX", start),
                        'E' => Result(GenTreeNodeKind.ARR_LENGTH, "ARR_LENGTH", start),
                        _ => Result(GenTreeNodeKind.ARR_OFFSET, "ARR_OFFSET", start)
                    },
                    _ => Result(GenTreeNodeKind.ASG, "ASG", start)
                },
                'B' => (start[1]) switch
                {
                    'I' => Result(GenTreeNodeKind.BITCAST, "BITCAST", start),
                    'L' => Result(GenTreeNodeKind.BLK, "BLK", start),
                    'O' => Result(GenTreeNodeKind.BOX, "BOX", start),
                    'S' => (start[5]) switch
                    {
                        '1' => Result(GenTreeNodeKind.BSWAP16, "BSWAP16", start),
                        _ => Result(GenTreeNodeKind.BSWAP, "BSWAP", start)
                    },
                    _ => Result(GenTreeNodeKind.BT, "BT", start)
                },
                'C' => (start[1]) switch
                {
                    'A' => (start[2]) switch
                    {
                        'L' => Result(GenTreeNodeKind.CALL, "CALL", start),
                        'S' => Result(GenTreeNodeKind.CAST, "CAST", start),
                        _ => Result(GenTreeNodeKind.CATCH_ARG, "CATCH_ARG", start)
                    },
                    'K' => Result(GenTreeNodeKind.CKFINITE, "CKFINITE", start),
                    'L' => (start[7]) switch
                    {
                        '_' => Result(GenTreeNodeKind.CLS_VAR_ADDR, "CLS_VAR_ADDR", start),
                        _ => Result(GenTreeNodeKind.CLS_VAR, "CLS_VAR", start)
                    },
                    'M' => (start[3]) switch
                    {
                        'X' => Result(GenTreeNodeKind.CMPXCHG, "CMPXCHG", start),
                        _ => Result(GenTreeNodeKind.CMP, "CMP", start)
                    },
                    'N' => (start[4]) switch
                    {
                        'N' => Result(GenTreeNodeKind.CNS_DBL, "CNS_DBL", start),
                        'I' => Result(GenTreeNodeKind.CNS_INT, "CNS_INT", start),
                        'L' => Result(GenTreeNodeKind.CNS_LNG, "CNS_LNG", start),
                        _ => Result(GenTreeNodeKind.CNS_STR, "CNS_STR", start)
                    },
                    _ => (start[2]) switch
                    {
                        'L' => Result(GenTreeNodeKind.COLON, "COLON", start),
                        'M' => Result(GenTreeNodeKind.COMMA, "COMMA", start),
                        _ => Result(GenTreeNodeKind.COPY, "COPY", start)
                    },
                },
                'D' => (start[1]) switch
                {
                    'I' => Result(GenTreeNodeKind.DIV, "DIV", start),
                    _ => Result(GenTreeNodeKind.DYN_BLK, "DYN_BLK", start)
                },
                'E' => (start[1]) switch
                {
                    'M' => Result(GenTreeNodeKind.EMITNOP, "EMITNOP", start),
                    'N' => Result(GenTreeNodeKind.END_LFIN, "END_LFIN", start),
                    _ => Result(GenTreeNodeKind.EQ, "EQ", start)
                },
                'F' => (start[1]) switch
                {
                    'I' => (start[5]) switch
                    {
                        '_' => Result(GenTreeNodeKind.FIELD_LIST, "FIELD_LIST", start),
                        _ => Result(GenTreeNodeKind.FIELD, "FIELD", start)
                    },
                    _ => Result(GenTreeNodeKind.FTN_ADDR, "FTN_ADDR", start)
                },
                'G' => (start[1]) switch
                {
                    'E' => Result(GenTreeNodeKind.GE, "GE", start),
                    _ => Result(GenTreeNodeKind.GT, "GT", start)
                },
                'H' => (start[2]) switch
                {
                    'I' => Result(GenTreeNodeKind.HWINTRINSIC, "HWINTRINSIC", start),
                    _ => Result(GenTreeNodeKind.HW_INTRINSIC_CHK, "HW_INTRINSIC_CHK", start)
                },
                'I' => (start[1]) switch
                {
                    'L' => Result(GenTreeNodeKind.IL_OFFSET, "IL_OFFSET", start),
                    _ => (start[2]) switch
                    {
                        'D' => (start[3]) switch
                        {
                            'E' => (start[5]) switch
                            {
                                '_' => Result(GenTreeNodeKind.INDEX_ADDR, "INDEX_ADDR", start),
                                _ => Result(GenTreeNodeKind.INDEX, "INDEX", start)
                            },
                            _ => Result(GenTreeNodeKind.IND, "IND", start)
                        },
                        'I' => Result(GenTreeNodeKind.INIT_VAL, "INIT_VAL", start),
                        _ => Result(GenTreeNodeKind.INTRINSIC, "INTRINSIC", start),
                    }
                },
                'J' => (start[1]) switch
                {
                    'C' => (start[2]) switch
                    {
                        'C' => Result(GenTreeNodeKind.JCC, "JCC", start),
                        _ => Result(GenTreeNodeKind.JCMP, "JCMP", start)
                    },
                    'M' => (start[3]) switch
                    {
                        'T' => Result(GenTreeNodeKind.JMPTABLE, "JMPTABLE", start),
                        _ => Result(GenTreeNodeKind.JMP, "JMP", start)
                    },
                    _ => Result(GenTreeNodeKind.JTRUE, "JTRUE", start)
                },
                'K' => Result(GenTreeNodeKind.KEEPALIVE, "KEEPALIVE", start),
                'L' => (start[1]) switch
                {
                    'A' => Result(GenTreeNodeKind.LABEL, "LABEL", start),
                    'C' => (start[4]) switch
                    {
                        'E' => Result(GenTreeNodeKind.LCLHEAP, "LCLHEAP", start),
                        'F' => (start[7]) switch
                        {
                            '_' => Result(GenTreeNodeKind.LCL_FLD_ADDR, "LCL_FLD_ADDR", start),
                            _ => Result(GenTreeNodeKind.LCL_FLD, "LCL_FLD", start)
                        },
                        _ => (start[7]) switch
                        {
                            '_' => Result(GenTreeNodeKind.LCL_VAR_ADDR, "LCL_VAR_ADDR", start),
                            _ => Result(GenTreeNodeKind.LCL_VAR, "LCL_VAR", start)
                        }
                    },
                    'E' => (start[2]) switch
                    {
                        'A' => Result(GenTreeNodeKind.LEA, "LEA", start),
                        _ => Result(GenTreeNodeKind.LE, "LE", start)
                    },
                    'I' => Result(GenTreeNodeKind.LIST, "LIST", start),
                    'O' => (start[2]) switch
                    {
                        'C' => Result(GenTreeNodeKind.LOCKADD, "LOCKADD", start),
                        _ => Result(GenTreeNodeKind.LONG, "LONG", start)
                    },
                    'S' => (start[3]) switch
                    {
                        '_' => Result(GenTreeNodeKind.LSH_HI, "LSH_HI", start),
                        _ => Result(GenTreeNodeKind.LSH, "LSH", start)
                    },
                    _ => Result(GenTreeNodeKind.LT, "LT", start)
                },
                'M' => (start[1]) switch
                {
                    'E' => Result(GenTreeNodeKind.MEMORYBARRIER, "MEMORYBARRIER", start),
                    'K' => Result(GenTreeNodeKind.MKREFANY, "MKREFANY", start),
                    'O' => Result(GenTreeNodeKind.MOD, "MOD", start),
                    _ => (start[3]) switch
                    {
                        'H' => Result(GenTreeNodeKind.MULHI, "MULHI", start),
                        '_' => Result(GenTreeNodeKind.MUL_LONG, "MUL_LONG", start),
                        _ => Result(GenTreeNodeKind.MUL, "MUL", start)
                    }
                },
                'N' => (start[2]) switch
                {
                    'G' => Result(GenTreeNodeKind.NEG, "NEG", start),
                    'P' => Result(GenTreeNodeKind.NOP, "NOP", start),
                    'T' => Result(GenTreeNodeKind.NOT, "NOT", start),
                    '_' => Result(GenTreeNodeKind.NO_OP, "NO_OP", start),
                    'L' => Result(GenTreeNodeKind.NULLCHECK, "NULLCHECK", start),
                    _ => Result(GenTreeNodeKind.NE, "NE", start)
                },
                'O' => (start[1]) switch
                {
                    'B' => Result(GenTreeNodeKind.OBJ, "OBJ", start),
                    _ => Result(GenTreeNodeKind.OR, "OR", start)
                },
                'P' => (start[2]) switch
                {
                    'I' => (start[3]) switch
                    {
                        '_' => Result(GenTreeNodeKind.PHI_ARG, "PHI_ARG", start),
                        _ => Result(GenTreeNodeKind.PHI, "PHI", start)
                    },
                    'Y' => Result(GenTreeNodeKind.PHYSREG, "PHYSREG", start),
                    'N' => (start[8]) switch
                    {
                        'E' => Result(GenTreeNodeKind.PINVOKE_EPILOG, "PINVOKE_EPILOG", start),
                        _ => Result(GenTreeNodeKind.PINVOKE_PROLOG, "PINVOKE_PROLOG", start)
                    },
                    'O' => Result(GenTreeNodeKind.PROF_HOOK, "PROF_HOOK", start),
                    _ => (start[8]) switch
                    {
                        'E' => Result(GenTreeNodeKind.PUTARG_REG, "PUTARG_REG", start),
                        'P' => Result(GenTreeNodeKind.PUTARG_SPLIT, "PUTARG_SPLIT", start),
                        'T' => Result(GenTreeNodeKind.PUTARG_STK, "PUTARG_STK", start),
                        _ => Result(GenTreeNodeKind.PUTARG_TYPE, "PUTARG_TYPE", start)
                    }
                },
                'Q' => Result(GenTreeNodeKind.QMARK, "QMARK", start),
                'R' => (start[2]) switch
                {
                    'L' => (start[3]) switch
                    {
                        'O' => Result(GenTreeNodeKind.RELOAD, "RELOAD", start),
                        _ => Result(GenTreeNodeKind.ROL, "ROL", start)
                    },
                    'T' => (start[3]) switch
                    {
                        'F' => Result(GenTreeNodeKind.RETFILT, "RETFILT", start),
                        'U' => (start[6]) switch
                        {
                            'T' => Result(GenTreeNodeKind.RETURNTRAP, "RETURNTRAP", start),
                            _ => Result(GenTreeNodeKind.RETURN, "RETURN", start)
                        },
                        _ => Result(GenTreeNodeKind.RET_EXPR, "RET_EXPR", start)
                    },
                    'R' => Result(GenTreeNodeKind.ROR, "ROR", start),
                    'H' => (start[3]) switch
                    {
                        '_' => Result(GenTreeNodeKind.RSH_LO, "RSH_LO", start),
                        _ => Result(GenTreeNodeKind.RSH, "RSH", start)
                    },
                    'Z' => Result(GenTreeNodeKind.RSZ, "RSZ", start),
                    _ => Result(GenTreeNodeKind.RUNTIMELOOKUP, "RUNTIMELOOKUP", start)
                },
                'S' => (start[1]) switch
                {
                    'E' => Result(GenTreeNodeKind.SETCC, "SETCC", start),
                    'I' => (start[4]) switch
                    {
                        '_' => Result(GenTreeNodeKind.SIMD_CHK, "SIMD_CHK", start),
                        _ => Result(GenTreeNodeKind.SIMD, "SIMD", start)
                    },
                    'T' => (start[7]) switch
                    {
                        'O' => Result(GenTreeNodeKind.START_NONGC, "START_NONGC", start),
                        'R' => Result(GenTreeNodeKind.START_PREEMPTGC, "START_PREEMPTGC", start),
                        'D' => Result(GenTreeNodeKind.STOREIND, "STOREIND", start),
                        'L' => Result(GenTreeNodeKind.STORE_BLK, "STORE_BLK", start),
                        'Y' => Result(GenTreeNodeKind.STORE_DYN_BLK, "STORE_DYN_BLK", start),
                        'C' => (start[10]) switch
                        {
                            'F' => Result(GenTreeNodeKind.STORE_LCL_FLD, "STORE_LCL_FLD", start),
                            _ => Result(GenTreeNodeKind.STORE_LCL_VAR, "STORE_LCL_VAR", start)
                        },
                        _ => Result(GenTreeNodeKind.STORE_OBJ, "STORE_OBJ", start)
                    },
                    'U' => (start[3]) switch
                    {
                        '_' => (start[4]) switch
                        {
                            'H' => Result(GenTreeNodeKind.SUB_HI, "SUB_HI", start),
                            _ => Result(GenTreeNodeKind.SUB_LO, "SUB_LO", start)
                        },
                        _ => Result(GenTreeNodeKind.SUB, "SUB", start)
                    },
                    _ => (start[2]) switch
                    {
                        'A' => Result(GenTreeNodeKind.SWAP, "SWAP", start),
                        _ => (start[6]) switch
                        {
                            '_' => Result(GenTreeNodeKind.SWITCH_TABLE, "SWITCH_TABLE", start),
                            _ => Result(GenTreeNodeKind.SWITCH, "SWITCH", start)
                        }
                    },
                },
                'T' => (start[5]) switch
                {
                    'E' => Result(GenTreeNodeKind.TEST_EQ, "TEST_EQ", start),
                    'N' => Result(GenTreeNodeKind.TEST_NE, "TEST_NE", start),
                    _ => Result(GenTreeNodeKind.TURE_ARG_SPLIT, "TURE_ARG_SPLIT", start)
                },
                'U' => (start[1]) switch
                {
                    'D' => Result(GenTreeNodeKind.UDIV, "UDIV", start),
                    _ => Result(GenTreeNodeKind.UMOD, "UMOD", start)
                },
                _ => (start[1]) switch
                {
                    'A' => Result(GenTreeNodeKind.XADD, "XADD", start),
                    'C' => Result(GenTreeNodeKind.XCHG, "XCHG", start),
                    _ => Result(GenTreeNodeKind.XOR, "XOR", start)
                }
            };
            
            return Result<GenTreeNodeKind>(result, out width);
        }
    }
}
