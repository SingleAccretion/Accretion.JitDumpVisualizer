using System;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    internal readonly struct Token : IEquatable<Token>
    {
        public const string OneHundredAndThirySevenDashes = "-----------------------------------------------------------------------------------------------------------------------------------------";

        // This ensures things stay in registers but pays for that in constructor size
        private readonly ulong _token;

        public Token(TokenKind kind) => _token = (ulong)kind;

        public Token(TokenKind kind, uint rawValue) => _token = (ulong)rawValue << 32 | (ulong)kind;

        public TokenKind Kind => (TokenKind)_token;
        private uint RawValue => (uint)(_token >> 32);

        public override string? ToString()
        {
            var stringification = $"{Kind}";
            object? value = Kind switch
            {
                TokenKind.Integer or
                TokenKind.BasicBlockInTable or
                TokenKind.BasicBlockInInnerHeader or
                TokenKind.BasicBlockInTopHeader or
                TokenKind.BasicBlockPredInTable or
                TokenKind.BasicBlockJumpTargetInTable or
                TokenKind.BasicBlockJumpTargetInTopHeader or
                TokenKind.Statement or
                TokenKind.BasicBlockRefCountInTable or
                TokenKind.BasicBlockTryCountInTable or
                TokenKind.BasicBlockHandleCountInTable or
                TokenKind.BasicBlockPredInTopHeader or
                TokenKind.BasicBlockSuccInTopHeader or
                TokenKind.GenTreeNodeEstimatedTime or
                TokenKind.GenTreeNodeEstimatedCost or
                TokenKind.GenTreeNodeId or
                TokenKind.GenTreeNodeVNumber or
                TokenKind.GenTreeNodeTemporaryNumber or
                TokenKind.GenTreeNodeArgumentNumber or
                TokenKind.GenTreeNodeNesting => RawValue,
                TokenKind.BasicBlockWeightInTable => BitConverter.Int32BitsToSingle((int)RawValue),
                TokenKind.BasicBlockJumpTargetKindInTable or 
                TokenKind.BasicBlockJumpTargetKindInTopHeader => (BasicBlockJumpTargetKind)RawValue,
                TokenKind.BasicBlockFlagInTable => (BasicBlockFlag)RawValue,
                TokenKind.StartingPhase or TokenKind.FinishingPhase => (RyuJitPhase)RawValue,
                TokenKind.StartingFunction => (RyuJitFunction)RawValue,
                TokenKind.GenTreeNodeFlags => (GenTreeNodeFlags)RawValue,
                TokenKind.GenTreeNodeKind => (GenTreeNodeKind)RawValue,
                TokenKind.GenTreeNodeType => (GenTreeNodeType)RawValue,
                TokenKind.InlineStartingAt => $"[{RawValue:000000}]",
                TokenKind.BasicBlockILRangeStartInTable or
                TokenKind.BasicBlockILRangeStartInTopHeader or
                TokenKind.StatementILRangeStart or
                TokenKind.BasicBlockILRangeEndInTable or
                TokenKind.BasicBlockILRangeEndInTopHeader or
                TokenKind.StatementILRangeEnd => $"{((int)RawValue < 0 ? "???" : $"{RawValue:000}")}",
                _ => null
            };
            if (value is not null)
            {
                stringification += $": {value}";
            }

            return stringification;
        }

        public override bool Equals(object? obj) => obj is Token token && Equals(token);
        public bool Equals(Token other) => _token == other._token;
        public override int GetHashCode() => HashCode.Combine(_token);

        public static bool operator ==(Token left, Token right) => left.Equals(right);
        public static bool operator !=(Token left, Token right) => !(left == right);
    }
}
