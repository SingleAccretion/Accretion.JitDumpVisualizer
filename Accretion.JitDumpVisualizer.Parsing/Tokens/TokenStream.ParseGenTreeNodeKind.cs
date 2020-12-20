using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal unsafe partial struct TokenStream
    {
        private static GenTreeNodeKind ParseGenTreeNodeKind(char* start, out nint width)
        {
            GenTreeNodeKind kind;
            switch (start[0])
            {
                case 'A':
                    switch (start[1])
                    {
                        case 'D':
                            switch (start[3])
                            {
                                case 'R':
                                    Assert.Equal(start, "ADDR");
                                    kind = GenTreeNodeKind.ADDR;
                                    width = "ADDR".Length;
                                    break;
                                case '_':
                                    switch (start[4])
                                    {
                                        case 'H':
                                            Assert.Equal(start, "ADD_HI");
                                            kind = GenTreeNodeKind.ADD_HI;
                                            width = "ADD_HI".Length;
                                            break;
                                        case 'L':
                                            Assert.Equal(start, "ADD_LO");
                                            kind = GenTreeNodeKind.ADD_LO;
                                            width = "ADD_LO".Length;
                                            break;
                                        default: Assert.Impossible(start); goto ReturnUnknown;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "ADD");
                                    kind = GenTreeNodeKind.ADD;
                                    width = "ADD".Length;
                                    break;
                            }
                            break;
                        case 'L':
                            Assert.Equal(start, "ALLOCOBJ");
                            kind = GenTreeNodeKind.ALLOCOBJ;
                            width = "ALLOCOBJ".Length;
                            break;
                        case 'N':
                            Assert.Equal(start, "AND");
                            kind = GenTreeNodeKind.AND;
                            width = "AND".Length;
                            break;
                        case 'R':
                            switch (start[5])
                            {
                                case 'A':
                                    Assert.Equal(start, "ARGPLACE");
                                    kind = GenTreeNodeKind.ARGPLACE;
                                    width = "ARGPLACE".Length;
                                    break;
                                case 'O':
                                    Assert.Equal(start, "ARR_BOUNDS_CHECK");
                                    kind = GenTreeNodeKind.ARR_BOUNDS_CHECK;
                                    width = "ARR_BOUNDS_CHECK".Length;
                                    break;
                                case 'L':
                                    Assert.Equal(start, "ARR_ELEM");
                                    kind = GenTreeNodeKind.ARR_ELEM;
                                    width = "ARR_ELEM".Length;
                                    break;
                                case 'N':
                                    Assert.Equal(start, "ARR_INDEX");
                                    kind = GenTreeNodeKind.ARR_INDEX;
                                    width = "ARR_INDEX".Length;
                                    break;
                                case 'E':
                                    Assert.Equal(start, "ARR_LENGTH");
                                    kind = GenTreeNodeKind.ARR_LENGTH;
                                    width = "ARR_LENGTH".Length;
                                    break;
                                case 'F':
                                    Assert.Equal(start, "ARR_OFFSET");
                                    kind = GenTreeNodeKind.ARR_OFFSET;
                                    width = "ARR_OFFSET".Length;
                                    break;
                                default: Assert.Impossible(start); goto ReturnUnknown;
                            }
                            break;
                        case 'S':
                            Assert.Equal(start, "ASG");
                            kind = GenTreeNodeKind.ASG;
                            width = "ASG".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'B':
                    switch (start[1])
                    {
                        case 'I':
                            Assert.Equal(start, "BITCAST");
                            kind = GenTreeNodeKind.BITCAST;
                            width = "BITCAST".Length;
                            break;
                        case 'L':
                            Assert.Equal(start, "BLK");
                            kind = GenTreeNodeKind.BLK;
                            width = "BLK".Length;
                            break;
                        case 'O':
                            Assert.Equal(start, "BOX");
                            kind = GenTreeNodeKind.BOX;
                            width = "BOX".Length;
                            break;
                        case 'S':
                            switch (start[5])
                            {
                                case '1':
                                    Assert.Equal(start, "BSWAP16");
                                    kind = GenTreeNodeKind.BSWAP16;
                                    width = "BSWAP16".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "BSWAP");
                                    kind = GenTreeNodeKind.BSWAP;
                                    width = "BSWAP".Length;
                                    break;
                            }
                            break;
                        case 'T':
                            Assert.Equal(start, "BT");
                            kind = GenTreeNodeKind.BT;
                            width = "BT".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'C':
                    switch (start[1])
                    {
                        case 'A':
                            switch (start[2])
                            {
                                case 'L':
                                    Assert.Equal(start, "CALL");
                                    kind = GenTreeNodeKind.CALL;
                                    width = "CALL".Length;
                                    break;
                                case 'S':
                                    Assert.Equal(start, "CAST");
                                    kind = GenTreeNodeKind.CAST;
                                    width = "CAST".Length;
                                    break;
                                case 'T':
                                    Assert.Equal(start, "CATCH_ARG");
                                    kind = GenTreeNodeKind.CATCH_ARG;
                                    width = "CATCH_ARG".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case 'K':
                            Assert.Equal(start, "CKFINITE");
                            kind = GenTreeNodeKind.CKFINITE;
                            width = "CKFINITE".Length;
                            break;
                        case 'L':
                            switch (start[7])
                            {
                                case '_':
                                    Assert.Equal(start, "CLS_VAR_ADDR");
                                    kind = GenTreeNodeKind.CLS_VAR_ADDR;
                                    width = "CLS_VAR_ADDR".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "CLS_VAR");
                                    kind = GenTreeNodeKind.CLS_VAR;
                                    width = "CLS_VAR".Length;
                                    break;
                            }
                            break;
                        case 'M':
                            switch (start[3])
                            {
                                case 'X':
                                    Assert.Equal(start, "CMPXCHG");
                                    kind = GenTreeNodeKind.CMPXCHG;
                                    width = "CMPXCHG".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "CMP");
                                    kind = GenTreeNodeKind.CMP;
                                    width = "CMP".Length;
                                    break;
                            }
                            break;
                        case 'N':
                            switch (start[4])
                            {
                                case 'N':
                                    Assert.Equal(start, "CNS_DBL");
                                    kind = GenTreeNodeKind.CNS_DBL;
                                    width = "CNS_DBL".Length;
                                    break;
                                case 'I':
                                    Assert.Equal(start, "CNS_INT");
                                    kind = GenTreeNodeKind.CNS_INT;
                                    width = "CNS_INT".Length;
                                    break;
                                case 'L':
                                    Assert.Equal(start, "CNS_LNG");
                                    kind = GenTreeNodeKind.CNS_LNG;
                                    width = "CNS_LNG".Length;
                                    break;
                                case 'S':
                                    Assert.Equal(start, "CNS_STR");
                                    kind = GenTreeNodeKind.CNS_STR;
                                    width = "CNS_STR".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case 'O':
                            switch (start[2])
                            {
                                case 'L':
                                    Assert.Equal(start, "COLON");
                                    kind = GenTreeNodeKind.COLON;
                                    width = "COLON".Length;
                                    break;
                                case 'M':
                                    Assert.Equal(start, "COMMA");
                                    kind = GenTreeNodeKind.COMMA;
                                    width = "COMMA".Length;
                                    break;
                                case 'P':
                                    Assert.Equal(start, "COPY");
                                    kind = GenTreeNodeKind.COPY;
                                    width = "COPY".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'D':
                    switch (start[1])
                    {
                        case 'I':
                            Assert.Equal(start, "DIV");
                            kind = GenTreeNodeKind.DIV;
                            width = "DIV".Length;
                            break;
                        case 'Y':
                            Assert.Equal(start, "DYN_BLK");
                            kind = GenTreeNodeKind.DYN_BLK;
                            width = "DYN_BLK".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'E':
                    switch (start[1])
                    {
                        case 'M':
                            Assert.Equal(start, "EMITNOP");
                            kind = GenTreeNodeKind.EMITNOP;
                            width = "EMITNOP".Length;
                            break;
                        case 'N':
                            Assert.Equal(start, "END_LFIN");
                            kind = GenTreeNodeKind.END_LFIN;
                            width = "END_LFIN".Length;
                            break;
                        case 'Q':
                            Assert.Equal(start, "EQ");
                            kind = GenTreeNodeKind.EQ;
                            width = "EQ".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'F':
                    switch (start[1])
                    {
                        case 'I':
                            switch (start[5])
                            {
                                case '_':
                                    Assert.Equal(start, "FIELD_LIST");
                                    kind = GenTreeNodeKind.FIELD_LIST;
                                    width = "FIELD_LIST".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "FIELD");
                                    kind = GenTreeNodeKind.FIELD;
                                    width = "FIELD".Length;
                                    break;
                            }
                            break;
                        case 'T':
                            Assert.Equal(start, "FTN_ADDR");
                            kind = GenTreeNodeKind.FTN_ADDR;
                            width = "FTN_ADDR".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'G':
                    switch (start[1])
                    {
                        case 'E':
                            Assert.Equal(start, "GE");
                            kind = GenTreeNodeKind.GE;
                            width = "GE".Length;
                            break;
                        case 'T':
                            Assert.Equal(start, "GT");
                            kind = GenTreeNodeKind.GT;
                            width = "GT".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'H':
                    switch (start[2])
                    {
                        case 'I':
                            Assert.Equal(start, "HWINTRINSIC");
                            kind = GenTreeNodeKind.HWINTRINSIC;
                            width = "HWINTRINSIC".Length;
                            break;
                        case '_':
                            Assert.Equal(start, "HW_INTRINSIC_CHK");
                            kind = GenTreeNodeKind.HW_INTRINSIC_CHK;
                            width = "HW_INTRINSIC_CHK".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'I':
                    switch (start[1])
                    {
                        case 'L':
                            Assert.Equal(start, "IL_OFFSET");
                            kind = GenTreeNodeKind.IL_OFFSET;
                            width = "IL_OFFSET".Length;
                            break;
                        case 'N':
                            switch (start[2])
                            {
                                case 'D':
                                    switch (start[3])
                                    {
                                        case 'E':
                                            switch (start[5])
                                            {
                                                case '_':
                                                    Assert.Equal(start, "INDEX_ADDR");
                                                    kind = GenTreeNodeKind.INDEX_ADDR;
                                                    width = "INDEX_ADDR".Length;
                                                    break;
                                                default:
                                                    Assert.Equal(start, "INDEX");
                                                    kind = GenTreeNodeKind.INDEX;
                                                    width = "INDEX".Length;
                                                    break;
                                            }
                                            break;
                                        default:
                                            Assert.Equal(start, "IND");
                                            kind = GenTreeNodeKind.IND;
                                            width = "IND".Length;
                                            break;
                                    }
                                    break;
                                case 'I':
                                    Assert.Equal(start, "INIT_VAL");
                                    kind = GenTreeNodeKind.INIT_VAL;
                                    width = "INIT_VAL".Length;
                                    break;
                                case 'T':
                                    Assert.Equal(start, "INTRINSIC");
                                    kind = GenTreeNodeKind.INTRINSIC;
                                    width = "INTRINSIC".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'J':
                    switch (start[1])
                    {
                        case 'C':
                            switch (start[2])
                            {
                                case 'C':
                                    Assert.Equal(start, "JCC");
                                    kind = GenTreeNodeKind.JCC;
                                    width = "JCC".Length;
                                    break;
                                case 'M':
                                    Assert.Equal(start, "JCMP");
                                    kind = GenTreeNodeKind.JCMP;
                                    width = "JCMP".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case 'M':
                            switch (start[3])
                            {
                                case 'T':
                                    Assert.Equal(start, "JMPTABLE");
                                    kind = GenTreeNodeKind.JMPTABLE;
                                    width = "JMPTABLE".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "JMP");
                                    kind = GenTreeNodeKind.JMP;
                                    width = "JMP".Length;
                                    break;
                            }
                            break;
                        case 'T':
                            Assert.Equal(start, "JTRUE");
                            kind = GenTreeNodeKind.JTRUE;
                            width = "JTRUE".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'K':
                    Assert.Equal(start, "KEEPALIVE");
                    kind = GenTreeNodeKind.KEEPALIVE;
                    width = "KEEPALIVE".Length;
                    break;
                case 'L':
                    switch (start[1])
                    {
                        case 'A':
                            Assert.Equal(start, "LABEL");
                            kind = GenTreeNodeKind.LABEL;
                            width = "LABEL".Length;
                            break;
                        case 'C':
                            switch (start[4])
                            {
                                case 'E':
                                    Assert.Equal(start, "LCLHEAP");
                                    kind = GenTreeNodeKind.LCLHEAP;
                                    width = "LCLHEAP".Length;
                                    break;
                                case 'F':
                                    switch (start[7])
                                    {
                                        case '_':
                                            Assert.Equal(start, "LCL_FLD_ADDR");
                                            kind = GenTreeNodeKind.LCL_FLD_ADDR;
                                            width = "LCL_FLD_ADDR".Length;
                                            break;
                                        default:
                                            Assert.Equal(start, "LCL_FLD");
                                            kind = GenTreeNodeKind.LCL_FLD;
                                            width = "LCL_FLD".Length;
                                            break;
                                    }
                                    break;
                                case 'V':
                                    switch (start[7])
                                    {
                                        case '_':
                                            Assert.Equal(start, "LCL_VAR_ADDR");
                                            kind = GenTreeNodeKind.LCL_VAR_ADDR;
                                            width = "LCL_VAR_ADDR".Length;
                                            break;
                                        default:
                                            Assert.Equal(start, "LCL_VAR");
                                            kind = GenTreeNodeKind.LCL_VAR;
                                            width = "LCL_VAR".Length;
                                            break;
                                    }
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case 'E':
                            switch (start[2])
                            {
                                case 'A':
                                    Assert.Equal(start, "LEA");
                                    kind = GenTreeNodeKind.LEA;
                                    width = "LEA".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "LE");
                                    kind = GenTreeNodeKind.LE;
                                    width = "LE".Length;
                                    break;
                            }
                            break;
                        case 'I':
                            Assert.Equal(start, "LIST");
                            kind = GenTreeNodeKind.LIST;
                            width = "LIST".Length;
                            break;
                        case 'O':
                            switch (start[2])
                            {
                                case 'C':
                                    Assert.Equal(start, "LOCKADD");
                                    kind = GenTreeNodeKind.LOCKADD;
                                    width = "LOCKADD".Length;
                                    break;
                                case 'N':
                                    Assert.Equal(start, "LONG");
                                    kind = GenTreeNodeKind.LONG;
                                    width = "LONG".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case 'S':
                            switch (start[3])
                            {
                                case '_':
                                    Assert.Equal(start, "LSH_HI");
                                    kind = GenTreeNodeKind.LSH_HI;
                                    width = "LSH_HI".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "LSH");
                                    kind = GenTreeNodeKind.LSH;
                                    width = "LSH".Length;
                                    break;
                            }
                            break;
                        case 'T':
                            Assert.Equal(start, "LT");
                            kind = GenTreeNodeKind.LT;
                            width = "LT".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'M':
                    switch (start[1])
                    {
                        case 'E':
                            Assert.Equal(start, "MEMORYBARRIER");
                            kind = GenTreeNodeKind.MEMORYBARRIER;
                            width = "MEMORYBARRIER".Length;
                            break;
                        case 'K':
                            Assert.Equal(start, "MKREFANY");
                            kind = GenTreeNodeKind.MKREFANY;
                            width = "MKREFANY".Length;
                            break;
                        case 'O':
                            Assert.Equal(start, "MOD");
                            kind = GenTreeNodeKind.MOD;
                            width = "MOD".Length;
                            break;
                        case 'U':
                            switch (start[3])
                            {
                                case 'H':
                                    Assert.Equal(start, "MULHI");
                                    kind = GenTreeNodeKind.MULHI;
                                    width = "MULHI".Length;
                                    break;
                                case '_':
                                    Assert.Equal(start, "MUL_LONG");
                                    kind = GenTreeNodeKind.MUL_LONG;
                                    width = "MUL_LONG".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "MUL");
                                    kind = GenTreeNodeKind.MUL;
                                    width = "MUL".Length;
                                    break;
                            }
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'N':
                    switch (start[2])
                    {
                        case 'G':
                            Assert.Equal(start, "NEG");
                            kind = GenTreeNodeKind.NEG;
                            width = "NEG".Length;
                            break;
                        case 'P':
                            Assert.Equal(start, "NOP");
                            kind = GenTreeNodeKind.NOP;
                            width = "NOP".Length;
                            break;
                        case 'T':
                            Assert.Equal(start, "NOT");
                            kind = GenTreeNodeKind.NOT;
                            width = "NOT".Length;
                            break;
                        case '_':
                            Assert.Equal(start, "NO_OP");
                            kind = GenTreeNodeKind.NO_OP;
                            width = "NO_OP".Length;
                            break;
                        case 'L':
                            Assert.Equal(start, "NULLCHECK");
                            kind = GenTreeNodeKind.NULLCHECK;
                            width = "NULLCHECK".Length;
                            break;
                        default:
                            Assert.Equal(start, "NE");
                            kind = GenTreeNodeKind.NE;
                            width = "NE".Length;
                            break;
                    }
                    break;
                case 'O':
                    switch (start[1])
                    {
                        case 'B':
                            Assert.Equal(start, "OBJ");
                            kind = GenTreeNodeKind.OBJ;
                            width = "OBJ".Length;
                            break;
                        case 'R':
                            Assert.Equal(start, "OR");
                            kind = GenTreeNodeKind.OR;
                            width = "OR".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'P':
                    switch (start[2])
                    {
                        case 'I':
                            switch (start[3])
                            {
                                case '_':
                                    Assert.Equal(start, "PHI_ARG");
                                    kind = GenTreeNodeKind.PHI_ARG;
                                    width = "PHI_ARG".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "PHI");
                                    kind = GenTreeNodeKind.PHI;
                                    width = "PHI".Length;
                                    break;
                            }
                            break;
                        case 'Y':
                            Assert.Equal(start, "PHYSREG");
                            kind = GenTreeNodeKind.PHYSREG;
                            width = "PHYSREG".Length;
                            break;
                        case 'N':
                            switch (start[8])
                            {
                                case 'E':
                                    Assert.Equal(start, "PINVOKE_EPILOG");
                                    kind = GenTreeNodeKind.PINVOKE_EPILOG;
                                    width = "PINVOKE_EPILOG".Length;
                                    break;
                                case 'P':
                                    Assert.Equal(start, "PINVOKE_PROLOG");
                                    kind = GenTreeNodeKind.PINVOKE_PROLOG;
                                    width = "PINVOKE_PROLOG".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case 'O':
                            Assert.Equal(start, "PROF_HOOK");
                            kind = GenTreeNodeKind.PROF_HOOK;
                            width = "PROF_HOOK".Length;
                            break;
                        case 'U':
                            switch (start[8])
                            {
                                case 'E':
                                    Assert.Equal(start, "PUTARG_REG");
                                    kind = GenTreeNodeKind.PUTARG_REG;
                                    width = "PUTARG_REG".Length;
                                    break;
                                case 'P':
                                    Assert.Equal(start, "PUTARG_SPLIT");
                                    kind = GenTreeNodeKind.PUTARG_SPLIT;
                                    width = "PUTARG_SPLIT".Length;
                                    break;
                                case 'T':
                                    Assert.Equal(start, "PUTARG_STK");
                                    kind = GenTreeNodeKind.PUTARG_STK;
                                    width = "PUTARG_STK".Length;
                                    break;
                                case 'Y':
                                    Assert.Equal(start, "PUTARG_TYPE");
                                    kind = GenTreeNodeKind.PUTARG_TYPE;
                                    width = "PUTARG_TYPE".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'Q':
                    Assert.Equal(start, "QMARK");
                    kind = GenTreeNodeKind.QMARK;
                    width = "QMARK".Length;
                    break;
                case 'R':
                    switch (start[2])
                    {
                        case 'L':
                            switch (start[3])
                            {
                                case 'O':
                                    Assert.Equal(start, "RELOAD");
                                    kind = GenTreeNodeKind.RELOAD;
                                    width = "RELOAD".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "ROL");
                                    kind = GenTreeNodeKind.ROL;
                                    width = "ROL".Length;
                                    break;
                            }
                            break;
                        case 'T':
                            switch (start[3])
                            {
                                case 'F':
                                    Assert.Equal(start, "RETFILT");
                                    kind = GenTreeNodeKind.RETFILT;
                                    width = "RETFILT".Length;
                                    break;
                                case 'U':
                                    switch (start[6])
                                    {
                                        case 'T':
                                            Assert.Equal(start, "RETURNTRAP");
                                            kind = GenTreeNodeKind.RETURNTRAP;
                                            width = "RETURNTRAP".Length;
                                            break;
                                        default:
                                            Assert.Equal(start, "RETURN");
                                            kind = GenTreeNodeKind.RETURN;
                                            width = "RETURN".Length;
                                            break;
                                    }
                                    break;
                                case '_':
                                    Assert.Equal(start, "RET_EXPR");
                                    kind = GenTreeNodeKind.RET_EXPR;
                                    width = "RET_EXPR".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case 'R':
                            Assert.Equal(start, "ROR");
                            kind = GenTreeNodeKind.ROR;
                            width = "ROR".Length;
                            break;
                        case 'H':
                            switch (start[3])
                            {
                                case '_':
                                    Assert.Equal(start, "RSH_LO");
                                    kind = GenTreeNodeKind.RSH_LO;
                                    width = "RSH_LO".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "RSH");
                                    kind = GenTreeNodeKind.RSH;
                                    width = "RSH".Length;
                                    break;
                            }
                            break;
                        case 'Z':
                            Assert.Equal(start, "RSZ");
                            kind = GenTreeNodeKind.RSZ;
                            width = "RSZ".Length;
                            break;
                        case 'N':
                            Assert.Equal(start, "RUNTIMELOOKUP");
                            kind = GenTreeNodeKind.RUNTIMELOOKUP;
                            width = "RUNTIMELOOKUP".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'S':
                    switch (start[1])
                    {
                        case 'E':
                            Assert.Equal(start, "SETCC");
                            kind = GenTreeNodeKind.SETCC;
                            width = "SETCC".Length;
                            break;
                        case 'I':
                            switch (start[4])
                            {
                                case '_':
                                    Assert.Equal(start, "SIMD_CHK");
                                    kind = GenTreeNodeKind.SIMD_CHK;
                                    width = "SIMD_CHK".Length;
                                    break;
                                default:
                                    Assert.Equal(start, "SIMD");
                                    kind = GenTreeNodeKind.SIMD;
                                    width = "SIMD".Length;
                                    break;
                            }
                            break;
                        case 'T':
                            switch (start[7])
                            {
                                case 'O':
                                    Assert.Equal(start, "START_NONGC");
                                    kind = GenTreeNodeKind.START_NONGC;
                                    width = "START_NONGC".Length;
                                    break;
                                case 'R':
                                    Assert.Equal(start, "START_PREEMPTGC");
                                    kind = GenTreeNodeKind.START_PREEMPTGC;
                                    width = "START_PREEMPTGC".Length;
                                    break;
                                case 'D':
                                    Assert.Equal(start, "STOREIND");
                                    kind = GenTreeNodeKind.STOREIND;
                                    width = "STOREIND".Length;
                                    break;
                                case 'L':
                                    Assert.Equal(start, "STORE_BLK");
                                    kind = GenTreeNodeKind.STORE_BLK;
                                    width = "STORE_BLK".Length;
                                    break;
                                case 'Y':
                                    Assert.Equal(start, "STORE_DYN_BLK");
                                    kind = GenTreeNodeKind.STORE_DYN_BLK;
                                    width = "STORE_DYN_BLK".Length;
                                    break;
                                case 'C':
                                    switch (start[10])
                                    {
                                        case 'F':
                                            Assert.Equal(start, "STORE_LCL_FLD");
                                            kind = GenTreeNodeKind.STORE_LCL_FLD;
                                            width = "STORE_LCL_FLD".Length;
                                            break;
                                        case 'V':
                                            Assert.Equal(start, "STORE_LCL_VAR");
                                            kind = GenTreeNodeKind.STORE_LCL_VAR;
                                            width = "STORE_LCL_VAR".Length;
                                            break;
                                        default: goto ReturnUnknown;
                                    }
                                    break;
                                case 'J':
                                    Assert.Equal(start, "STORE_OBJ");
                                    kind = GenTreeNodeKind.STORE_OBJ;
                                    width = "STORE_OBJ".Length;
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        case 'U':
                            switch (start[3])
                            {
                                case '_':
                                    switch (start[4])
                                    {
                                        case 'H':
                                            Assert.Equal(start, "SUB_HI");
                                            kind = GenTreeNodeKind.SUB_HI;
                                            width = "SUB_HI".Length;
                                            break;
                                        case 'L':
                                            Assert.Equal(start, "SUB_LO");
                                            kind = GenTreeNodeKind.SUB_LO;
                                            width = "SUB_LO".Length;
                                            break;
                                        default: goto ReturnUnknown;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "SUB");
                                    kind = GenTreeNodeKind.SUB;
                                    width = "SUB".Length;
                                    break;
                            }
                            break;
                        case 'W':
                            switch (start[2])
                            {
                                case 'A':
                                    Assert.Equal(start, "SWAP");
                                    kind = GenTreeNodeKind.SWAP;
                                    width = "SWAP".Length;
                                    break;
                                case 'I':
                                    switch (start[6])
                                    {
                                        case '_':
                                            Assert.Equal(start, "SWITCH_TABLE");
                                            kind = GenTreeNodeKind.SWITCH_TABLE;
                                            width = "SWITCH_TABLE".Length;
                                            break;
                                        default:
                                            Assert.Equal(start, "SWITCH");
                                            kind = GenTreeNodeKind.SWITCH;
                                            width = "SWITCH".Length;
                                            break;
                                    }
                                    break;
                                default: goto ReturnUnknown;
                            }
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'T':
                    switch (start[5])
                    {
                        case 'E':
                            Assert.Equal(start, "TEST_EQ");
                            kind = GenTreeNodeKind.TEST_EQ;
                            width = "TEST_EQ".Length;
                            break;
                        case 'N':
                            Assert.Equal(start, "TEST_NE");
                            kind = GenTreeNodeKind.TEST_NE;
                            width = "TEST_NE".Length;
                            break;
                        case 'A':
                            Assert.Equal(start, "TURE_ARG_SPLIT");
                            kind = GenTreeNodeKind.TURE_ARG_SPLIT;
                            width = "TURE_ARG_SPLIT".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'U':
                    switch (start[1])
                    {
                        case 'D':
                            Assert.Equal(start, "UDIV");
                            kind = GenTreeNodeKind.UDIV;
                            width = "UDIV".Length;
                            break;
                        case 'M':
                            Assert.Equal(start, "UMOD");
                            kind = GenTreeNodeKind.UMOD;
                            width = "UMOD".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                case 'X':
                    switch (start[1])
                    {
                        case 'A':
                            Assert.Equal(start, "XADD");
                            kind = GenTreeNodeKind.XADD;
                            width = "XADD".Length;
                            break;
                        case 'C':
                            Assert.Equal(start, "XCHG");
                            kind = GenTreeNodeKind.XCHG;
                            width = "XCHG".Length;
                            break;
                        case 'O':
                            Assert.Equal(start, "XOR");
                            kind = GenTreeNodeKind.XOR;
                            width = "XOR".Length;
                            break;
                        default: goto ReturnUnknown;
                    }
                    break;
                default:
                ReturnUnknown:
                    Assert.Impossible(start);
                    kind = GenTreeNodeKind.Unknown;
                    width = 1;
                    break;
            }

            return kind;
        }
    }
}
