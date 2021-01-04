using System;

namespace Accretion.JitDumpVisualizer.Parsing.Tokens
{
    public struct MethodNameHandle : IEquatable<MethodNameHandle>
    {
        public MethodNameHandle(uint value) => Value = value;

        public uint Value { get; }

        public override bool Equals(object? obj) => obj is MethodNameHandle handle && Equals(handle);
        public bool Equals(MethodNameHandle other) => Value == other.Value;
        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(MethodNameHandle left, MethodNameHandle right) => left.Equals(right);
        public static bool operator !=(MethodNameHandle left, MethodNameHandle right) => !(left == right);
    }
}