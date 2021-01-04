﻿using Accretion.JitDumpVisualizer.Parsing.Tokens;
using Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing;
using Xunit;

namespace Accretion.JitDumpVisualizer.Tests.LexerTests
{
    public unsafe class ParseRyuJitHelperTests
    {
        [InlineData("HELPER.CORINFO_HELP_ARE_TYPES_EQUIVALENT", RyuJitHelperMethod.ARE_TYPES_EQUIVALENT, 40)]
        [InlineData("HELPER.CORINFO_HELP_ARRADDR_ST", RyuJitHelperMethod.ARRADDR_ST, 30)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_BYREF", RyuJitHelperMethod.ASSIGN_BYREF, 32)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_REF", RyuJitHelperMethod.ASSIGN_REF, 30)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_REF_EAX", RyuJitHelperMethod.ASSIGN_REF_EAX, 34)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_REF_EBP", RyuJitHelperMethod.ASSIGN_REF_EBP, 34)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_REF_EBX", RyuJitHelperMethod.ASSIGN_REF_EBX, 34)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_REF_ECX", RyuJitHelperMethod.ASSIGN_REF_ECX, 34)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_REF_EDI", RyuJitHelperMethod.ASSIGN_REF_EDI, 34)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_REF_ENSURE_NONHEAP", RyuJitHelperMethod.ASSIGN_REF_ENSURE_NONHEAP, 45)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_REF_ESI", RyuJitHelperMethod.ASSIGN_REF_ESI, 34)]
        [InlineData("HELPER.CORINFO_HELP_ASSIGN_STRUCT", RyuJitHelperMethod.ASSIGN_STRUCT, 33)]
        [InlineData("HELPER.CORINFO_HELP_BBT_FCN_ENTER", RyuJitHelperMethod.BBT_FCN_ENTER, 33)]
        [InlineData("HELPER.CORINFO_HELP_BOX", RyuJitHelperMethod.BOX, 23)]
        [InlineData("HELPER.CORINFO_HELP_BOX_NULLABLE", RyuJitHelperMethod.BOX_NULLABLE, 32)]
        [InlineData("HELPER.CORINFO_HELP_CHECKED_ASSIGN_REF", RyuJitHelperMethod.CHECKED_ASSIGN_REF, 38)]
        [InlineData("HELPER.CORINFO_HELP_CHECKED_ASSIGN_REF_EAX", RyuJitHelperMethod.CHECKED_ASSIGN_REF_EAX, 42)]
        [InlineData("HELPER.CORINFO_HELP_CHECKED_ASSIGN_REF_EBP", RyuJitHelperMethod.CHECKED_ASSIGN_REF_EBP, 42)]
        [InlineData("HELPER.CORINFO_HELP_CHECKED_ASSIGN_REF_EBX", RyuJitHelperMethod.CHECKED_ASSIGN_REF_EBX, 42)]
        [InlineData("HELPER.CORINFO_HELP_CHECKED_ASSIGN_REF_ECX", RyuJitHelperMethod.CHECKED_ASSIGN_REF_ECX, 42)]
        [InlineData("HELPER.CORINFO_HELP_CHECKED_ASSIGN_REF_EDI", RyuJitHelperMethod.CHECKED_ASSIGN_REF_EDI, 42)]
        [InlineData("HELPER.CORINFO_HELP_CHECKED_ASSIGN_REF_ESI", RyuJitHelperMethod.CHECKED_ASSIGN_REF_ESI, 42)]
        [InlineData("HELPER.CORINFO_HELP_CHECK_OBJ", RyuJitHelperMethod.CHECK_OBJ, 29)]
        [InlineData("HELPER.CORINFO_HELP_CHKCASTANY", RyuJitHelperMethod.CHKCASTANY, 30)]
        [InlineData("HELPER.CORINFO_HELP_CHKCASTARRAY", RyuJitHelperMethod.CHKCASTARRAY, 32)]
        [InlineData("HELPER.CORINFO_HELP_CHKCASTCLASS", RyuJitHelperMethod.CHKCASTCLASS, 32)]
        [InlineData("HELPER.CORINFO_HELP_CHKCASTCLASS_SPECIAL", RyuJitHelperMethod.CHKCASTCLASS_SPECIAL, 40)]
        [InlineData("HELPER.CORINFO_HELP_CHKCASTINTERFACE", RyuJitHelperMethod.CHKCASTINTERFACE, 36)]
        [InlineData("HELPER.CORINFO_HELP_CLASSINIT_SHARED_DYNAMICCLASS", RyuJitHelperMethod.CLASSINIT_SHARED_DYNAMICCLASS, 49)]
        [InlineData("HELPER.CORINFO_HELP_CLASSPROFILE", RyuJitHelperMethod.CLASSPROFILE, 32)]
        [InlineData("HELPER.CORINFO_HELP_CLASS_ACCESS_EXCEPTION", RyuJitHelperMethod.CLASS_ACCESS_EXCEPTION, 42)]
        [InlineData("HELPER.CORINFO_HELP_DBG_IS_JUST_MY_CODE", RyuJitHelperMethod.DBG_IS_JUST_MY_CODE, 39)]
        [InlineData("HELPER.CORINFO_HELP_DBL2INT", RyuJitHelperMethod.DBL2INT, 27)]
        [InlineData("HELPER.CORINFO_HELP_DBL2INT_OVF", RyuJitHelperMethod.DBL2INT_OVF, 31)]
        [InlineData("HELPER.CORINFO_HELP_DBL2LNG", RyuJitHelperMethod.DBL2LNG, 27)]
        [InlineData("HELPER.CORINFO_HELP_DBL2LNG_OVF", RyuJitHelperMethod.DBL2LNG_OVF, 31)]
        [InlineData("HELPER.CORINFO_HELP_DBL2UINT", RyuJitHelperMethod.DBL2UINT, 28)]
        [InlineData("HELPER.CORINFO_HELP_DBL2UINT_OVF", RyuJitHelperMethod.DBL2UINT_OVF, 32)]
        [InlineData("HELPER.CORINFO_HELP_DBL2ULNG", RyuJitHelperMethod.DBL2ULNG, 28)]
        [InlineData("HELPER.CORINFO_HELP_DBL2ULNG_OVF", RyuJitHelperMethod.DBL2ULNG_OVF, 32)]
        [InlineData("HELPER.CORINFO_HELP_DBLREM", RyuJitHelperMethod.DBLREM, 26)]
        [InlineData("HELPER.CORINFO_HELP_DBLROUND", RyuJitHelperMethod.DBLROUND, 28)]
        [InlineData("HELPER.CORINFO_HELP_DEBUG_LOG_LOOP_CLONING", RyuJitHelperMethod.DEBUG_LOG_LOOP_CLONING, 42)]
        [InlineData("HELPER.CORINFO_HELP_DIV", RyuJitHelperMethod.DIV, 23)]
        [InlineData("HELPER.CORINFO_HELP_EE_EXTERNAL_FIXUP", RyuJitHelperMethod.EE_EXTERNAL_FIXUP, 37)]
        [InlineData("HELPER.CORINFO_HELP_EE_PERSONALITY_ROUTINE", RyuJitHelperMethod.EE_PERSONALITY_ROUTINE, 42)]
        [InlineData("HELPER.CORINFO_HELP_EE_PERSONALITY_ROUTINE_FILTER_FUNCLET", RyuJitHelperMethod.EE_PERSONALITY_ROUTINE_FILTER_FUNCLET, 57)]
        [InlineData("HELPER.CORINFO_HELP_EE_PINVOKE_FIXUP", RyuJitHelperMethod.EE_PINVOKE_FIXUP, 36)]
        [InlineData("HELPER.CORINFO_HELP_EE_PRECODE_FIXUP", RyuJitHelperMethod.EE_PRECODE_FIXUP, 36)]
        [InlineData("HELPER.CORINFO_HELP_EE_PRESTUB", RyuJitHelperMethod.EE_PRESTUB, 30)]
        [InlineData("HELPER.CORINFO_HELP_EE_REMOTING_THUNK", RyuJitHelperMethod.EE_REMOTING_THUNK, 37)]
        [InlineData("HELPER.CORINFO_HELP_EE_VSD_FIXUP", RyuJitHelperMethod.EE_VSD_FIXUP, 32)]
        [InlineData("HELPER.CORINFO_HELP_EE_VTABLE_FIXUP", RyuJitHelperMethod.EE_VTABLE_FIXUP, 35)]
        [InlineData("HELPER.CORINFO_HELP_ENDCATCH", RyuJitHelperMethod.ENDCATCH, 28)]
        [InlineData("HELPER.CORINFO_HELP_FAIL_FAST", RyuJitHelperMethod.FAIL_FAST, 29)]
        [InlineData("HELPER.CORINFO_HELP_FIELDDESC_TO_STUBRUNTIMEFIELD", RyuJitHelperMethod.FIELDDESC_TO_STUBRUNTIMEFIELD, 49)]
        [InlineData("HELPER.CORINFO_HELP_FIELD_ACCESS_EXCEPTION", RyuJitHelperMethod.FIELD_ACCESS_EXCEPTION, 42)]
        [InlineData("HELPER.CORINFO_HELP_FLTREM", RyuJitHelperMethod.FLTREM, 26)]
        [InlineData("HELPER.CORINFO_HELP_FLTROUND", RyuJitHelperMethod.FLTROUND, 28)]
        [InlineData("HELPER.CORINFO_HELP_GETCLASSFROMMETHODPARAM", RyuJitHelperMethod.GETCLASSFROMMETHODPARAM, 43)]
        [InlineData("HELPER.CORINFO_HELP_GETCURRENTMANAGEDTHREADID", RyuJitHelperMethod.GETCURRENTMANAGEDTHREADID, 45)]
        [InlineData("HELPER.CORINFO_HELP_GETFIELD16", RyuJitHelperMethod.GETFIELD16, 30)]
        [InlineData("HELPER.CORINFO_HELP_GETFIELD32", RyuJitHelperMethod.GETFIELD32, 30)]
        [InlineData("HELPER.CORINFO_HELP_GETFIELD64", RyuJitHelperMethod.GETFIELD64, 30)]
        [InlineData("HELPER.CORINFO_HELP_GETFIELD8", RyuJitHelperMethod.GETFIELD8, 29)]
        [InlineData("HELPER.CORINFO_HELP_GETFIELDADDR", RyuJitHelperMethod.GETFIELDADDR, 32)]
        [InlineData("HELPER.CORINFO_HELP_GETFIELDDOUBLE", RyuJitHelperMethod.GETFIELDDOUBLE, 34)]
        [InlineData("HELPER.CORINFO_HELP_GETFIELDFLOAT", RyuJitHelperMethod.GETFIELDFLOAT, 33)]
        [InlineData("HELPER.CORINFO_HELP_GETFIELDOBJ", RyuJitHelperMethod.GETFIELDOBJ, 31)]
        [InlineData("HELPER.CORINFO_HELP_GETFIELDSTRUCT", RyuJitHelperMethod.GETFIELDSTRUCT, 34)]
        [InlineData("HELPER.CORINFO_HELP_GETGENERICS_GCSTATIC_BASE", RyuJitHelperMethod.GETGENERICS_GCSTATIC_BASE, 45)]
        [InlineData("HELPER.CORINFO_HELP_GETGENERICS_GCTHREADSTATIC_BASE", RyuJitHelperMethod.GETGENERICS_GCTHREADSTATIC_BASE, 51)]
        [InlineData("HELPER.CORINFO_HELP_GETGENERICS_NONGCSTATIC_BASE", RyuJitHelperMethod.GETGENERICS_NONGCSTATIC_BASE, 48)]
        [InlineData("HELPER.CORINFO_HELP_GETGENERICS_NONGCTHREADSTATIC_BASE", RyuJitHelperMethod.GETGENERICS_NONGCTHREADSTATIC_BASE, 54)]
        [InlineData("HELPER.CORINFO_HELP_GETREFANY", RyuJitHelperMethod.GETREFANY, 29)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_GCSTATIC_BASE", RyuJitHelperMethod.GETSHARED_GCSTATIC_BASE, 43)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_GCSTATIC_BASE_DYNAMICCLASS", RyuJitHelperMethod.GETSHARED_GCSTATIC_BASE_DYNAMICCLASS, 56)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_GCSTATIC_BASE_NOCTOR", RyuJitHelperMethod.GETSHARED_GCSTATIC_BASE_NOCTOR, 50)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE", RyuJitHelperMethod.GETSHARED_GCTHREADSTATIC_BASE, 49)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE_DYNAMICCLASS", RyuJitHelperMethod.GETSHARED_GCTHREADSTATIC_BASE_DYNAMICCLASS, 62)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE_NOCTOR", RyuJitHelperMethod.GETSHARED_GCTHREADSTATIC_BASE_NOCTOR, 56)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE", RyuJitHelperMethod.GETSHARED_NONGCSTATIC_BASE, 46)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE_DYNAMICCLASS", RyuJitHelperMethod.GETSHARED_NONGCSTATIC_BASE_DYNAMICCLASS, 59)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE_NOCTOR", RyuJitHelperMethod.GETSHARED_NONGCSTATIC_BASE_NOCTOR, 53)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE", RyuJitHelperMethod.GETSHARED_NONGCTHREADSTATIC_BASE, 52)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE_DYNAMICCLASS", RyuJitHelperMethod.GETSHARED_NONGCTHREADSTATIC_BASE_DYNAMICCLASS, 65)]
        [InlineData("HELPER.CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE_NOCTOR", RyuJitHelperMethod.GETSHARED_NONGCTHREADSTATIC_BASE_NOCTOR, 59)]
        [InlineData("HELPER.CORINFO_HELP_GETSTATICFIELDADDR_CONTEXT", RyuJitHelperMethod.GETSTATICFIELDADDR_CONTEXT, 46)]
        [InlineData("HELPER.CORINFO_HELP_GETSTATICFIELDADDR_TLS", RyuJitHelperMethod.GETSTATICFIELDADDR_TLS, 42)]
        [InlineData("HELPER.CORINFO_HELP_GETSYNCFROMCLASSHANDLE", RyuJitHelperMethod.GETSYNCFROMCLASSHANDLE, 42)]
        [InlineData("HELPER.CORINFO_HELP_GVMLOOKUP_FOR_SLOT", RyuJitHelperMethod.GVMLOOKUP_FOR_SLOT, 38)]
        [InlineData("HELPER.CORINFO_HELP_INITCLASS", RyuJitHelperMethod.INITCLASS, 29)]
        [InlineData("HELPER.CORINFO_HELP_INITINSTCLASS", RyuJitHelperMethod.INITINSTCLASS, 33)]
        [InlineData("HELPER.CORINFO_HELP_INIT_PINVOKE_FRAME", RyuJitHelperMethod.INIT_PINVOKE_FRAME, 38)]
        [InlineData("HELPER.CORINFO_HELP_ISINSTANCEOFANY", RyuJitHelperMethod.ISINSTANCEOFANY, 35)]
        [InlineData("HELPER.CORINFO_HELP_ISINSTANCEOFARRAY", RyuJitHelperMethod.ISINSTANCEOFARRAY, 37)]
        [InlineData("HELPER.CORINFO_HELP_ISINSTANCEOFCLASS", RyuJitHelperMethod.ISINSTANCEOFCLASS, 37)]
        [InlineData("HELPER.CORINFO_HELP_ISINSTANCEOFINTERFACE", RyuJitHelperMethod.ISINSTANCEOFINTERFACE, 41)]
        [InlineData("HELPER.CORINFO_HELP_JIT_PINVOKE_BEGIN", RyuJitHelperMethod.JIT_PINVOKE_BEGIN, 37)]
        [InlineData("HELPER.CORINFO_HELP_JIT_PINVOKE_END", RyuJitHelperMethod.JIT_PINVOKE_END, 35)]
        [InlineData("HELPER.CORINFO_HELP_JIT_REVERSE_PINVOKE_ENTER", RyuJitHelperMethod.JIT_REVERSE_PINVOKE_ENTER, 45)]
        [InlineData("HELPER.CORINFO_HELP_JIT_REVERSE_PINVOKE_EXIT", RyuJitHelperMethod.JIT_REVERSE_PINVOKE_EXIT, 44)]
        [InlineData("HELPER.CORINFO_HELP_LDELEMA_REF", RyuJitHelperMethod.LDELEMA_REF, 31)]
        [InlineData("HELPER.CORINFO_HELP_LDIV", RyuJitHelperMethod.LDIV, 24)]
        [InlineData("HELPER.CORINFO_HELP_LLSH", RyuJitHelperMethod.LLSH, 24)]
        [InlineData("HELPER.CORINFO_HELP_LMOD", RyuJitHelperMethod.LMOD, 24)]
        [InlineData("HELPER.CORINFO_HELP_LMUL", RyuJitHelperMethod.LMUL, 24)]
        [InlineData("HELPER.CORINFO_HELP_LMUL_OVF", RyuJitHelperMethod.LMUL_OVF, 28)]
        [InlineData("HELPER.CORINFO_HELP_LNG2DBL", RyuJitHelperMethod.LNG2DBL, 27)]
        [InlineData("HELPER.CORINFO_HELP_LOOP_CLONE_CHOICE_ADDR", RyuJitHelperMethod.LOOP_CLONE_CHOICE_ADDR, 42)]
        [InlineData("HELPER.CORINFO_HELP_LRSH", RyuJitHelperMethod.LRSH, 24)]
        [InlineData("HELPER.CORINFO_HELP_LRSZ", RyuJitHelperMethod.LRSZ, 24)]
        [InlineData("HELPER.CORINFO_HELP_MEMCPY", RyuJitHelperMethod.MEMCPY, 26)]
        [InlineData("HELPER.CORINFO_HELP_MEMSET", RyuJitHelperMethod.MEMSET, 26)]
        [InlineData("HELPER.CORINFO_HELP_METHODDESC_TO_STUBRUNTIMEMETHOD", RyuJitHelperMethod.METHODDESC_TO_STUBRUNTIMEMETHOD, 51)]
        [InlineData("HELPER.CORINFO_HELP_METHOD_ACCESS_EXCEPTION", RyuJitHelperMethod.METHOD_ACCESS_EXCEPTION, 43)]
        [InlineData("HELPER.CORINFO_HELP_MOD", RyuJitHelperMethod.MOD, 23)]
        [InlineData("HELPER.CORINFO_HELP_MON_ENTER", RyuJitHelperMethod.MON_ENTER, 29)]
        [InlineData("HELPER.CORINFO_HELP_MON_ENTER_STATIC", RyuJitHelperMethod.MON_ENTER_STATIC, 36)]
        [InlineData("HELPER.CORINFO_HELP_MON_EXIT", RyuJitHelperMethod.MON_EXIT, 28)]
        [InlineData("HELPER.CORINFO_HELP_MON_EXIT_STATIC", RyuJitHelperMethod.MON_EXIT_STATIC, 35)]
        [InlineData("HELPER.CORINFO_HELP_NEWARR_1_ALIGN8", RyuJitHelperMethod.NEWARR_1_ALIGN8, 35)]
        [InlineData("HELPER.CORINFO_HELP_NEWARR_1_DIRECT", RyuJitHelperMethod.NEWARR_1_DIRECT, 35)]
        [InlineData("HELPER.CORINFO_HELP_NEWARR_1_OBJ", RyuJitHelperMethod.NEWARR_1_OBJ, 32)]
        [InlineData("HELPER.CORINFO_HELP_NEWARR_1_VC", RyuJitHelperMethod.NEWARR_1_VC, 31)]
        [InlineData("HELPER.CORINFO_HELP_NEWFAST", RyuJitHelperMethod.NEWFAST, 27)]
        [InlineData("HELPER.CORINFO_HELP_NEWSFAST", RyuJitHelperMethod.NEWSFAST, 28)]
        [InlineData("HELPER.CORINFO_HELP_NEWSFAST_ALIGN8", RyuJitHelperMethod.NEWSFAST_ALIGN8, 35)]
        [InlineData("HELPER.CORINFO_HELP_NEWSFAST_ALIGN8_FINALIZE", RyuJitHelperMethod.NEWSFAST_ALIGN8_FINALIZE, 44)]
        [InlineData("HELPER.CORINFO_HELP_NEWSFAST_ALIGN8_VC", RyuJitHelperMethod.NEWSFAST_ALIGN8_VC, 38)]
        [InlineData("HELPER.CORINFO_HELP_NEWSFAST_FINALIZE", RyuJitHelperMethod.NEWSFAST_FINALIZE, 37)]
        [InlineData("HELPER.CORINFO_HELP_NEW_MDARR", RyuJitHelperMethod.NEW_MDARR, 29)]
        [InlineData("HELPER.CORINFO_HELP_NEW_MDARR_NONVARARG", RyuJitHelperMethod.NEW_MDARR_NONVARARG, 39)]
        [InlineData("HELPER.CORINFO_HELP_OVERFLOW", RyuJitHelperMethod.OVERFLOW, 28)]
        [InlineData("HELPER.CORINFO_HELP_PATCHPOINT", RyuJitHelperMethod.PATCHPOINT, 30)]
        [InlineData("HELPER.CORINFO_HELP_PINVOKE_CALLI", RyuJitHelperMethod.PINVOKE_CALLI, 33)]
        [InlineData("HELPER.CORINFO_HELP_POLL_GC", RyuJitHelperMethod.POLL_GC, 27)]
        [InlineData("HELPER.CORINFO_HELP_PROF_FCN_ENTER", RyuJitHelperMethod.PROF_FCN_ENTER, 34)]
        [InlineData("HELPER.CORINFO_HELP_PROF_FCN_LEAVE", RyuJitHelperMethod.PROF_FCN_LEAVE, 34)]
        [InlineData("HELPER.CORINFO_HELP_PROF_FCN_TAILCALL", RyuJitHelperMethod.PROF_FCN_TAILCALL, 37)]
        [InlineData("HELPER.CORINFO_HELP_READYTORUN_CHKCAST", RyuJitHelperMethod.READYTORUN_CHKCAST, 38)]
        [InlineData("HELPER.CORINFO_HELP_READYTORUN_DELEGATE_CTOR", RyuJitHelperMethod.READYTORUN_DELEGATE_CTOR, 44)]
        [InlineData("HELPER.CORINFO_HELP_READYTORUN_GENERIC_HANDLE", RyuJitHelperMethod.READYTORUN_GENERIC_HANDLE, 45)]
        [InlineData("HELPER.CORINFO_HELP_READYTORUN_GENERIC_STATIC_BASE", RyuJitHelperMethod.READYTORUN_GENERIC_STATIC_BASE, 50)]
        [InlineData("HELPER.CORINFO_HELP_READYTORUN_ISINSTANCEOF", RyuJitHelperMethod.READYTORUN_ISINSTANCEOF, 43)]
        [InlineData("HELPER.CORINFO_HELP_READYTORUN_NEW", RyuJitHelperMethod.READYTORUN_NEW, 34)]
        [InlineData("HELPER.CORINFO_HELP_READYTORUN_NEWARR_1", RyuJitHelperMethod.READYTORUN_NEWARR_1, 39)]
        [InlineData("HELPER.CORINFO_HELP_READYTORUN_STATIC_BASE", RyuJitHelperMethod.READYTORUN_STATIC_BASE, 42)]
        [InlineData("HELPER.CORINFO_HELP_READYTORUN_VIRTUAL_FUNC_PTR", RyuJitHelperMethod.READYTORUN_VIRTUAL_FUNC_PTR, 47)]
        [InlineData("HELPER.CORINFO_HELP_RETHROW", RyuJitHelperMethod.RETHROW, 27)]
        [InlineData("HELPER.CORINFO_HELP_RNGCHKFAIL", RyuJitHelperMethod.RNGCHKFAIL, 30)]
        [InlineData("HELPER.CORINFO_HELP_RUNTIMEHANDLE_CLASS", RyuJitHelperMethod.RUNTIMEHANDLE_CLASS, 39)]
        [InlineData("HELPER.CORINFO_HELP_RUNTIMEHANDLE_CLASS_LOG", RyuJitHelperMethod.RUNTIMEHANDLE_CLASS_LOG, 43)]
        [InlineData("HELPER.CORINFO_HELP_RUNTIMEHANDLE_METHOD", RyuJitHelperMethod.RUNTIMEHANDLE_METHOD, 40)]
        [InlineData("HELPER.CORINFO_HELP_RUNTIMEHANDLE_METHOD_LOG", RyuJitHelperMethod.RUNTIMEHANDLE_METHOD_LOG, 44)]
        [InlineData("HELPER.CORINFO_HELP_SETFIELD16", RyuJitHelperMethod.SETFIELD16, 30)]
        [InlineData("HELPER.CORINFO_HELP_SETFIELD32", RyuJitHelperMethod.SETFIELD32, 30)]
        [InlineData("HELPER.CORINFO_HELP_SETFIELD64", RyuJitHelperMethod.SETFIELD64, 30)]
        [InlineData("HELPER.CORINFO_HELP_SETFIELD8", RyuJitHelperMethod.SETFIELD8, 29)]
        [InlineData("HELPER.CORINFO_HELP_SETFIELDDOUBLE", RyuJitHelperMethod.SETFIELDDOUBLE, 34)]
        [InlineData("HELPER.CORINFO_HELP_SETFIELDFLOAT", RyuJitHelperMethod.SETFIELDFLOAT, 33)]
        [InlineData("HELPER.CORINFO_HELP_SETFIELDOBJ", RyuJitHelperMethod.SETFIELDOBJ, 31)]
        [InlineData("HELPER.CORINFO_HELP_SETFIELDSTRUCT", RyuJitHelperMethod.SETFIELDSTRUCT, 34)]
        [InlineData("HELPER.CORINFO_HELP_STACK_PROBE", RyuJitHelperMethod.STACK_PROBE, 31)]
        [InlineData("HELPER.CORINFO_HELP_STOP_FOR_GC", RyuJitHelperMethod.STOP_FOR_GC, 31)]
        [InlineData("HELPER.CORINFO_HELP_STRCNS", RyuJitHelperMethod.STRCNS, 26)]
        [InlineData("HELPER.CORINFO_HELP_STRCNS_CURRENT_MODULE", RyuJitHelperMethod.STRCNS_CURRENT_MODULE, 41)]
        [InlineData("HELPER.CORINFO_HELP_STRESS_GC", RyuJitHelperMethod.STRESS_GC, 29)]
        [InlineData("HELPER.CORINFO_HELP_TAILCALL", RyuJitHelperMethod.TAILCALL, 28)]
        [InlineData("HELPER.CORINFO_HELP_THROW", RyuJitHelperMethod.THROW, 25)]
        [InlineData("HELPER.CORINFO_HELP_THROWDIVZERO", RyuJitHelperMethod.THROWDIVZERO, 32)]
        [InlineData("HELPER.CORINFO_HELP_THROWNULLREF", RyuJitHelperMethod.THROWNULLREF, 32)]
        [InlineData("HELPER.CORINFO_HELP_THROW_ARGUMENTEXCEPTION", RyuJitHelperMethod.THROW_ARGUMENTEXCEPTION, 43)]
        [InlineData("HELPER.CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION", RyuJitHelperMethod.THROW_ARGUMENTOUTOFRANGEEXCEPTION, 53)]
        [InlineData("HELPER.CORINFO_HELP_THROW_NOT_IMPLEMENTED", RyuJitHelperMethod.THROW_NOT_IMPLEMENTED, 41)]
        [InlineData("HELPER.CORINFO_HELP_THROW_PLATFORM_NOT_SUPPORTED", RyuJitHelperMethod.THROW_PLATFORM_NOT_SUPPORTED, 48)]
        [InlineData("HELPER.CORINFO_HELP_THROW_TYPE_NOT_SUPPORTED", RyuJitHelperMethod.THROW_TYPE_NOT_SUPPORTED, 44)]
        [InlineData("HELPER.CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPE", RyuJitHelperMethod.TYPEHANDLE_TO_RUNTIMETYPE, 45)]
        [InlineData("HELPER.CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPEHANDLE", RyuJitHelperMethod.TYPEHANDLE_TO_RUNTIMETYPEHANDLE, 51)]
        [InlineData("HELPER.CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPEHANDLE_MAYBENULL", RyuJitHelperMethod.TYPEHANDLE_TO_RUNTIMETYPEHANDLE_MAYBENULL, 61)]
        [InlineData("HELPER.CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPE_MAYBENULL", RyuJitHelperMethod.TYPEHANDLE_TO_RUNTIMETYPE_MAYBENULL, 55)]
        [InlineData("HELPER.CORINFO_HELP_UDIV", RyuJitHelperMethod.UDIV, 24)]
        [InlineData("HELPER.CORINFO_HELP_ULDIV", RyuJitHelperMethod.ULDIV, 25)]
        [InlineData("HELPER.CORINFO_HELP_ULMOD", RyuJitHelperMethod.ULMOD, 25)]
        [InlineData("HELPER.CORINFO_HELP_ULMUL_OVF", RyuJitHelperMethod.ULMUL_OVF, 29)]
        [InlineData("HELPER.CORINFO_HELP_ULNG2DBL", RyuJitHelperMethod.ULNG2DBL, 28)]
        [InlineData("HELPER.CORINFO_HELP_UMOD", RyuJitHelperMethod.UMOD, 24)]
        [InlineData("HELPER.CORINFO_HELP_UNBOX", RyuJitHelperMethod.UNBOX, 25)]
        [InlineData("HELPER.CORINFO_HELP_UNBOX_NULLABLE", RyuJitHelperMethod.UNBOX_NULLABLE, 34)]
        [InlineData("HELPER.CORINFO_HELP_UNDEF", RyuJitHelperMethod.UNDEF, 25)]
        [InlineData("HELPER.CORINFO_HELP_USER_BREAKPOINT", RyuJitHelperMethod.USER_BREAKPOINT, 35)]
        [InlineData("HELPER.CORINFO_HELP_VERIFICATION", RyuJitHelperMethod.VERIFICATION, 32)]
        [InlineData("HELPER.CORINFO_HELP_VIRTUAL_FUNC_PTR", RyuJitHelperMethod.VIRTUAL_FUNC_PTR, 36)]
        [Theory]
        public void ParsesKnownHelpersCorrectly(string str, RyuJitHelperMethod expectedHelper, int expectedWidth)
        {
            fixed (char* ptr = str)
            {
                Assert.Equal(expectedHelper, Lexer.ParseRyuJitHelperMethod(ptr, out var width));
                Assert.Equal(expectedWidth, width);
            }
        }
    }
}
