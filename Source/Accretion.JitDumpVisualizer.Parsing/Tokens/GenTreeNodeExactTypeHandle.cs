using System;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public struct GenTreeNodeExactTypeHandle : IEquatable<GenTreeNodeExactTypeHandle>
    {
        public GenTreeNodeExactTypeHandle(int value) => Value = value;

        public int Value { get; }

        public override bool Equals(object? obj) => obj is GenTreeNodeExactTypeHandle handle && Equals(handle);
        public bool Equals(GenTreeNodeExactTypeHandle other) => Value == other.Value;
        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(GenTreeNodeExactTypeHandle left, GenTreeNodeExactTypeHandle right) => left.Equals(right);
        public static bool operator !=(GenTreeNodeExactTypeHandle left, GenTreeNodeExactTypeHandle right) => !(left == right);
    }
}