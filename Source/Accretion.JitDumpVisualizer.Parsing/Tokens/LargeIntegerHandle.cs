using System;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public struct LargeIntegerHandle : IEquatable<LargeIntegerHandle>
    {
        public LargeIntegerHandle(uint value) => Value = value;

        public uint Value { get; }

        public override bool Equals(object? obj) => obj is LargeIntegerHandle handle && Equals(handle);
        public bool Equals(LargeIntegerHandle other) => Value == other.Value;
        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(LargeIntegerHandle left, LargeIntegerHandle right) => left.Equals(right);
        public static bool operator !=(LargeIntegerHandle left, LargeIntegerHandle right) => !(left == right);
    }
}