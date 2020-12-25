﻿namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal enum TokenKind
    {
        Unknown,
        Whitespace,
        Star,
        FourStars,
        SixStars,
        FifteenStars,
        Dot,
        TwoDots,
        ThreeDots,
        BasicBlockTableHeader,
        Pipe,
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
        DominatorSetsHeader,
        BasicBlockTryCountInTable,
        BasicBlockHandleCountInTable,
        BasicBlockPredInTable,
        BasicBlockWeightInTable,
        SeventyFiveStars,
        BasicBlockILRangeStartInTable,
        BasicBlockILRangeEndInTable,
        BasicBlockJumpTargetInTable,
        BasicBlockJumpTargetKindInTable,
        BasicBlockFlagInTable,
        BasicBlockTableFooter,
        BasicBlockILRangeStartInTopHeader,
        BasicBlockILRangeEndInTopHeader,
        SeventeenDashes,
        BasicBlockJumpTargetKindInTopHeader,
        BasicBlockJumpTargetInTopHeader,
        BasicBlockPredInTopHeader,
        BasicBlockSuccInTopHeader,
        StatementILRangeStart,
        StatementILRangeEnd,
        GenTreeLIRNode,
        GenTreeNodeEstimatedTime,
        GenTreeNodeEstimatedCost,
        GenTreeNodeId,
        GenTreeNodeFlags,
        GenTreeNodeKind,
        GenTreeNodeType,
        GenTreeNodeExactType,
        GenTreeNodeVNumber,
        GenTreeNodeTemporaryNumber,
        GenTreeNodeArgumentNumber,
        GenTreeNodeDNumber,
        GenTreeNodeUNumber,
        GenTreeNodeNullConstant,
        GenTreeNodeNesting,
        GenTreeNodeIsInit,
        GenTreeNodeIsCopy,
        GenTreeNodeIsInlineReturn,
        GenTreeNodePred,
        GenTreeNodeIsHandle,
        GenTreeNodeNullcheck,
        GenTreeNodeHelper,
    }
}
