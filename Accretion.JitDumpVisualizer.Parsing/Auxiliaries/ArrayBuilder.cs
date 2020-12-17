using System;
using System.Diagnostics;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    public struct ArrayBuilder<T>
    {
        public const int InitialCapacity = 4;
        public const float DefaultGrowthFactor = 1.5f;

        private T[] _array;
        private float _growthFactor;

        public ArrayBuilder(int capacity = InitialCapacity) : this()
        {
            _array = new T[capacity];
            _growthFactor = DefaultGrowthFactor;
        }

        public int Count { get; private set; }
        public float GrowthFactor => _growthFactor != 0 ? _growthFactor : _growthFactor = DefaultGrowthFactor;
        public int Capacity => _array.Length;

        public void Add(T item)
        {
            EnsureCapacity();
            _array[Count] = item;
            Count++;
        }

        private void EnsureCapacity()
        {
            if (_array is null)
            {
                _array = new T[InitialCapacity];
            }
            if (Count == Capacity)
            {
                var newCapacity = (int)(Capacity * GrowthFactor);
                Array.Resize(ref _array, newCapacity);
            }
        }

        public ReadOnlyArray<T> AsReadOnlyArray()
        {
            Debug.Assert(_array is not null);

            return new ReadOnlyArray<T>(_array, Count);
        }
    }
}
