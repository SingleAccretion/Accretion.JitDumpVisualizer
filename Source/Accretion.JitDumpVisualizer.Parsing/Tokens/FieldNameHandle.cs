using System;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public struct FieldNameHandle : IEquatable<FieldNameHandle>
    {
        public FieldNameHandle(uint value) => Value = value;

        public uint Value { get; }

        public override bool Equals(object? obj) => obj is FieldNameHandle handle && Equals(handle);
        public bool Equals(FieldNameHandle other) => Value == other.Value;
        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(FieldNameHandle left, FieldNameHandle right) => left.Equals(right);
        public static bool operator !=(FieldNameHandle left, FieldNameHandle right) => !(left == right);
    }
}