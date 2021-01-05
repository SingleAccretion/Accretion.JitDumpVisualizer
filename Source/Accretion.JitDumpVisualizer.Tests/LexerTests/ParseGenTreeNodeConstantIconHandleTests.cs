using Accretion.JitDumpVisualizer.Parsing.Tokens;
using Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing;
using Xunit;

namespace Accretion.JitDumpVisualizer.Tests.LexerTests
{
    public unsafe class ParseGenTreeNodeConstantIconHandleTests
    {
        [Theory]
        [InlineData("class", GenTreeNodeConstantIconHandle.Class, 5)]
        [InlineData("field", GenTreeNodeConstantIconHandle.Field, 5)]
        [InlineData("token", GenTreeNodeConstantIconHandle.Token, 5)]
        [InlineData("scope", GenTreeNodeConstantIconHandle.Scope, 5)]
        [InlineData("ftn", GenTreeNodeConstantIconHandle.Function, 3)]
        [InlineData("method", GenTreeNodeConstantIconHandle.Method, 6)]
        [InlineData("static", GenTreeNodeConstantIconHandle.Static, 6)]
        [InlineData("vararg", GenTreeNodeConstantIconHandle.Vararg, 6)]
        [InlineData("UNKNOWN", GenTreeNodeConstantIconHandle.Unknown, 7)]
        [InlineData("pinvoke", GenTreeNodeConstantIconHandle.PInvoke, 7)]
        [InlineData("cid/mid", GenTreeNodeConstantIconHandle.CidOrMid, 7)]
        [InlineData("bbc", GenTreeNodeConstantIconHandle.BasicBlockPointer, 3)]
        [InlineData("tls", GenTreeNodeConstantIconHandle.ThreadLocalStorage, 3)]
        [InlineData("globalptr", GenTreeNodeConstantIconHandle.GlobalPointer, 9)]
        [InlineData("constptr", GenTreeNodeConstantIconHandle.ConstantPointer, 8)]
        [InlineData("[ICON_STR_HDL]", GenTreeNodeConstantIconHandle.StringHandle, 14)]
        public void ParsesKnownHandlesCorrectly(string str, GenTreeNodeConstantIconHandle expectedHandle, int expectedWidth)
        {
            fixed (char* ptr = str)
            {
                Assert.Equal(expectedHandle, Lexer.ParseGenTreeNodeConstantIconHandle(ptr, out var width));
                Assert.Equal(expectedWidth, width);
            }
        }
    }
}
