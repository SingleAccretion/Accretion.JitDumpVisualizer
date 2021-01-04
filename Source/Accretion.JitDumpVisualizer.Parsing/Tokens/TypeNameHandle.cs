using System;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public struct TypeNameHandle : IEquatable<TypeNameHandle>
    {
        public TypeNameHandle(uint value) => Value = value;

        public uint Value { get; }

        public override bool Equals(object? obj) => obj is TypeNameHandle handle && Equals(handle);
        public bool Equals(TypeNameHandle other) => Value == other.Value;
        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(TypeNameHandle left, TypeNameHandle right) => left.Equals(right);
        public static bool operator !=(TypeNameHandle left, TypeNameHandle right) => !(left == right);
    }
}