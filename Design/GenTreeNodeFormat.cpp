void Compiler::gtDispNode(GenTree* tree, IndentStack* indentStack, __in __in_z __in_opt const char* msg, bool isLIR)
{
    bool printPointer = true; // always true..
    bool printFlags   = true; // always true..
    bool printCost    = true; // always true..

    int msgLength = 25;

    GenTree* prev;

    if (tree->gtSeqNum)
    {
        printf("N%03u ", tree->gtSeqNum);
        if (tree->gtCostsInitialized)
        {
            printf("(%3u,%3u) ", tree->GetCostEx(), tree->GetCostSz());
        }
        else
        {
            printf("(???"
                   ",???"
                   ") "); // This probably indicates a bug: the node has a sequence number, but not costs.
        }
    }
    else
    {
        prev = tree;

        bool     hasSeqNum = true;
        unsigned dotNum    = 0;
        do
        {
            dotNum++;
            prev = prev->gtPrev;

            if ((prev == nullptr) || (prev == tree))
            {
                hasSeqNum = false;
                break;
            }

            assert(prev);
        } while (prev->gtSeqNum == 0);

        // If we have an indent stack, don't add additional characters,
        // as it will mess up the alignment.
        bool displayDotNum = hasSeqNum && (indentStack == nullptr);
        if (displayDotNum)
        {
            printf("N%03u.%02u ", prev->gtSeqNum, dotNum);
        }
        else
        {
            printf("     ");
        }

        if (tree->gtCostsInitialized)
        {
            printf("(%3u,%3u) ", tree->GetCostEx(), tree->GetCostSz());
        }
        else
        {
            if (displayDotNum)
            {
                // Do better alignment in this case
                printf("       ");
            }
            else
            {
                printf("          ");
            }
        }
    }

    if (optValnumCSE_phase)
    {
        if (IS_CSE_INDEX(tree->gtCSEnum))
        {
            printf("CSE #%02d (%s)", GET_CSE_INDEX(tree->gtCSEnum), (IS_CSE_USE(tree->gtCSEnum) ? "use" : "def"));
        }
        else
        {
            printf("             ");
        }
    }

    /* Print the node ID */
    printTreeID(tree);
    printf(" ");

    if (tree->gtOper >= GT_COUNT)
    {
        printf(" **** ILLEGAL NODE ****");
        return;
    }

    if (printFlags)
    {
        /* First print the flags associated with the node */
        switch (tree->gtOper)
        {
            case GT_LEA:
            case GT_BLK:
            case GT_OBJ:
            case GT_DYN_BLK:
            case GT_STORE_BLK:
            case GT_STORE_OBJ:
            case GT_STORE_DYN_BLK:

            case GT_IND:
                // We prefer printing V or U
                if ((tree->gtFlags & (GTF_IND_VOLATILE | GTF_IND_UNALIGNED)) == 0)
                {
                    if (tree->gtFlags & GTF_IND_TGTANYWHERE)
                    {
                        printf("*");
                        --msgLength;
                        break;
                    }
                    if (tree->gtFlags & GTF_IND_TGT_NOT_HEAP)
                    {
                        printf("s");
                        --msgLength;
                        break;
                    }
                    if (tree->gtFlags & GTF_IND_INVARIANT)
                    {
                        printf("#");
                        --msgLength;
                        break;
                    }
                    if (tree->gtFlags & GTF_IND_ARR_INDEX)
                    {
                        printf("a");
                        --msgLength;
                        break;
                    }
                    if (tree->gtFlags & GTF_IND_NONFAULTING)
                    {
                        printf("n"); // print a n for non-faulting
                        --msgLength;
                        break;
                    }
                    if (tree->gtFlags & GTF_IND_ASG_LHS)
                    {
                        printf("D"); // print a D for definition
                        --msgLength;
                        break;
                    }
                }
                FALLTHROUGH;

            case GT_INDEX:
            case GT_INDEX_ADDR:
            case GT_FIELD:
            case GT_CLS_VAR:
                if (tree->gtFlags & GTF_IND_VOLATILE)
                {
                    printf("V");
                    --msgLength;
                    break;
                }
                if (tree->gtFlags & GTF_IND_UNALIGNED)
                {
                    printf("U");
                    --msgLength;
                    break;
                }
                goto DASH;

            case GT_ASG:
                if (tree->OperIsInitBlkOp())
                {
                    printf("I");
                    --msgLength;
                    break;
                }
                goto DASH;

            case GT_CALL:
                if (tree->AsCall()->IsInlineCandidate())
                {
                    if (tree->AsCall()->IsGuardedDevirtualizationCandidate())
                    {
                        printf("&");
                    }
                    else
                    {
                        printf("I");
                    }
                    --msgLength;
                    break;
                }
                else if (tree->AsCall()->IsGuardedDevirtualizationCandidate())
                {
                    printf("G");
                    --msgLength;
                    break;
                }
                if (tree->AsCall()->gtCallMoreFlags & GTF_CALL_M_RETBUFFARG)
                {
                    printf("S");
                    --msgLength;
                    break;
                }
                if (tree->gtFlags & GTF_CALL_HOISTABLE)
                {
                    printf("H");
                    --msgLength;
                    break;
                }

                goto DASH;

            case GT_MUL:
#if !defined(TARGET_64BIT)
            case GT_MUL_LONG:
#endif
                if (tree->gtFlags & GTF_MUL_64RSLT)
                {
                    printf("L");
                    --msgLength;
                    break;
                }
                goto DASH;

            case GT_DIV:
            case GT_MOD:
            case GT_UDIV:
            case GT_UMOD:
                if (tree->gtFlags & GTF_DIV_BY_CNS_OPT)
                {
                    printf("M"); // We will use a Multiply by reciprical
                    --msgLength;
                    break;
                }
                goto DASH;

            case GT_LCL_FLD:
            case GT_LCL_VAR:
            case GT_LCL_VAR_ADDR:
            case GT_LCL_FLD_ADDR:
            case GT_STORE_LCL_FLD:
            case GT_STORE_LCL_VAR:
                if (tree->gtFlags & GTF_VAR_USEASG)
                {
                    printf("U");
                    --msgLength;
                    break;
                }
                if (tree->gtFlags & GTF_VAR_MULTIREG)
                {
                    printf((tree->gtFlags & GTF_VAR_DEF) ? "M" : "m");
                    --msgLength;
                    break;
                }
                if (tree->gtFlags & GTF_VAR_DEF)
                {
                    printf("D");
                    --msgLength;
                    break;
                }
                if (tree->gtFlags & GTF_VAR_CAST)
                {
                    printf("C");
                    --msgLength;
                    break;
                }
                if (tree->gtFlags & GTF_VAR_ARR_INDEX)
                {
                    printf("i");
                    --msgLength;
                    break;
                }
                if (tree->gtFlags & GTF_VAR_CONTEXT)
                {
                    printf("!");
                    --msgLength;
                    break;
                }

                goto DASH;

            case GT_EQ:
            case GT_NE:
            case GT_LT:
            case GT_LE:
            case GT_GE:
            case GT_GT:
            case GT_TEST_EQ:
            case GT_TEST_NE:
                if (tree->gtFlags & GTF_RELOP_NAN_UN)
                {
                    printf("N");
                    --msgLength;
                    break;
                }
                if (tree->gtFlags & GTF_RELOP_JMP_USED)
                {
                    printf("J");
                    --msgLength;
                    break;
                }
                if (tree->gtFlags & GTF_RELOP_QMARK)
                {
                    printf("Q");
                    --msgLength;
                    break;
                }
                goto DASH;

            case GT_JCMP:
                printf((tree->gtFlags & GTF_JCMP_TST) ? "T" : "C");
                printf((tree->gtFlags & GTF_JCMP_EQ) ? "EQ" : "NE");
                goto DASH;

            case GT_CNS_INT:
                if (tree->IsIconHandle())
                {
                    if ((tree->gtFlags & GTF_ICON_INITCLASS) != 0)
                    {
                        printf("I"); // Static Field handle with INITCLASS requirement
                        --msgLength;
                        break;
                    }
                    else if ((tree->gtFlags & GTF_ICON_FIELD_OFF) != 0)
                    {
                        printf("O");
                        --msgLength;
                        break;
                    }
                    else
                    {
                        // Some other handle
                        printf("H");
                        --msgLength;
                        break;
                    }
                }
                goto DASH;

            default:
            DASH:
                printf("-");
                --msgLength;
                break;
        }

        /* Then print the general purpose flags */
        unsigned flags = tree->gtFlags;

        if (tree->OperIsBinary())
        {
            genTreeOps oper = tree->OperGet();

            // Check for GTF_ADDRMODE_NO_CSE flag on add/mul/shl Binary Operators
            if ((oper == GT_ADD) || (oper == GT_MUL) || (oper == GT_LSH))
            {
                if ((tree->gtFlags & GTF_ADDRMODE_NO_CSE) != 0)
                {
                    flags |= GTF_DONT_CSE; // Force the GTF_ADDRMODE_NO_CSE flag to print out like GTF_DONT_CSE
                }
            }
        }
        else // !tree->OperIsBinary()
        {
            // the GTF_REVERSE flag only applies to binary operations
            flags &= ~GTF_REVERSE_OPS; // we use this value for GTF_VAR_ARR_INDEX above
        }

        msgLength -= GenTree::gtDispFlags(flags, tree->gtDebugFlags);
        /*
            printf("%c", (flags & GTF_ASG           ) ? 'A' : '-');
            printf("%c", (flags & GTF_CALL          ) ? 'C' : '-');
            printf("%c", (flags & GTF_EXCEPT        ) ? 'X' : '-');
            printf("%c", (flags & GTF_GLOB_REF      ) ? 'G' : '-');
            printf("%c", (flags & GTF_ORDER_SIDEEFF ) ? 'O' : '-');
            printf("%c", (flags & GTF_COLON_COND    ) ? '?' : '-');
            printf("%c", (flags & GTF_DONT_CSE      ) ? 'N' :        // N is for No cse
                         (flags & GTF_MAKE_CSE      ) ? 'H' : '-');  // H is for Hoist this expr
            printf("%c", (flags & GTF_REVERSE_OPS   ) ? 'R' : '-');
            printf("%c", (flags & GTF_UNSIGNED      ) ? 'U' :
                         (flags & GTF_BOOLEAN       ) ? 'B' : '-');
            printf("%c", (flags & GTF_SET_FLAGS     ) ? 'S' : '-');
            printf("%c", (flags & GTF_SPILLED       ) ? 'z' : '-');
            printf("%c", (flags & GTF_SPILL         ) ? 'Z' : '-');
        */
    }

    // If we're printing a node for LIR, we use the space normally associated with the message
    // to display the node's temp name (if any)
    const bool hasOperands = tree->OperandsBegin() != tree->OperandsEnd();
    if (isLIR)
    {
        assert(msg == nullptr);

        // If the tree does not have any operands, we do not display the indent stack. This gives us
        // two additional characters for alignment.
        if (!hasOperands)
        {
            msgLength += 1;
        }

        if (tree->IsValue())
        {
            const size_t bufLength = msgLength - 1;
            msg                    = reinterpret_cast<char*>(alloca(bufLength * sizeof(char)));
            sprintf_s(const_cast<char*>(msg), bufLength, "t%d = %s", tree->gtTreeID, hasOperands ? "" : " ");
        }
    }

    /* print the msg associated with the node */

    if (msg == nullptr)
    {
        msg = "";
    }
    if (msgLength < 0)
    {
        msgLength = 0;
    }

    printf(isLIR ? " %+*s" : " %-*s", msgLength, msg);

    /* Indent the node accordingly */
    if (!isLIR || hasOperands)
    {
        printIndent(indentStack);
    }

    gtDispNodeName(tree);

    assert(tree == nullptr || tree->gtOper < GT_COUNT);

    if (tree)
    {
        /* print the type of the node */
        if (tree->gtOper != GT_CAST)
        {
            printf(" %-6s", varTypeName(tree->TypeGet()));

            if (varTypeIsStruct(tree->TypeGet()))
            {
                ClassLayout* layout = nullptr;

                if (tree->OperIs(GT_BLK, GT_OBJ, GT_STORE_BLK, GT_STORE_OBJ))
                {
                    layout = tree->AsBlk()->GetLayout();
                }
                else if (tree->OperIs(GT_LCL_VAR, GT_STORE_LCL_VAR))
                {
                    LclVarDsc* varDsc = lvaGetDesc(tree->AsLclVar());

                    if (varTypeIsStruct(varDsc->TypeGet()))
                    {
                        layout = varDsc->GetLayout();
                    }
                }

                if (layout != nullptr)
                {
                    gtDispClassLayout(layout, tree->TypeGet());
                }
            }

            if (tree->gtOper == GT_LCL_VAR || tree->gtOper == GT_STORE_LCL_VAR)
            {
                LclVarDsc* varDsc = &lvaTable[tree->AsLclVarCommon()->GetLclNum()];
                if (varDsc->lvAddrExposed)
                {
                    printf("(AX)"); // Variable has address exposed.
                }

                if (varDsc->lvUnusedStruct)
                {
                    assert(varDsc->lvPromoted);
                    printf("(U)"); // Unused struct
                }
                else if (varDsc->lvPromoted)
                {
                    if (varTypeIsPromotable(varDsc))
                    {
                        printf("(P)"); // Promoted struct
                    }
                    else
                    {
                        // Promoted implicit by-refs can have this state during
                        // global morph while they are being rewritten
                        assert(fgGlobalMorph);
                        printf("(P?!)"); // Promoted struct
                    }
                }
            }

            if (tree->IsArgPlaceHolderNode() && (tree->AsArgPlace()->gtArgPlaceClsHnd != nullptr))
            {
                printf(" => [clsHnd=%08X]", dspPtr(tree->AsArgPlace()->gtArgPlaceClsHnd));
            }

            if (tree->gtOper == GT_RUNTIMELOOKUP)
            {
#ifdef TARGET_64BIT
                printf(" 0x%llx", dspPtr(tree->AsRuntimeLookup()->gtHnd));
#else
                printf(" 0x%x", dspPtr(tree->AsRuntimeLookup()->gtHnd));
#endif

                switch (tree->AsRuntimeLookup()->gtHndType)
                {
                    case CORINFO_HANDLETYPE_CLASS:
                        printf(" class");
                        break;
                    case CORINFO_HANDLETYPE_METHOD:
                        printf(" method");
                        break;
                    case CORINFO_HANDLETYPE_FIELD:
                        printf(" field");
                        break;
                    default:
                        printf(" unknown");
                        break;
                }
            }
        }

        // for tracking down problems in reguse prediction or liveness tracking

        if (verbose && 0)
        {
            printf(" RR=");
            dspRegMask(tree->gtRsvdRegs);
            printf("\n");
        }
    }
}

