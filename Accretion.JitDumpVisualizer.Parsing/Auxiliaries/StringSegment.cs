using System;
using System.Diagnostics;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    internal readonly struct StringSegment : IEquatable<StringSegment>
    {
        private readonly string _source;
        private readonly int _start;

        public StringSegment(string source, int start, int length)
        {
            Assert.True(length >= 0);
            Assert.True(source.Length >= start + length);

            _source = source;
            _start = start;
            Length = length;
        }

        public static StringSegment Empty => string.Empty;

        public int Length { get; }

        public ReadOnlySpan<char> AsSpan() => AsSpan(0);
        public ReadOnlySpan<char> AsSpan(int start) => AsSpan(start, Length - start);
        public ReadOnlySpan<char> AsSpan(int start, int length) => _source.AsSpan(_start + start, length);

        public override bool Equals(object? obj) => obj is StringSegment segment && Equals(segment);
        public bool Equals(StringSegment other) => AsSpan().SequenceEqual(other.AsSpan());
        public override int GetHashCode() => string.GetHashCode(AsSpan());
        public override string ToString() => AsSpan().ToString();

        public StringSegment Slice(int start) => Slice(start, Length - start);
        public StringSegment Slice(int start, int length) => new StringSegment(_source, _start + start, length);

        public int IndexOf(ReadOnlySpan<char> span) => AsSpan().IndexOf(span);

        public static bool operator ==(StringSegment left, StringSegment right) => left.Equals(right);
        public static bool operator !=(StringSegment left, StringSegment right) => !(left == right);

        public static implicit operator ReadOnlySpan<char>(StringSegment segment) => segment.AsSpan(0);
        public static implicit operator StringSegment(string source) => new StringSegment(source, 0, source.Length);
    }
}
