﻿using Accretion.JitDumpVisualizer.Parsing.Auxiliaries;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal unsafe static partial class Tokenizer
    {
        private static char* SkipEndOfLine(char* start)
        {
            Assert.Dump(TokenKind.EndOfLine);
            if (*start is '\r')
            {
                Assert.Equal(start, "\r\n");
            }
            else
            {
                Assert.Equal(start, "\n");
            }
            
            return start + (((nint)(*start) >> 1) & 0b11);
        }

        private static char* SkipWhitespaces(char* start)
        {
            while (*start is ' ')
            {
                start++;
            }

            return start;
        }

        private static bool IsNotEndOfLine(char* start)
        {
            if (*start is '\r' or '\n')
            {
                return false;
            }

            return true;
        }

        private static bool IsEndOfLine(char* start)
        {
            if (*start is '\r' or '\n')
            {
                return true;
            }

            return false;
        }
    }
}