void Compiler::gtDispNodeName(GenTree* tree)
{
    /* print the node name */

    const char* name;

    assert(tree);
    if (tree->gtOper < GT_COUNT)
    {
        name = GenTree::OpName(tree->OperGet());
    }
    else
    {
        name = "<ERROR>";
    }
    char  buf[32];
    char* bufp = &buf[0];

    if ((tree->gtOper == GT_CNS_INT) && tree->IsIconHandle())
    {
        sprintf_s(bufp, sizeof(buf), " %s(h)%c", name, 0);
    }
    else if (tree->gtOper == GT_PUTARG_STK)
    {
        sprintf_s(bufp, sizeof(buf), " %s [+0x%02x]%c", name, tree->AsPutArgStk()->getArgOffset(), 0);
    }
    else if (tree->gtOper == GT_CALL)
    {
        const char* callType = "CALL";
        const char* gtfType  = "";
        const char* ctType   = "";
        char        gtfTypeBuf[100];

        if (tree->AsCall()->gtCallType == CT_USER_FUNC)
        {
            if (tree->AsCall()->IsVirtual())
            {
                callType = "CALLV";
            }
        }
        else if (tree->AsCall()->gtCallType == CT_HELPER)
        {
            ctType = " help";
        }
        else if (tree->AsCall()->gtCallType == CT_INDIRECT)
        {
            ctType = " ind";
        }
        else
        {
            assert(!"Unknown gtCallType");
        }

        if (tree->gtFlags & GTF_CALL_NULLCHECK)
        {
            gtfType = " nullcheck";
        }
        if (tree->AsCall()->IsVirtualVtable())
        {
            gtfType = " ind";
        }
        else if (tree->AsCall()->IsVirtualStub())
        {
            gtfType = " stub";
        }
#ifdef FEATURE_READYTORUN_COMPILER
        else if (tree->AsCall()->IsR2RRelativeIndir())
        {
            gtfType = " r2r_ind";
        }
#endif // FEATURE_READYTORUN_COMPILER
        else if (tree->gtFlags & GTF_CALL_UNMANAGED)
        {
            char* gtfTypeBufWalk = gtfTypeBuf;
            gtfTypeBufWalk += SimpleSprintf_s(gtfTypeBufWalk, gtfTypeBuf, sizeof(gtfTypeBuf), " unman");
            if (tree->gtFlags & GTF_CALL_POP_ARGS)
            {
                gtfTypeBufWalk += SimpleSprintf_s(gtfTypeBufWalk, gtfTypeBuf, sizeof(gtfTypeBuf), " popargs");
            }
            if (tree->AsCall()->gtCallMoreFlags & GTF_CALL_M_UNMGD_THISCALL)
            {
                gtfTypeBufWalk += SimpleSprintf_s(gtfTypeBufWalk, gtfTypeBuf, sizeof(gtfTypeBuf), " thiscall");
            }
            gtfType = gtfTypeBuf;
        }

        sprintf_s(bufp, sizeof(buf), " %s%s%s%c", callType, ctType, gtfType, 0);
    }
    else if (tree->gtOper == GT_ARR_ELEM)
    {
        bufp += SimpleSprintf_s(bufp, buf, sizeof(buf), " %s[", name);
        for (unsigned rank = tree->AsArrElem()->gtArrRank - 1; rank; rank--)
        {
            bufp += SimpleSprintf_s(bufp, buf, sizeof(buf), ",");
        }
        SimpleSprintf_s(bufp, buf, sizeof(buf), "]");
    }
    else if (tree->gtOper == GT_ARR_OFFSET || tree->gtOper == GT_ARR_INDEX)
    {
        bufp += SimpleSprintf_s(bufp, buf, sizeof(buf), " %s[", name);
        unsigned char currDim;
        unsigned char rank;
        if (tree->gtOper == GT_ARR_OFFSET)
        {
            currDim = tree->AsArrOffs()->gtCurrDim;
            rank    = tree->AsArrOffs()->gtArrRank;
        }
        else
        {
            currDim = tree->AsArrIndex()->gtCurrDim;
            rank    = tree->AsArrIndex()->gtArrRank;
        }

        for (unsigned char dim = 0; dim < rank; dim++)
        {
            // Use a defacto standard i,j,k for the dimensions.
            // Note that we only support up to rank 3 arrays with these nodes, so we won't run out of characters.
            char dimChar = '*';
            if (dim == currDim)
            {
                dimChar = 'i' + dim;
            }
            else if (dim > currDim)
            {
                dimChar = ' ';
            }

            bufp += SimpleSprintf_s(bufp, buf, sizeof(buf), "%c", dimChar);
            if (dim != rank - 1)
            {
                bufp += SimpleSprintf_s(bufp, buf, sizeof(buf), ",");
            }
        }
        SimpleSprintf_s(bufp, buf, sizeof(buf), "]");
    }
    else if (tree->gtOper == GT_LEA)
    {
        GenTreeAddrMode* lea = tree->AsAddrMode();
        bufp += SimpleSprintf_s(bufp, buf, sizeof(buf), " %s(", name);
        if (lea->Base() != nullptr)
        {
            bufp += SimpleSprintf_s(bufp, buf, sizeof(buf), "b+");
        }
        if (lea->Index() != nullptr)
        {
            bufp += SimpleSprintf_s(bufp, buf, sizeof(buf), "(i*%d)+", lea->gtScale);
        }
        bufp += SimpleSprintf_s(bufp, buf, sizeof(buf), "%d)", lea->Offset());
    }
    else if (tree->gtOper == GT_ARR_BOUNDS_CHECK)
    {
        switch (tree->AsBoundsChk()->gtThrowKind)
        {
            case SCK_RNGCHK_FAIL:
                sprintf_s(bufp, sizeof(buf), " %s_Rng", name);
                break;
            case SCK_ARG_EXCPN:
                sprintf_s(bufp, sizeof(buf), " %s_Arg", name);
                break;
            case SCK_ARG_RNG_EXCPN:
                sprintf_s(bufp, sizeof(buf), " %s_ArgRng", name);
                break;
            default:
                unreached();
        }
    }
    else if (tree->gtOverflowEx())
    {
        sprintf_s(bufp, sizeof(buf), " %s_ovfl%c", name, 0);
    }
    else
    {
        sprintf_s(bufp, sizeof(buf), " %s%c", name, 0);
    }

    if (strlen(buf) < 10)
    {
        printf(" %-10s", buf);
    }
    else
    {
        printf(" %s", buf);
    }
}

