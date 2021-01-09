using Accretion.JitDumpVisualizer.Parsing.Tokens;
using BenchmarkDotNet.Attributes;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Accretion.JitDumpVisualizer.CLI
{
    [DisassemblyDiagnoser(maxDepth: 2)]
    public unsafe class CallingConventionBenchmarks
    {
        public const int IterationCount = 1000;
        public static void* Tokens { get; }

        static CallingConventionBenchmarks()
        {
            Tokens = (Token*)Marshal.AllocHGlobal(100);
        }

        [Benchmark]
        [Arguments(0UL, 0UL, 0UL)]
        public nint StackCallConvBenchmark(ulong start, ulong tokens, ref ulong end)
        {
            var result = (nint)StackCallConv((char*)start, out var endPtr, (Token*)tokens);
            end = (ulong)(nint)endPtr;
            return result;
        }

        [Benchmark(OperationsPerInvoke = IterationCount)]
        public void SneakyCallConvBenchmark()
        {
            var start = 0;
            var tokens = Tokens;
            for (int i = 0; i < IterationCount; i++)
            {
                SneakyCallConv((char*)start, (Token*)tokens);
            }
        }

        [Benchmark]
        [Arguments(0UL, 0UL)]
        public double VectorCallConvBenchmark(ulong start, ulong tokens)
        {
            return VectorCallConv((char*)start, (Token*)tokens);
        }
        
        [Benchmark(OperationsPerInvoke = IterationCount)]
        public void ShiftyCallConvBenchmark()
        {
            var start = 0;
            var tokens = Tokens;
            for (int i = 0; i < IterationCount; i++)
            {
                ShiftyCallConv((char*)start, (Token*)tokens);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        internal Token* StackCallConv(char* start, out char* end, Token* tokens)
        {
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);

            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);

            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);

            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);

            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);

            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);

            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);

            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);
            tokens = ParseStack(start, out start, tokens);

            return ParseStack(start, out end, tokens);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Token* SneakyCallConv(char* start, Token* tokens)
        {
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;

            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;

            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;

            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;

            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;

            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;

            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;

            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;
            tokens = ParseSneaky(start, tokens);
            start = *(char**)tokens;

            return ParseSneaky(start, tokens);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal double VectorCallConv(char* start, Token* tokens)
        {
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);

            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);

            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);

            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);

            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);

            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);

            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);

            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);
            ExtractValuesFromVector(ParseVector(start, tokens), out start, out tokens);

            return ParseVector(start, tokens);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong ShiftyCallConv(char* start, Token* tokens)
        {
            var res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;

            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;

            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;

            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;

            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;

            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;

            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;

            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;
            res = ParseShifty(start, tokens);
            start += (uint)res;
            tokens += res >> 32;

            return ParseShifty(start, tokens);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        // Total bytes of code 15
        private static double ParseVector(char* start, Token* tokens)
        {
            if (IntPtr.Size == 8)
            {
                return Vector128.Create((ulong)start, (ulong)tokens).AsDouble().ToScalar();
            }
            else
            {
                return Vector128.Create((uint)start, (uint)tokens, 0, 0).AsDouble().ToScalar();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        // Total bytes of code 7
        private static Token* ParseSneaky(char* start, Token* tokens)
        {
            *(nint*)tokens = (nint)start;
            return tokens;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        // Total bytes of code 7
        private static Token* ParseStack(char* start, out char* end, Token* tokens)
        {
            end = start;
            return tokens;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong ParseShifty(char* start, Token* tokens)
        {
            return 5 | (5 << 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExtractValuesFromVector(double vector, out char* start, out Token* tokens)
        {
            if (IntPtr.Size == 8)
            {
                var v = Vector128.CreateScalarUnsafe(vector).AsUInt64();
                start = (char*)(nuint)v.GetElement(0);
                tokens = (Token*)(nuint)v.GetElement(1);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Unpack(ulong result, ref char* start, ref Token* tokens)
        {
            start += (uint)result;
            tokens += result >> 32;
        }
    }
}
