using Accretion.JitDumpVisualizer.IL;
using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using System.Runtime.CompilerServices;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        // Parsing methods in this file save on machine code size in switches by packing the information
        // This is in the following general format (measured to save up to ~25% in code size as compared to the naive approach):
        // ulong result = [...[Width][Enum]]
        // Tight packing ensures nothing is wasted on encoding the constants in assembly
        // Further savings could be achieved by returning "result" directly
        // Instead of using "out int width"
        // I am not prepared to go that far yet

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Result<T>(T value, string represenation, char* start) where T : struct, Enum
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static ulong Cast(T enumValue) => Unsafe.SizeOf<T>() switch
            {
                1 => ILLegal.As<T, byte>(enumValue),
                2 => ILLegal.As<T, ushort>(enumValue),
                _ => ILLegal.As<T, uint>(enumValue),
            };

            Assert.True(Unsafe.SizeOf<T>() <= 4);
            Assert.Equal(start, represenation);

            return Cast(value) | ((ulong)represenation.Length << (Unsafe.SizeOf<T>() * 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Result<T>(ulong result, out int width) where T : struct, Enum
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static T Cast(ulong value) => Unsafe.SizeOf<T>() switch
            {
                1 => ILLegal.As<byte, T>((byte)value),
                2 => ILLegal.As<ushort, T>((ushort)value),
                _ => ILLegal.As<uint, T>((uint)value)
            };

            Assert.True(Unsafe.SizeOf<T>() <= 4);

            width = (int)(result >> (Unsafe.SizeOf<T>() * 8));
            return Cast(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Result<T>(T value, string represenation, ref char* start) where T : struct, Enum
        {
            Assert.True(Unsafe.SizeOf<T>() <= 4);
            Assert.Equal(start, represenation);

            start += represenation.Length;
            return value;
        }
    }
}