void Compiler::gtDispConst(GenTree* tree)
{
    assert(tree->OperKind() & GTK_CONST);

    switch (tree->gtOper)
    {
        case GT_CNS_INT:
            if (tree->IsIconHandle(GTF_ICON_STR_HDL))
            {
                const WCHAR* str = eeGetCPString(tree->AsIntCon()->gtIconVal);
                // If *str points to a '\0' then don't print the string's values
                if ((str != nullptr) && (*str != '\0'))
                {
                    printf(" 0x%X \"%S\"", dspPtr(tree->AsIntCon()->gtIconVal), str);
                }
                else // We can't print the value of the string
                {
                    // Note that eeGetCPString isn't currently implemented on Linux/ARM
                    // and instead always returns nullptr
                    printf(" 0x%X [ICON_STR_HDL]", dspPtr(tree->AsIntCon()->gtIconVal));
                }
            }
            else
            {
                ssize_t dspIconVal =
                    tree->IsIconHandle() ? dspPtr(tree->AsIntCon()->gtIconVal) : tree->AsIntCon()->gtIconVal;

                if (tree->TypeGet() == TYP_REF)
                {
                    assert(tree->AsIntCon()->gtIconVal == 0);
                    printf(" null");
                }
                else if ((tree->AsIntCon()->gtIconVal > -1000) && (tree->AsIntCon()->gtIconVal < 1000))
                {
                    printf(" %ld", dspIconVal);
                }
#ifdef TARGET_64BIT
                else if ((tree->AsIntCon()->gtIconVal & 0xFFFFFFFF00000000LL) != 0)
                {
                    if (dspIconVal >= 0)
                    {
                        printf(" 0x%llx", dspIconVal);
                    }
                    else
                    {
                        printf(" -0x%llx", -dspIconVal);
                    }
                }
#endif
                else
                {
                    if (dspIconVal >= 0)
                    {
                        printf(" 0x%X", dspIconVal);
                    }
                    else
                    {
                        printf(" -0x%X", -dspIconVal);
                    }
                }

                if (tree->IsIconHandle())
                {
                    switch (tree->GetIconHandleFlag())
                    {
                        case GTF_ICON_SCOPE_HDL:
                            printf(" scope");
                            break;
                        case GTF_ICON_CLASS_HDL:
                            printf(" class");
                            break;
                        case GTF_ICON_METHOD_HDL:
                            printf(" method");
                            break;
                        case GTF_ICON_FIELD_HDL:
                            printf(" field");
                            break;
                        case GTF_ICON_STATIC_HDL:
                            printf(" static");
                            break;
                        case GTF_ICON_STR_HDL:
                            unreached(); // This case is handled above
                            break;
                        case GTF_ICON_CONST_PTR:
                            printf(" const ptr");
                            break;
                        case GTF_ICON_GLOBAL_PTR:
                            printf(" global ptr");
                            break;
                        case GTF_ICON_VARG_HDL:
                            printf(" vararg");
                            break;
                        case GTF_ICON_PINVKI_HDL:
                            printf(" pinvoke");
                            break;
                        case GTF_ICON_TOKEN_HDL:
                            printf(" token");
                            break;
                        case GTF_ICON_TLS_HDL:
                            printf(" tls");
                            break;
                        case GTF_ICON_FTN_ADDR:
                            printf(" ftn");
                            break;
                        case GTF_ICON_CIDMID_HDL:
                            printf(" cid/mid");
                            break;
                        case GTF_ICON_BBC_PTR:
                            printf(" bbc");
                            break;
                        default:
                            printf(" UNKNOWN");
                            break;
                    }
                }

                if ((tree->gtFlags & GTF_ICON_FIELD_OFF) != 0)
                {
                    printf(" field offset");
                }

#ifdef FEATURE_SIMD
                if ((tree->gtFlags & GTF_ICON_SIMD_COUNT) != 0)
                {
                    printf(" vector element count");
                }
#endif

                if ((tree->IsReuseRegVal()) != 0)
                {
                    printf(" reuse reg val");
                }
            }

            gtDispFieldSeq(tree->AsIntCon()->gtFieldSeq);

            break;

        case GT_CNS_LNG:
            printf(" 0x%016I64x", tree->AsLngCon()->gtLconVal);
            break;

        case GT_CNS_DBL:
            if (*((__int64*)&tree->AsDblCon()->gtDconVal) == (__int64)I64(0x8000000000000000))
            {
                printf(" -0.00000");
            }
            else
            {
                printf(" %#.17g", tree->AsDblCon()->gtDconVal);
            }
            break;
        case GT_CNS_STR:
            printf("<string constant>");
            break;
        default:
            assert(!"unexpected constant node");
    }
}