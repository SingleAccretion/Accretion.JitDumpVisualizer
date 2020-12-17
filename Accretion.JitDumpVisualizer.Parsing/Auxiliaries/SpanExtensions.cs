using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    internal static class SpanExtensions
    {
        public static SplitSpansEnumerable Split(this ReadOnlySpan<char> span) => new SplitSpansEnumerable(span);

        public ref struct SplitSpansEnumerable
        {
            private readonly ReadOnlySpan<char> _span;

            public SplitSpansEnumerable(ReadOnlySpan<char> span) => _span = span;

            public SplitSpansEnumerator GetEnumerator() => new SplitSpansEnumerator(_span);

            public ref struct SplitSpansEnumerator
            {
                private ReadOnlySpan<char> _remainingSpan;

                public SplitSpansEnumerator(ReadOnlySpan<char> span) : this() => _remainingSpan = span.Trim();

                public bool MoveNext()
                {
                    var endIndex = _remainingSpan.IndexOf(' ');
                    if (endIndex < 0 && !_remainingSpan.IsEmpty)
                    {
                        endIndex = _remainingSpan.Length;
                    }
                    if (endIndex > 0)
                    {
                        Current = _remainingSpan.Slice(0, endIndex);
                        _remainingSpan = _remainingSpan.Slice(endIndex).TrimStart();
                        return true;
                    }

                    return false;
                }

                public ReadOnlySpan<char> Current { get; private set; }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<TEnum>(this ReadOnlySpan<TEnum> span, TEnum value) where TEnum : struct, Enum
        {
            if (Unsafe.SizeOf<TEnum>() == sizeof(long))
            {
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TEnum, long>(ref MemoryMarshal.GetReference(span)), span.Length).Contains(Unsafe.As<TEnum, long>(ref value));
            }
            else if (Unsafe.SizeOf<TEnum>() == sizeof(int))
            {
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TEnum, int>(ref MemoryMarshal.GetReference(span)), span.Length).Contains(Unsafe.As<TEnum, int>(ref value));
            }
            else if (Unsafe.SizeOf<TEnum>() == sizeof(short))
            {
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TEnum, short>(ref MemoryMarshal.GetReference(span)), span.Length).Contains(Unsafe.As<TEnum, short>(ref value));
            }
            else
            {
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TEnum, byte>(ref MemoryMarshal.GetReference(span)), span.Length).Contains(Unsafe.As<TEnum, byte>(ref value));
            }
        }

        public static bool Any<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            foreach (var item in span)
            {
                if (predicate(item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}