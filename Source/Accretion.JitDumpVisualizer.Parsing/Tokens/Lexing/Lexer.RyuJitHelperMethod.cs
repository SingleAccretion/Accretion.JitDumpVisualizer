using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        public static RyuJitHelperMethod ParseRyuJitHelperMethod(char* start, out int width)
        {
            // We have to use another method so that the Jit inlines and folds all Result() calls
            static ulong ParseRyuJitHelperMethod(char* start) => (*start) switch
            {
                'I' => start[4] switch
                {
                    'C' => Result(RyuJitHelperMethod.INITCLASS, "INITCLASS", start),
                    'I' => Result(RyuJitHelperMethod.INITINSTCLASS, "INITINSTCLASS", start),
                    '_' => Result(RyuJitHelperMethod.INIT_PINVOKE_FRAME, "INIT_PINVOKE_FRAME", start),
                    _ => start[14] switch
                    {
                        'Y' => Result(RyuJitHelperMethod.ISINSTANCEOFANY, "ISINSTANCEOFANY", start),
                        'R' => Result(RyuJitHelperMethod.ISINSTANCEOFARRAY, "ISINSTANCEOFARRAY", start),
                        'A' => Result(RyuJitHelperMethod.ISINSTANCEOFCLASS, "ISINSTANCEOFCLASS", start),
                        _ => Result(RyuJitHelperMethod.ISINSTANCEOFINTERFACE, "ISINSTANCEOFINTERFACE", start)
                    }
                },
                'J' => start[12] switch
                {
                    'B' => Result(RyuJitHelperMethod.JIT_PINVOKE_BEGIN, "JIT_PINVOKE_BEGIN", start),
                    'E' => Result(RyuJitHelperMethod.JIT_PINVOKE_END, "JIT_PINVOKE_END", start),
                    _ => start[21] switch
                    {
                        'N' => Result(RyuJitHelperMethod.JIT_REVERSE_PINVOKE_ENTER, "JIT_REVERSE_PINVOKE_ENTER", start),
                        _ => Result(RyuJitHelperMethod.JIT_REVERSE_PINVOKE_EXIT, "JIT_REVERSE_PINVOKE_EXIT", start)
                    }
                },
                'L' => start[3] switch
                {
                    'L' => start[4] switch
                    {
                        'E' => Result(RyuJitHelperMethod.LDELEMA_REF, "LDELEMA_REF", start),
                        '_' => Result(RyuJitHelperMethod.LMUL_OVF, "LMUL_OVF", start),
                        _ => Result(RyuJitHelperMethod.LMUL, "LMUL", start)
                    },
                    'V' => Result(RyuJitHelperMethod.LDIV, "LDIV", start),
                    'H' => start[1] switch
                    {
                        'L' => Result(RyuJitHelperMethod.LLSH, "LLSH", start),
                        _ => Result(RyuJitHelperMethod.LRSH, "LRSH", start)
                    },
                    'D' => Result(RyuJitHelperMethod.LMOD, "LMOD", start),
                    '2' => Result(RyuJitHelperMethod.LNG2DBL, "LNG2DBL", start),
                    'P' => Result(RyuJitHelperMethod.LOOP_CLONE_CHOICE_ADDR, "LOOP_CLONE_CHOICE_ADDR", start),
                    _ => Result(RyuJitHelperMethod.LRSZ, "LRSZ", start)
                },
                'M' => start[3] switch
                {
                    'C' => Result(RyuJitHelperMethod.MEMCPY, "MEMCPY", start),
                    'S' => Result(RyuJitHelperMethod.MEMSET, "MEMSET", start),
                    'H' => start[6] switch
                    {
                        'D' => Result(RyuJitHelperMethod.METHODDESC_TO_STUBRUNTIMEMETHOD, "METHODDESC_TO_STUBRUNTIMEMETHOD", start),
                        _ => Result(RyuJitHelperMethod.METHOD_ACCESS_EXCEPTION, "METHOD_ACCESS_EXCEPTION", start)
                    },
                    '_' => start[8] switch
                    {
                        'R' => start[9] switch
                        {
                            '_' => Result(RyuJitHelperMethod.MON_ENTER_STATIC, "MON_ENTER_STATIC", start),
                            _ => Result(RyuJitHelperMethod.MON_ENTER, "MON_ENTER", start)
                        },
                        '_' => Result(RyuJitHelperMethod.MON_EXIT_STATIC, "MON_EXIT_STATIC", start),
                        _ => Result(RyuJitHelperMethod.MON_EXIT, "MON_EXIT", start)
                    },
                    _ => Result(RyuJitHelperMethod.MOD, "MOD", start)
                },
                'N' => start[3] switch
                {
                    'A' => start[9] switch
                    {
                        'A' => Result(RyuJitHelperMethod.NEWARR_1_ALIGN8, "NEWARR_1_ALIGN8", start),
                        'D' => Result(RyuJitHelperMethod.NEWARR_1_DIRECT, "NEWARR_1_DIRECT", start),
                        'O' => Result(RyuJitHelperMethod.NEWARR_1_OBJ, "NEWARR_1_OBJ", start),
                        _ => Result(RyuJitHelperMethod.NEWARR_1_VC, "NEWARR_1_VC", start)
                    },
                    'F' => Result(RyuJitHelperMethod.NEWFAST, "NEWFAST", start),
                    'S' => start[8] switch
                    {
                        '_' => start[15] switch
                        {
                            '_' => start[16] switch
                            {
                                'F' => Result(RyuJitHelperMethod.NEWSFAST_ALIGN8_FINALIZE, "NEWSFAST_ALIGN8_FINALIZE", start),
                                _ => Result(RyuJitHelperMethod.NEWSFAST_ALIGN8_VC, "NEWSFAST_ALIGN8_VC", start)
                            },
                            'Z' => Result(RyuJitHelperMethod.NEWSFAST_FINALIZE, "NEWSFAST_FINALIZE", start),
                            _ => Result(RyuJitHelperMethod.NEWSFAST_ALIGN8, "NEWSFAST_ALIGN8", start),
                        },
                        _ => Result(RyuJitHelperMethod.NEWSFAST, "NEWSFAST", start)
                    },
                    _ => start[9] switch
                    {
                        '_' => Result(RyuJitHelperMethod.NEW_MDARR_NONVARARG, "NEW_MDARR_NONVARARG", start),
                        _ => Result(RyuJitHelperMethod.NEW_MDARR, "NEW_MDARR", start)
                    }
                },
                'O' => Result(RyuJitHelperMethod.OVERFLOW, "OVERFLOW", start),
                'P' => start[1] switch
                {
                    'A' => Result(RyuJitHelperMethod.PATCHPOINT, "PATCHPOINT", start),
                    'I' => Result(RyuJitHelperMethod.PINVOKE_CALLI, "PINVOKE_CALLI", start),
                    'O' => Result(RyuJitHelperMethod.POLL_GC, "POLL_GC", start),
                    _ => start[9] switch
                    {
                        'E' => Result(RyuJitHelperMethod.PROF_FCN_ENTER, "PROF_FCN_ENTER", start),
                        'L' => Result(RyuJitHelperMethod.PROF_FCN_LEAVE, "PROF_FCN_LEAVE", start),
                        _ => Result(RyuJitHelperMethod.PROF_FCN_TAILCALL, "PROF_FCN_TAILCALL", start)
                    }
                },
                'R' => start[2] switch
                {
                    'A' => start[13] switch
                    {
                        'K' => Result(RyuJitHelperMethod.READYTORUN_CHKCAST, "READYTORUN_CHKCAST", start),
                        'L' => Result(RyuJitHelperMethod.READYTORUN_DELEGATE_CTOR, "READYTORUN_DELEGATE_CTOR", start),
                        'N' => start[19] switch
                        {
                            'H' => Result(RyuJitHelperMethod.READYTORUN_GENERIC_HANDLE, "READYTORUN_GENERIC_HANDLE", start),
                            _ => Result(RyuJitHelperMethod.READYTORUN_GENERIC_STATIC_BASE, "READYTORUN_GENERIC_STATIC_BASE", start)
                        },
                        'I' => Result(RyuJitHelperMethod.READYTORUN_ISINSTANCEOF, "READYTORUN_ISINSTANCEOF", start),
                        'W' => start[14] switch
                        {
                            'A' => Result(RyuJitHelperMethod.READYTORUN_NEWARR_1, "READYTORUN_NEWARR_1", start),
                            _ => Result(RyuJitHelperMethod.READYTORUN_NEW, "READYTORUN_NEW", start),
                        },
                        'A' => Result(RyuJitHelperMethod.READYTORUN_STATIC_BASE, "READYTORUN_STATIC_BASE", start),
                        _ => Result(RyuJitHelperMethod.READYTORUN_VIRTUAL_FUNC_PTR, "READYTORUN_VIRTUAL_FUNC_PTR", start)
                    },
                    'T' => Result(RyuJitHelperMethod.RETHROW, "RETHROW", start),
                    'G' => Result(RyuJitHelperMethod.RNGCHKFAIL, "RNGCHKFAIL", start),
                    _ => start[19] switch
                    {
                        'D' => start[20] switch
                        {
                            '_' => Result(RyuJitHelperMethod.RUNTIMEHANDLE_METHOD_LOG, "RUNTIMEHANDLE_METHOD_LOG", start),
                            _ => Result(RyuJitHelperMethod.RUNTIMEHANDLE_METHOD, "RUNTIMEHANDLE_METHOD", start)
                        },
                        '_' => Result(RyuJitHelperMethod.RUNTIMEHANDLE_CLASS_LOG, "RUNTIMEHANDLE_CLASS_LOG", start),
                        _ => Result(RyuJitHelperMethod.RUNTIMEHANDLE_CLASS, "RUNTIMEHANDLE_CLASS", start)
                    }
                },
                'S' => start[4] switch
                {
                    'I' => start[8] switch
                    {
                        '1' => Result(RyuJitHelperMethod.SETFIELD16, "SETFIELD16", start),
                        '3' => Result(RyuJitHelperMethod.SETFIELD32, "SETFIELD32", start),
                        '6' => Result(RyuJitHelperMethod.SETFIELD64, "SETFIELD64", start),
                        '8' => Result(RyuJitHelperMethod.SETFIELD8, "SETFIELD8", start),
                        'D' => Result(RyuJitHelperMethod.SETFIELDDOUBLE, "SETFIELDDOUBLE", start),
                        'F' => Result(RyuJitHelperMethod.SETFIELDFLOAT, "SETFIELDFLOAT", start),
                        'O' => Result(RyuJitHelperMethod.SETFIELDOBJ, "SETFIELDOBJ", start),
                        _ => Result(RyuJitHelperMethod.SETFIELDSTRUCT, "SETFIELDSTRUCT", start)
                    },
                    'K' => Result(RyuJitHelperMethod.STACK_PROBE, "STACK_PROBE", start),
                    '_' => Result(RyuJitHelperMethod.STOP_FOR_GC, "STOP_FOR_GC", start),
                    'N' => start[6] switch
                    {
                        '_' => Result(RyuJitHelperMethod.STRCNS_CURRENT_MODULE, "STRCNS_CURRENT_MODULE", start),
                        _ => Result(RyuJitHelperMethod.STRCNS, "STRCNS", start)
                    },
                    _ => Result(RyuJitHelperMethod.STRESS_GC, "STRESS_GC", start)
                },
                'T' => start[1] switch
                {
                    'A' => Result(RyuJitHelperMethod.TAILCALL, "TAILCALL", start),
                    'H' => start[5] switch
                    {
                        'D' => Result(RyuJitHelperMethod.THROWDIVZERO, "THROWDIVZERO", start),
                        'N' => Result(RyuJitHelperMethod.THROWNULLREF, "THROWNULLREF", start),
                        '_' => start[15] switch
                        {
                            'X' => Result(RyuJitHelperMethod.THROW_ARGUMENTEXCEPTION, "THROW_ARGUMENTEXCEPTION", start),
                            'U' => Result(RyuJitHelperMethod.THROW_ARGUMENTOUTOFRANGEEXCEPTION, "THROW_ARGUMENTOUTOFRANGEEXCEPTION", start),
                            'M' => Result(RyuJitHelperMethod.THROW_NOT_IMPLEMENTED, "THROW_NOT_IMPLEMENTED", start),
                            'N' => Result(RyuJitHelperMethod.THROW_PLATFORM_NOT_SUPPORTED, "THROW_PLATFORM_NOT_SUPPORTED", start),
                            _ => Result(RyuJitHelperMethod.THROW_TYPE_NOT_SUPPORTED, "THROW_TYPE_NOT_SUPPORTED", start)
                        },

                        _ => Result(RyuJitHelperMethod.THROW, "THROW", start)
                    },
                    _ => start[25] switch
                    {
                        'H' => start[31] switch
                        {
                            '_' => Result(RyuJitHelperMethod.TYPEHANDLE_TO_RUNTIMETYPEHANDLE_MAYBENULL, "TYPEHANDLE_TO_RUNTIMETYPEHANDLE_MAYBENULL", start),
                            _ => Result(RyuJitHelperMethod.TYPEHANDLE_TO_RUNTIMETYPEHANDLE, "TYPEHANDLE_TO_RUNTIMETYPEHANDLE", start)
                        },
                        '_' => Result(RyuJitHelperMethod.TYPEHANDLE_TO_RUNTIMETYPE_MAYBENULL, "TYPEHANDLE_TO_RUNTIMETYPE_MAYBENULL", start),
                        _ => Result(RyuJitHelperMethod.TYPEHANDLE_TO_RUNTIMETYPE, "TYPEHANDLE_TO_RUNTIMETYPE", start)
                    },
                },
                'U' => start[3] switch
                {
                    'V' => Result(RyuJitHelperMethod.UDIV, "UDIV", start),
                    'I' => Result(RyuJitHelperMethod.ULDIV, "ULDIV", start),
                    'U' => Result(RyuJitHelperMethod.ULMUL_OVF, "ULMUL_OVF", start),
                    'O' => start[4] switch
                    {
                        'D' => Result(RyuJitHelperMethod.ULMOD, "ULMOD", start),
                        _ => start[5] switch
                        {
                            '_' => Result(RyuJitHelperMethod.UNBOX_NULLABLE, "UNBOX_NULLABLE", start),
                            _ => Result(RyuJitHelperMethod.UNBOX, "UNBOX", start)
                        }
                    },
                    'G' => Result(RyuJitHelperMethod.ULNG2DBL, "ULNG2DBL", start),
                    'D' => Result(RyuJitHelperMethod.UMOD, "UMOD", start),
                    'E' => Result(RyuJitHelperMethod.UNDEF, "UNDEF", start),
                    _ => Result(RyuJitHelperMethod.USER_BREAKPOINT, "USER_BREAKPOINT", start)
                },
                _ => start[1] switch
                {
                    'E' => Result(RyuJitHelperMethod.VERIFICATION, "VERIFICATION", start),
                    _ => Result(RyuJitHelperMethod.VIRTUAL_FUNC_PTR, "VIRTUAL_FUNC_PTR", start)
                }
            };

            Assert.True(Unsafe.SizeOf<RyuJitHelperMethod>() is 1);

            Assert.Equal(start, "HELPER.CORINFO_HELP_");
            start += 20;
            var result = (start[0]) switch
            {
                'A' => start[7] switch
                {
                    'E' => Result(RyuJitHelperMethod.ARE_TYPES_EQUIVALENT, "ARE_TYPES_EQUIVALENT", start),
                    '_' => Result(RyuJitHelperMethod.ARRADDR_ST, "ARRADDR_ST", start),
                    'B' => Result(RyuJitHelperMethod.ASSIGN_BYREF, "ASSIGN_BYREF", start),
                    'R' => start[10] switch
                    {
                        '_' => start[12] switch
                        {
                            'A' => Result(RyuJitHelperMethod.ASSIGN_REF_EAX, "ASSIGN_REF_EAX", start),
                            'B' => start[13] switch
                            {
                                'P' => Result(RyuJitHelperMethod.ASSIGN_REF_EBP, "ASSIGN_REF_EBP", start),
                                _ => Result(RyuJitHelperMethod.ASSIGN_REF_EBX, "ASSIGN_REF_EBX", start)
                            },
                            'C' => Result(RyuJitHelperMethod.ASSIGN_REF_ECX, "ASSIGN_REF_ECX", start),
                            'D' => Result(RyuJitHelperMethod.ASSIGN_REF_EDI, "ASSIGN_REF_EDI", start),
                            'N' => Result(RyuJitHelperMethod.ASSIGN_REF_ENSURE_NONHEAP, "ASSIGN_REF_ENSURE_NONHEAP", start),
                            _ => Result(RyuJitHelperMethod.ASSIGN_REF_ESI, "ASSIGN_REF_ESI", start)
                        },
                        _ => Result(RyuJitHelperMethod.ASSIGN_REF, "ASSIGN_REF", start)
                    },
                    _ => Result(RyuJitHelperMethod.ASSIGN_STRUCT, "ASSIGN_STRUCT", start)
                },
                'B' => start[2] switch
                {
                    'T' => Result(RyuJitHelperMethod.BBT_FCN_ENTER, "BBT_FCN_ENTER", start),
                    _ => start[3] switch
                    {
                        '_' => Result(RyuJitHelperMethod.BOX_NULLABLE, "BOX_NULLABLE", start),
                        _ => Result(RyuJitHelperMethod.BOX, "BOX", start)
                    }
                },
                'C' => start[6] switch
                {
                    'D' => start[18] switch
                    {
                        '_' => start[20] switch
                        {
                            'A' => Result(RyuJitHelperMethod.CHECKED_ASSIGN_REF_EAX, "CHECKED_ASSIGN_REF_EAX", start),
                            'B' => start[21] switch
                            {
                                'P' => Result(RyuJitHelperMethod.CHECKED_ASSIGN_REF_EBP, "CHECKED_ASSIGN_REF_EBP", start),
                                _ => Result(RyuJitHelperMethod.CHECKED_ASSIGN_REF_EBX, "CHECKED_ASSIGN_REF_EBX", start),
                            },
                            'C' => Result(RyuJitHelperMethod.CHECKED_ASSIGN_REF_ECX, "CHECKED_ASSIGN_REF_ECX", start),
                            'D' => Result(RyuJitHelperMethod.CHECKED_ASSIGN_REF_EDI, "CHECKED_ASSIGN_REF_EDI", start),
                            _ => Result(RyuJitHelperMethod.CHECKED_ASSIGN_REF_ESI, "CHECKED_ASSIGN_REF_ESI", start)

                        },
                        _ => Result(RyuJitHelperMethod.CHECKED_ASSIGN_REF, "CHECKED_ASSIGN_REF", start)
                    },
                    'O' => Result(RyuJitHelperMethod.CHECK_OBJ, "CHECK_OBJ", start),
                    'T' => start[9] switch
                    {
                        'Y' => Result(RyuJitHelperMethod.CHKCASTANY, "CHKCASTANY", start),
                        'R' => Result(RyuJitHelperMethod.CHKCASTARRAY, "CHKCASTARRAY", start),
                        'A' => start[12] switch
                        {
                            '_' => Result(RyuJitHelperMethod.CHKCASTCLASS_SPECIAL, "CHKCASTCLASS_SPECIAL", start),
                            _ => Result(RyuJitHelperMethod.CHKCASTCLASS, "CHKCASTCLASS", start)
                        },
                        _ => Result(RyuJitHelperMethod.CHKCASTINTERFACE, "CHKCASTINTERFACE", start)
                    },
                    'N' => Result(RyuJitHelperMethod.CLASSINIT_SHARED_DYNAMICCLASS, "CLASSINIT_SHARED_DYNAMICCLASS", start),
                    'R' => Result(RyuJitHelperMethod.CLASSPROFILE, "CLASSPROFILE", start),
                    _ => Result(RyuJitHelperMethod.CLASS_ACCESS_EXCEPTION, "CLASS_ACCESS_EXCEPTION", start),
                },
                'D' => start[2] switch
                {
                    'G' => Result(RyuJitHelperMethod.DBG_IS_JUST_MY_CODE, "DBG_IS_JUST_MY_CODE", start),
                    'L' => start[4] switch
                    {
                        'I' => start[7] switch
                        {
                            '_' => Result(RyuJitHelperMethod.DBL2INT_OVF, "DBL2INT_OVF", start),
                            _ => Result(RyuJitHelperMethod.DBL2INT, "DBL2INT", start)
                        },
                        'L' => start[7] switch
                        {
                            '_' => Result(RyuJitHelperMethod.DBL2LNG_OVF, "DBL2LNG_OVF", start),
                            _ => Result(RyuJitHelperMethod.DBL2LNG, "DBL2LNG", start)
                        },
                        'U' => start[5] switch
                        {
                            'I' => start[8] switch
                            {
                                '_' => Result(RyuJitHelperMethod.DBL2UINT_OVF, "DBL2UINT_OVF", start),
                                _ => Result(RyuJitHelperMethod.DBL2UINT, "DBL2UINT", start)
                            },
                            _ => start[8] switch
                            {
                                '_' => Result(RyuJitHelperMethod.DBL2ULNG_OVF, "DBL2ULNG_OVF", start),
                                _ => Result(RyuJitHelperMethod.DBL2ULNG, "DBL2ULNG", start),
                            }
                        },
                        'E' => Result(RyuJitHelperMethod.DBLREM, "DBLREM", start),
                        _ => Result(RyuJitHelperMethod.DBLROUND, "DBLROUND", start)
                    },
                    'B' => Result(RyuJitHelperMethod.DEBUG_LOG_LOOP_CLONING, "DEBUG_LOG_LOOP_CLONING", start),
                    _ => Result(RyuJitHelperMethod.DIV, "DIV", start)
                },
                'E' => start[6] switch
                {
                    'E' => Result(RyuJitHelperMethod.EE_EXTERNAL_FIXUP, "EE_EXTERNAL_FIXUP", start),
                    'S' => start[10] switch
                    {
                        'L' => start[22] switch
                        {
                            '_' => Result(RyuJitHelperMethod.EE_PERSONALITY_ROUTINE_FILTER_FUNCLET, "EE_PERSONALITY_ROUTINE_FILTER_FUNCLET", start),
                            _ => Result(RyuJitHelperMethod.EE_PERSONALITY_ROUTINE, "EE_PERSONALITY_ROUTINE", start),
                        },
                        _ => Result(RyuJitHelperMethod.EE_PRESTUB, "EE_PRESTUB", start),
                    },
                    'V' => Result(RyuJitHelperMethod.EE_PINVOKE_FIXUP, "EE_PINVOKE_FIXUP", start),
                    'O' => Result(RyuJitHelperMethod.EE_REMOTING_THUNK, "EE_REMOTING_THUNK", start),
                    '_' => Result(RyuJitHelperMethod.EE_VSD_FIXUP, "EE_VSD_FIXUP", start),
                    'B' => Result(RyuJitHelperMethod.EE_VTABLE_FIXUP, "EE_VTABLE_FIXUP", start),
                    _ => start[7] switch
                    {
                        'O' => Result(RyuJitHelperMethod.EE_PRECODE_FIXUP, "EE_PRECODE_FIXUP", start),
                        _ => Result(RyuJitHelperMethod.ENDCATCH, "ENDCATCH", start)
                    }
                },
                'F' => start[5] switch
                {
                    'F' => Result(RyuJitHelperMethod.FAIL_FAST, "FAIL_FAST", start),
                    'D' => Result(RyuJitHelperMethod.FIELDDESC_TO_STUBRUNTIMEFIELD, "FIELDDESC_TO_STUBRUNTIMEFIELD", start),
                    '_' => Result(RyuJitHelperMethod.FIELD_ACCESS_EXCEPTION, "FIELD_ACCESS_EXCEPTION", start),
                    'M' => Result(RyuJitHelperMethod.FLTREM, "FLTREM", start),
                    _ => Result(RyuJitHelperMethod.FLTROUND, "FLTROUND", start)
                },
                'G' => start[8] switch
                {
                    'F' => start[9] switch
                    {
                        'L' => Result(RyuJitHelperMethod.GETFIELDFLOAT, "GETFIELDFLOAT", start),
                        _ => Result(RyuJitHelperMethod.GETCLASSFROMMETHODPARAM, "GETCLASSFROMMETHODPARAM", start)
                    },
                    'N' => Result(RyuJitHelperMethod.GETCURRENTMANAGEDTHREADID, "GETCURRENTMANAGEDTHREADID", start),
                    '1' => Result(RyuJitHelperMethod.GETFIELD16, "GETFIELD16", start),
                    '3' => Result(RyuJitHelperMethod.GETFIELD32, "GETFIELD32", start),
                    '6' => Result(RyuJitHelperMethod.GETFIELD64, "GETFIELD64", start),
                    '8' => Result(RyuJitHelperMethod.GETFIELD8, "GETFIELD8", start),
                    'A' => Result(RyuJitHelperMethod.GETFIELDADDR, "GETFIELDADDR", start),
                    'O' => Result(RyuJitHelperMethod.GETFIELDOBJ, "GETFIELDOBJ", start),
                    'S' => Result(RyuJitHelperMethod.GETFIELDSTRUCT, "GETFIELDSTRUCT", start),
                    'I' => start[18] switch
                    {
                        'I' => Result(RyuJitHelperMethod.GETGENERICS_GCSTATIC_BASE, "GETGENERICS_GCSTATIC_BASE", start),
                        'A' => Result(RyuJitHelperMethod.GETGENERICS_GCTHREADSTATIC_BASE, "GETGENERICS_GCTHREADSTATIC_BASE", start),
                        'T' => Result(RyuJitHelperMethod.GETGENERICS_NONGCSTATIC_BASE, "GETGENERICS_NONGCSTATIC_BASE", start),
                        _ => Result(RyuJitHelperMethod.GETGENERICS_NONGCTHREADSTATIC_BASE, "GETGENERICS_NONGCTHREADSTATIC_BASE", start)
                    },
                    'Y' => Result(RyuJitHelperMethod.GETREFANY, "GETREFANY", start),
                    'D' => start[9] switch
                    {
                        'O' => Result(RyuJitHelperMethod.GETFIELDDOUBLE, "GETFIELDDOUBLE", start),
                        _ => start[23] switch
                        {
                            '_' => start[24] switch
                            {
                                'D' => Result(RyuJitHelperMethod.GETSHARED_GCSTATIC_BASE_DYNAMICCLASS, "GETSHARED_GCSTATIC_BASE_DYNAMICCLASS", start),
                                _ => Result(RyuJitHelperMethod.GETSHARED_GCSTATIC_BASE_NOCTOR, "GETSHARED_GCSTATIC_BASE_NOCTOR", start),
                            },
                            'C' => start[29] switch
                            {
                                '_' => start[30] switch
                                {
                                    'D' => Result(RyuJitHelperMethod.GETSHARED_GCTHREADSTATIC_BASE_DYNAMICCLASS, "GETSHARED_GCTHREADSTATIC_BASE_DYNAMICCLASS", start),
                                    _ => Result(RyuJitHelperMethod.GETSHARED_GCTHREADSTATIC_BASE_NOCTOR, "GETSHARED_GCTHREADSTATIC_BASE_NOCTOR", start)
                                },
                                _ => Result(RyuJitHelperMethod.GETSHARED_GCTHREADSTATIC_BASE, "GETSHARED_GCTHREADSTATIC_BASE", start)
                            },
                            'A' => start[26] switch
                            {
                                '_' => start[27] switch
                                {
                                    'D' => Result(RyuJitHelperMethod.GETSHARED_NONGCSTATIC_BASE_DYNAMICCLASS, "GETSHARED_NONGCSTATIC_BASE_DYNAMICCLASS", start),
                                    _ => Result(RyuJitHelperMethod.GETSHARED_NONGCSTATIC_BASE_NOCTOR, "GETSHARED_NONGCSTATIC_BASE_NOCTOR", start),
                                },
                                'C' => start[32] switch
                                {
                                    '_' => start[33] switch
                                    {
                                        'D' => Result(RyuJitHelperMethod.GETSHARED_NONGCTHREADSTATIC_BASE_DYNAMICCLASS, "GETSHARED_NONGCTHREADSTATIC_BASE_DYNAMICCLASS", start),
                                        _ => Result(RyuJitHelperMethod.GETSHARED_NONGCTHREADSTATIC_BASE_NOCTOR, "GETSHARED_NONGCTHREADSTATIC_BASE_NOCTOR", start),
                                    },
                                    _ => Result(RyuJitHelperMethod.GETSHARED_NONGCTHREADSTATIC_BASE, "GETSHARED_NONGCTHREADSTATIC_BASE", start)
                                },
                                _ => Result(RyuJitHelperMethod.GETSHARED_NONGCSTATIC_BASE, "GETSHARED_NONGCSTATIC_BASE", start)
                            },
                            _ => Result(RyuJitHelperMethod.GETSHARED_GCSTATIC_BASE, "GETSHARED_GCSTATIC_BASE", start)
                        }
                    },
                    'C' => start[19] switch
                    {
                        'C' => Result(RyuJitHelperMethod.GETSTATICFIELDADDR_CONTEXT, "GETSTATICFIELDADDR_CONTEXT", start),
                        _ => Result(RyuJitHelperMethod.GETSTATICFIELDADDR_TLS, "GETSTATICFIELDADDR_TLS", start),
                    },
                    'R' => Result(RyuJitHelperMethod.GETSYNCFROMCLASSHANDLE, "GETSYNCFROMCLASSHANDLE", start),
                    _ => Result(RyuJitHelperMethod.GVMLOOKUP_FOR_SLOT, "GVMLOOKUP_FOR_SLOT", start)
                },
                _ => ParseRyuJitHelperMethod(start)
            };

            var helper = Result<RyuJitHelperMethod>(result, out width);
            width += 20;
            return helper;
        }
    }
}
