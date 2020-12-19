﻿namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal enum TokenKind
    {
        Unknown,
        Whitespace,
        OpenBracket,
        CloseBracket,        
        OpenCurly,
        CloseCurly,
        OpenParen,
        CloseParen,
        LessThan,
        GreaterThan,
        Colon,
        Semicolon,
        EqualsSign,
        SingleQuote,
        DoubleQuote,
        Comma,
        Hash,
        QuestionMark,
        Star,
        FourStars,
        SixStars,
        FifteenStars,
        Dot,
        TwoDots,
        ThreeDots,
        LineOfOneHundredAndThirtySevenDashes,
        Pipe,
        BasicBlock,
        BasicBlockNumberColumnHeader,
        BasicBlockIdColumnHeader,
        Statement,
        EndOfLine,
        EndOfFile,
        Integer,
        Word,
        Identifier,
        StartingPhase,
        FinishingPhase,
        NoChangesInPhase,
        UnknownILRange,
        StartingFunction,
        InlineStartingAt,
        InlineTreeHeader,
        AfterAction,
        BeforeAction,
        RefColumnHeader,
        TryColumnHeader,
        HandleColumnHeader,
        WeightColumnHeader,
        ILRangeColumnHeader,
        JumpColumnHeader,
        ExceptionHandlingColumnHeader,
        FlagsColumnHeader,
        TwelveDashes,
        BasicBlockInTopHeader,
        BasicBlockInInnerHeader,
        BasicBlockTableCenter,
        BasicBlockInTable,
        BasicBlockIdInTable,
        BasicBlockRefCountInTable,
        PredsColumnHeader,
        ReachabilitySetsHeader,
        FourtyEightDashes,
        DominatorSetsHeader
    }
}
