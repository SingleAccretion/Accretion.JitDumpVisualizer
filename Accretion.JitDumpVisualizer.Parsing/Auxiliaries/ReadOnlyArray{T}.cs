using System;
using System.Collections.Generic;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    public readonly struct ReadOnlyArray<T> : IEquatable<ReadOnlyArray<T>>
    {
        private readonly T[] _array;
        private readonly int _start;

        public ReadOnlyArray(T[] array) : this(array, array.Length) { }

        public ReadOnlyArray(T[] array, int length) : this(array, 0, length) { }

        public ReadOnlyArray(T[] array, int start, int length)
        {
            _array = array;
            _start = start;
            Length = length;
        }

        public int Length { get; }

        public T this[int index] => _array[_start + index];

        public bool Contains(T item)
        {
            foreach (var value in AsSpan())
            {
                if (EqualityComparer<T>.Default.Equals(value, item))
                {
                    return true;
                }
            }

            return false;
        }

        public ReadOnlySpan<T> AsSpan() => _array.AsSpan(_start, Length);
        public ReadOnlyArray<T> Slice(int start) => Slice(start, Length - start);
        public ReadOnlyArray<T> Slice(int start, int length) => new ReadOnlyArray<T>(_array, _start + start, length);
        
        public override bool Equals(object? obj) => obj is ReadOnlyArray<T> array && Equals(array);
        public bool Equals(ReadOnlyArray<T> other) => _array == other._array && _start == other._start && Length == other.Length;
        public override int GetHashCode() => HashCode.Combine(_array, _start, Length);

        public ReadOnlySpan<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

        public static bool operator ==(ReadOnlyArray<T> left, ReadOnlyArray<T> right) => left.Equals(right);
        public static bool operator !=(ReadOnlyArray<T> left, ReadOnlyArray<T> right) => !(left == right);

        public static implicit operator ReadOnlyArray<T>(T[] array) => new ReadOnlyArray<T>(array);
    }
}
