using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal unsafe static partial class Tokenizer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Token* Store(Token* tokens, TokenKind t0)
        {
            Assert.Dump(t0);

            tokens[0] = new(t0);

            return tokens + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Token* Store(Token* tokens, TokenKind t0, int v0) => Store(tokens, t0, (uint)v0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Token* Store(Token* tokens, TokenKind t0, uint v0)
        {
            Assert.Dump(new Token(t0, v0));
            Assert.True(BitConverter.IsLittleEndian);

            ((uint*)tokens)[0] = (uint)t0;
            ((uint*)tokens)[1] = v0;

            return tokens + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Token* Store(
            Token* tokens,
            TokenKind t0,
            TokenKind t1,
            TokenKind t2,
            TokenKind t3,
            TokenKind t4)
        {
            Assert.Dump(t0, t1, t2, t3, t4);

            Store(tokens, t0, t1, t2, t3);
            tokens[4] = new(t4);

            return tokens + 5;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Token* Store(
            Token* tokens,
            TokenKind t0,
            TokenKind t1,
            TokenKind t2,
            TokenKind t3,
            TokenKind t4,
            TokenKind t5,
            TokenKind t6,
            TokenKind t7,
            TokenKind t8,
            TokenKind t9,
            TokenKind t10)
        {
            Assert.Dump(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

            Store(tokens, t0, t1, t2, t3);
            Store(tokens, t4, t5, t6, t7);
            Store(tokens, t8, t9, t10, default);

            return tokens + 11;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Store(Token* tokens, TokenKind t0, TokenKind t1, TokenKind t2, TokenKind t3) => *(Vector256<ulong>*)tokens = Vector256.Create((ulong)new Token(t0), (ulong)new Token(t1), (ulong)new Token(t2), (ulong)new Token(t3));
    }
}