using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static GenTreeNodeKind ParseGenTreeNodeKind(char* start, out int width)
        {
            Assert.True(Unsafe.SizeOf<GenTreeNodeKind>() is 1);

            ulong result;
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
                                    result = (ulong)GenTreeNodeKind.ADDR | ((ulong)"ADDR".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case '_':
                                    switch (start[4])
                                    {
                                        case 'H':
                                            Assert.Equal(start, "ADD_HI");
                                            result = (ulong)GenTreeNodeKind.ADD_HI | ((ulong)"ADD_HI".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "ADD_LO");
                                            result = (ulong)GenTreeNodeKind.ADD_LO | ((ulong)"ADD_LO".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "ADD");
                                    result = (ulong)GenTreeNodeKind.ADD | ((ulong)"ADD".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'L':
                            Assert.Equal(start, "ALLOCOBJ");
                            result = (ulong)GenTreeNodeKind.ALLOCOBJ | ((ulong)"ALLOCOBJ".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'N':
                            Assert.Equal(start, "AND");
                            result = (ulong)GenTreeNodeKind.AND | ((ulong)"AND".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'R':
                            switch (start[5])
                            {
                                case 'A':
                                    Assert.Equal(start, "ARGPLACE");
                                    result = (ulong)GenTreeNodeKind.ARGPLACE | ((ulong)"ARGPLACE".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'O':
                                    Assert.Equal(start, "ARR_BOUNDS_CHECK");
                                    result = (ulong)GenTreeNodeKind.ARR_BOUNDS_CHECK | ((ulong)"ARR_BOUNDS_CHECK".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'L':
                                    Assert.Equal(start, "ARR_ELEM");
                                    result = (ulong)GenTreeNodeKind.ARR_ELEM | ((ulong)"ARR_ELEM".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'N':
                                    Assert.Equal(start, "ARR_INDEX");
                                    result = (ulong)GenTreeNodeKind.ARR_INDEX | ((ulong)"ARR_INDEX".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'E':
                                    Assert.Equal(start, "ARR_LENGTH");
                                    result = (ulong)GenTreeNodeKind.ARR_LENGTH | ((ulong)"ARR_LENGTH".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "ARR_OFFSET");
                                    result = (ulong)GenTreeNodeKind.ARR_OFFSET | ((ulong)"ARR_OFFSET".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        default:
                            Assert.Equal(start, "ASG");
                            result = (ulong)GenTreeNodeKind.ASG | ((ulong)"ASG".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'B':
                    switch (start[1])
                    {
                        case 'I':
                            Assert.Equal(start, "BITCAST");
                            result = (ulong)GenTreeNodeKind.BITCAST | ((ulong)"BITCAST".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'L':
                            Assert.Equal(start, "BLK");
                            result = (ulong)GenTreeNodeKind.BLK | ((ulong)"BLK".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'O':
                            Assert.Equal(start, "BOX");
                            result = (ulong)GenTreeNodeKind.BOX | ((ulong)"BOX".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'S':
                            switch (start[5])
                            {
                                case '1':
                                    Assert.Equal(start, "BSWAP16");
                                    result = (ulong)GenTreeNodeKind.BSWAP16 | ((ulong)"BSWAP16".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "BSWAP");
                                    result = (ulong)GenTreeNodeKind.BSWAP | ((ulong)"BSWAP".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        default:
                            Assert.Equal(start, "BT");
                            result = (ulong)GenTreeNodeKind.BT | ((ulong)"BT".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
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
                                    result = (ulong)GenTreeNodeKind.CALL | ((ulong)"CALL".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'S':
                                    Assert.Equal(start, "CAST");
                                    result = (ulong)GenTreeNodeKind.CAST | ((ulong)"CAST".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "CATCH_ARG");
                                    result = (ulong)GenTreeNodeKind.CATCH_ARG | ((ulong)"CATCH_ARG".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'K':
                            Assert.Equal(start, "CKFINITE");
                            result = (ulong)GenTreeNodeKind.CKFINITE | ((ulong)"CKFINITE".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'L':
                            switch (start[7])
                            {
                                case '_':
                                    Assert.Equal(start, "CLS_VAR_ADDR");
                                    result = (ulong)GenTreeNodeKind.CLS_VAR_ADDR | ((ulong)"CLS_VAR_ADDR".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "CLS_VAR");
                                    result = (ulong)GenTreeNodeKind.CLS_VAR | ((ulong)"CLS_VAR".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'M':
                            switch (start[3])
                            {
                                case 'X':
                                    Assert.Equal(start, "CMPXCHG");
                                    result = (ulong)GenTreeNodeKind.CMPXCHG | ((ulong)"CMPXCHG".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "CMP");
                                    result = (ulong)GenTreeNodeKind.CMP | ((ulong)"CMP".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'N':
                            switch (start[4])
                            {
                                case 'N':
                                    Assert.Equal(start, "CNS_DBL");
                                    result = (ulong)GenTreeNodeKind.CNS_DBL | ((ulong)"CNS_DBL".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'I':
                                    Assert.Equal(start, "CNS_INT");
                                    result = (ulong)GenTreeNodeKind.CNS_INT | ((ulong)"CNS_INT".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'L':
                                    Assert.Equal(start, "CNS_LNG");
                                    result = (ulong)GenTreeNodeKind.CNS_LNG | ((ulong)"CNS_LNG".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "CNS_STR");
                                    result = (ulong)GenTreeNodeKind.CNS_STR | ((ulong)"CNS_STR".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        default:
                            switch (start[2])
                            {
                                case 'L':
                                    Assert.Equal(start, "COLON");
                                    result = (ulong)GenTreeNodeKind.COLON | ((ulong)"COLON".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'M':
                                    Assert.Equal(start, "COMMA");
                                    result = (ulong)GenTreeNodeKind.COMMA | ((ulong)"COMMA".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "COPY");
                                    result = (ulong)GenTreeNodeKind.COPY | ((ulong)"COPY".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'D':
                    switch (start[1])
                    {
                        case 'I':
                            Assert.Equal(start, "DIV");
                            result = (ulong)GenTreeNodeKind.DIV | ((ulong)"DIV".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "DYN_BLK");
                            result = (ulong)GenTreeNodeKind.DYN_BLK | ((ulong)"DYN_BLK".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'E':
                    switch (start[1])
                    {
                        case 'M':
                            Assert.Equal(start, "EMITNOP");
                            result = (ulong)GenTreeNodeKind.EMITNOP | ((ulong)"EMITNOP".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'N':
                            Assert.Equal(start, "END_LFIN");
                            result = (ulong)GenTreeNodeKind.END_LFIN | ((ulong)"END_LFIN".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "EQ");
                            result = (ulong)GenTreeNodeKind.EQ | ((ulong)"EQ".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
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
                                    result = (ulong)GenTreeNodeKind.FIELD_LIST | ((ulong)"FIELD_LIST".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "FIELD");
                                    result = (ulong)GenTreeNodeKind.FIELD | ((ulong)"FIELD".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        default:
                            Assert.Equal(start, "FTN_ADDR");
                            result = (ulong)GenTreeNodeKind.FTN_ADDR | ((ulong)"FTN_ADDR".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'G':
                    switch (start[1])
                    {
                        case 'E':
                            Assert.Equal(start, "GE");
                            result = (ulong)GenTreeNodeKind.GE | ((ulong)"GE".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "GT");
                            result = (ulong)GenTreeNodeKind.GT | ((ulong)"GT".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'H':
                    switch (start[2])
                    {
                        case 'I':
                            Assert.Equal(start, "HWINTRINSIC");
                            result = (ulong)GenTreeNodeKind.HWINTRINSIC | ((ulong)"HWINTRINSIC".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "HW_INTRINSIC_CHK");
                            result = (ulong)GenTreeNodeKind.HW_INTRINSIC_CHK | ((ulong)"HW_INTRINSIC_CHK".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'I':
                    switch (start[1])
                    {
                        case 'L':
                            Assert.Equal(start, "IL_OFFSET");
                            result = (ulong)GenTreeNodeKind.IL_OFFSET | ((ulong)"IL_OFFSET".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
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
                                                    result = (ulong)GenTreeNodeKind.INDEX_ADDR | ((ulong)"INDEX_ADDR".Length << (sizeof(GenTreeNodeKind) * 8));
                                                    break;
                                                default:
                                                    Assert.Equal(start, "INDEX");
                                                    result = (ulong)GenTreeNodeKind.INDEX | ((ulong)"INDEX".Length << (sizeof(GenTreeNodeKind) * 8));
                                                    break;
                                            }
                                            break;
                                        default:
                                            Assert.Equal(start, "IND");
                                            result = (ulong)GenTreeNodeKind.IND | ((ulong)"IND".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                    }
                                    break;
                                case 'I':
                                    Assert.Equal(start, "INIT_VAL");
                                    result = (ulong)GenTreeNodeKind.INIT_VAL | ((ulong)"INIT_VAL".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "INTRINSIC");
                                    result = (ulong)GenTreeNodeKind.INTRINSIC | ((ulong)"INTRINSIC".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
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
                                    result = (ulong)GenTreeNodeKind.JCC | ((ulong)"JCC".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "JCMP");
                                    result = (ulong)GenTreeNodeKind.JCMP | ((ulong)"JCMP".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'M':
                            switch (start[3])
                            {
                                case 'T':
                                    Assert.Equal(start, "JMPTABLE");
                                    result = (ulong)GenTreeNodeKind.JMPTABLE | ((ulong)"JMPTABLE".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "JMP");
                                    result = (ulong)GenTreeNodeKind.JMP | ((ulong)"JMP".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        default:
                            Assert.Equal(start, "JTRUE");
                            result = (ulong)GenTreeNodeKind.JTRUE | ((ulong)"JTRUE".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'K':
                    Assert.Equal(start, "KEEPALIVE");
                    result = (ulong)GenTreeNodeKind.KEEPALIVE | ((ulong)"KEEPALIVE".Length << (sizeof(GenTreeNodeKind) * 8));
                    break;
                case 'L':
                    switch (start[1])
                    {
                        case 'A':
                            Assert.Equal(start, "LABEL");
                            result = (ulong)GenTreeNodeKind.LABEL | ((ulong)"LABEL".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'C':
                            switch (start[4])
                            {
                                case 'E':
                                    Assert.Equal(start, "LCLHEAP");
                                    result = (ulong)GenTreeNodeKind.LCLHEAP | ((ulong)"LCLHEAP".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'F':
                                    switch (start[7])
                                    {
                                        case '_':
                                            Assert.Equal(start, "LCL_FLD_ADDR");
                                            result = (ulong)GenTreeNodeKind.LCL_FLD_ADDR | ((ulong)"LCL_FLD_ADDR".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "LCL_FLD");
                                            result = (ulong)GenTreeNodeKind.LCL_FLD | ((ulong)"LCL_FLD".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                    }
                                    break;
                                default:
                                    switch (start[7])
                                    {
                                        case '_':
                                            Assert.Equal(start, "LCL_VAR_ADDR");
                                            result = (ulong)GenTreeNodeKind.LCL_VAR_ADDR | ((ulong)"LCL_VAR_ADDR".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "LCL_VAR");
                                            result = (ulong)GenTreeNodeKind.LCL_VAR | ((ulong)"LCL_VAR".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case 'E':
                            switch (start[2])
                            {
                                case 'A':
                                    Assert.Equal(start, "LEA");
                                    result = (ulong)GenTreeNodeKind.LEA | ((ulong)"LEA".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "LE");
                                    result = (ulong)GenTreeNodeKind.LE | ((ulong)"LE".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'I':
                            Assert.Equal(start, "LIST");
                            result = (ulong)GenTreeNodeKind.LIST | ((ulong)"LIST".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'O':
                            switch (start[2])
                            {
                                case 'C':
                                    Assert.Equal(start, "LOCKADD");
                                    result = (ulong)GenTreeNodeKind.LOCKADD | ((ulong)"LOCKADD".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "LONG");
                                    result = (ulong)GenTreeNodeKind.LONG | ((ulong)"LONG".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'S':
                            switch (start[3])
                            {
                                case '_':
                                    Assert.Equal(start, "LSH_HI");
                                    result = (ulong)GenTreeNodeKind.LSH_HI | ((ulong)"LSH_HI".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "LSH");
                                    result = (ulong)GenTreeNodeKind.LSH | ((ulong)"LSH".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        default:
                            Assert.Equal(start, "LT");
                            result = (ulong)GenTreeNodeKind.LT | ((ulong)"LT".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'M':
                    switch (start[1])
                    {
                        case 'E':
                            Assert.Equal(start, "MEMORYBARRIER");
                            result = (ulong)GenTreeNodeKind.MEMORYBARRIER | ((ulong)"MEMORYBARRIER".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'K':
                            Assert.Equal(start, "MKREFANY");
                            result = (ulong)GenTreeNodeKind.MKREFANY | ((ulong)"MKREFANY".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'O':
                            Assert.Equal(start, "MOD");
                            result = (ulong)GenTreeNodeKind.MOD | ((ulong)"MOD".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            switch (start[3])
                            {
                                case 'H':
                                    Assert.Equal(start, "MULHI");
                                    result = (ulong)GenTreeNodeKind.MULHI | ((ulong)"MULHI".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case '_':
                                    Assert.Equal(start, "MUL_LONG");
                                    result = (ulong)GenTreeNodeKind.MUL_LONG | ((ulong)"MUL_LONG".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "MUL");
                                    result = (ulong)GenTreeNodeKind.MUL | ((ulong)"MUL".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'N':
                    switch (start[2])
                    {
                        case 'G':
                            Assert.Equal(start, "NEG");
                            result = (ulong)GenTreeNodeKind.NEG | ((ulong)"NEG".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'P':
                            Assert.Equal(start, "NOP");
                            result = (ulong)GenTreeNodeKind.NOP | ((ulong)"NOP".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'T':
                            Assert.Equal(start, "NOT");
                            result = (ulong)GenTreeNodeKind.NOT | ((ulong)"NOT".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case '_':
                            Assert.Equal(start, "NO_OP");
                            result = (ulong)GenTreeNodeKind.NO_OP | ((ulong)"NO_OP".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'L':
                            Assert.Equal(start, "NULLCHECK");
                            result = (ulong)GenTreeNodeKind.NULLCHECK | ((ulong)"NULLCHECK".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "NE");
                            result = (ulong)GenTreeNodeKind.NE | ((ulong)"NE".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'O':
                    switch (start[1])
                    {
                        case 'B':
                            Assert.Equal(start, "OBJ");
                            result = (ulong)GenTreeNodeKind.OBJ | ((ulong)"OBJ".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "OR");
                            result = (ulong)GenTreeNodeKind.OR | ((ulong)"OR".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
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
                                    result = (ulong)GenTreeNodeKind.PHI_ARG | ((ulong)"PHI_ARG".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "PHI");
                                    result = (ulong)GenTreeNodeKind.PHI | ((ulong)"PHI".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'Y':
                            Assert.Equal(start, "PHYSREG");
                            result = (ulong)GenTreeNodeKind.PHYSREG | ((ulong)"PHYSREG".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'N':
                            switch (start[8])
                            {
                                case 'E':
                                    Assert.Equal(start, "PINVOKE_EPILOG");
                                    result = (ulong)GenTreeNodeKind.PINVOKE_EPILOG | ((ulong)"PINVOKE_EPILOG".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "PINVOKE_PROLOG");
                                    result = (ulong)GenTreeNodeKind.PINVOKE_PROLOG | ((ulong)"PINVOKE_PROLOG".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'O':
                            Assert.Equal(start, "PROF_HOOK");
                            result = (ulong)GenTreeNodeKind.PROF_HOOK | ((ulong)"PROF_HOOK".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            switch (start[8])
                            {
                                case 'E':
                                    Assert.Equal(start, "PUTARG_REG");
                                    result = (ulong)GenTreeNodeKind.PUTARG_REG | ((ulong)"PUTARG_REG".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'P':
                                    Assert.Equal(start, "PUTARG_SPLIT");
                                    result = (ulong)GenTreeNodeKind.PUTARG_SPLIT | ((ulong)"PUTARG_SPLIT".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'T':
                                    Assert.Equal(start, "PUTARG_STK");
                                    result = (ulong)GenTreeNodeKind.PUTARG_STK | ((ulong)"PUTARG_STK".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "PUTARG_TYPE");
                                    result = (ulong)GenTreeNodeKind.PUTARG_TYPE | ((ulong)"PUTARG_TYPE".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                    }
                    break;
                case 'Q':
                    Assert.Equal(start, "QMARK");
                    result = (ulong)GenTreeNodeKind.QMARK | ((ulong)"QMARK".Length << (sizeof(GenTreeNodeKind) * 8));
                    break;
                case 'R':
                    switch (start[2])
                    {
                        case 'L':
                            switch (start[3])
                            {
                                case 'O':
                                    Assert.Equal(start, "RELOAD");
                                    result = (ulong)GenTreeNodeKind.RELOAD | ((ulong)"RELOAD".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "ROL");
                                    result = (ulong)GenTreeNodeKind.ROL | ((ulong)"ROL".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'T':
                            switch (start[3])
                            {
                                case 'F':
                                    Assert.Equal(start, "RETFILT");
                                    result = (ulong)GenTreeNodeKind.RETFILT | ((ulong)"RETFILT".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'U':
                                    switch (start[6])
                                    {
                                        case 'T':
                                            Assert.Equal(start, "RETURNTRAP");
                                            result = (ulong)GenTreeNodeKind.RETURNTRAP | ((ulong)"RETURNTRAP".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "RETURN");
                                            result = (ulong)GenTreeNodeKind.RETURN | ((ulong)"RETURN".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "RET_EXPR");
                                    result = (ulong)GenTreeNodeKind.RET_EXPR | ((ulong)"RET_EXPR".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'R':
                            Assert.Equal(start, "ROR");
                            result = (ulong)GenTreeNodeKind.ROR | ((ulong)"ROR".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'H':
                            switch (start[3])
                            {
                                case '_':
                                    Assert.Equal(start, "RSH_LO");
                                    result = (ulong)GenTreeNodeKind.RSH_LO | ((ulong)"RSH_LO".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "RSH");
                                    result = (ulong)GenTreeNodeKind.RSH | ((ulong)"RSH".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'Z':
                            Assert.Equal(start, "RSZ");
                            result = (ulong)GenTreeNodeKind.RSZ | ((ulong)"RSZ".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "RUNTIMELOOKUP");
                            result = (ulong)GenTreeNodeKind.RUNTIMELOOKUP | ((ulong)"RUNTIMELOOKUP".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'S':
                    switch (start[1])
                    {
                        case 'E':
                            Assert.Equal(start, "SETCC");
                            result = (ulong)GenTreeNodeKind.SETCC | ((ulong)"SETCC".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'I':
                            switch (start[4])
                            {
                                case '_':
                                    Assert.Equal(start, "SIMD_CHK");
                                    result = (ulong)GenTreeNodeKind.SIMD_CHK | ((ulong)"SIMD_CHK".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    Assert.Equal(start, "SIMD");
                                    result = (ulong)GenTreeNodeKind.SIMD | ((ulong)"SIMD".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        case 'T':
                            switch (start[7])
                            {
                                case 'O':
                                    Assert.Equal(start, "START_NONGC");
                                    result = (ulong)GenTreeNodeKind.START_NONGC | ((ulong)"START_NONGC".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'R':
                                    Assert.Equal(start, "START_PREEMPTGC");
                                    result = (ulong)GenTreeNodeKind.START_PREEMPTGC | ((ulong)"START_PREEMPTGC".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'D':
                                    Assert.Equal(start, "STOREIND");
                                    result = (ulong)GenTreeNodeKind.STOREIND | ((ulong)"STOREIND".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'L':
                                    Assert.Equal(start, "STORE_BLK");
                                    result = (ulong)GenTreeNodeKind.STORE_BLK | ((ulong)"STORE_BLK".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'Y':
                                    Assert.Equal(start, "STORE_DYN_BLK");
                                    result = (ulong)GenTreeNodeKind.STORE_DYN_BLK | ((ulong)"STORE_DYN_BLK".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                case 'C':
                                    switch (start[10])
                                    {
                                        case 'F':
                                            Assert.Equal(start, "STORE_LCL_FLD");
                                            result = (ulong)GenTreeNodeKind.STORE_LCL_FLD | ((ulong)"STORE_LCL_FLD".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "STORE_LCL_VAR");
                                            result = (ulong)GenTreeNodeKind.STORE_LCL_VAR | ((ulong)"STORE_LCL_VAR".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "STORE_OBJ");
                                    result = (ulong)GenTreeNodeKind.STORE_OBJ | ((ulong)"STORE_OBJ".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
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
                                            result = (ulong)GenTreeNodeKind.SUB_HI | ((ulong)"SUB_HI".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "SUB_LO");
                                            result = (ulong)GenTreeNodeKind.SUB_LO | ((ulong)"SUB_LO".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                    }
                                    break;
                                default:
                                    Assert.Equal(start, "SUB");
                                    result = (ulong)GenTreeNodeKind.SUB | ((ulong)"SUB".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                            }
                            break;
                        default:
                            switch (start[2])
                            {
                                case 'A':
                                    Assert.Equal(start, "SWAP");
                                    result = (ulong)GenTreeNodeKind.SWAP | ((ulong)"SWAP".Length << (sizeof(GenTreeNodeKind) * 8));
                                    break;
                                default:
                                    switch (start[6])
                                    {
                                        case '_':
                                            Assert.Equal(start, "SWITCH_TABLE");
                                            result = (ulong)GenTreeNodeKind.SWITCH_TABLE | ((ulong)"SWITCH_TABLE".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                        default:
                                            Assert.Equal(start, "SWITCH");
                                            result = (ulong)GenTreeNodeKind.SWITCH | ((ulong)"SWITCH".Length << (sizeof(GenTreeNodeKind) * 8));
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
                case 'T':
                    switch (start[5])
                    {
                        case 'E':
                            Assert.Equal(start, "TEST_EQ");
                            result = (ulong)GenTreeNodeKind.TEST_EQ | ((ulong)"TEST_EQ".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'N':
                            Assert.Equal(start, "TEST_NE");
                            result = (ulong)GenTreeNodeKind.TEST_NE | ((ulong)"TEST_NE".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "TURE_ARG_SPLIT");
                            result = (ulong)GenTreeNodeKind.TURE_ARG_SPLIT | ((ulong)"TURE_ARG_SPLIT".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                case 'U':
                    switch (start[1])
                    {
                        case 'D':
                            Assert.Equal(start, "UDIV");
                            result = (ulong)GenTreeNodeKind.UDIV | ((ulong)"UDIV".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "UMOD");
                            result = (ulong)GenTreeNodeKind.UMOD | ((ulong)"UMOD".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
                default:
                    switch (start[1])
                    {
                        case 'A':
                            Assert.Equal(start, "XADD");
                            result = (ulong)GenTreeNodeKind.XADD | ((ulong)"XADD".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        case 'C':
                            Assert.Equal(start, "XCHG");
                            result = (ulong)GenTreeNodeKind.XCHG | ((ulong)"XCHG".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                        default:
                            Assert.Equal(start, "XOR");
                            result = (ulong)GenTreeNodeKind.XOR | ((ulong)"XOR".Length << (sizeof(GenTreeNodeKind) * 8));
                            break;
                    }
                    break;
            }

            width = (int)(result >> (sizeof(GenTreeNodeKind) * 8));
            return (GenTreeNodeKind)result;
        }
    }
}
