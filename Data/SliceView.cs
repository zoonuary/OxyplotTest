using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Data
{
    public readonly struct SliceView<T>
    {
        public readonly SliceSegment<T> First;
        public readonly SliceSegment<T> Second;
        public static SliceView<T> Empty { get; } = default;
        public bool IsEmpty => First.Count == 0 && Second.Count == 0;
        public SliceView(SliceSegment<T> first, SliceSegment<T> second)
        {
            First = first;
            Second = second;
        }
    }

    public readonly struct SliceResult<T>
    {
        public readonly SliceSegment<T> Slice;

        public readonly T Left;
        public readonly bool HasLeft;

        public readonly T Right;
        public readonly bool HasRight;

        public readonly bool HasData => !Slice.IsEmpty;

        public static readonly SliceResult<T> Empty = new SliceResult<T>(SliceSegment<T>.Empty, default!, false, default!, false);
        public SliceResult(SliceSegment<T> slice, T left, bool hasLeft, T right, bool hasRight)
        {
            Slice = slice;
            Left = left;
            Right = right;
            HasLeft = hasLeft;
            HasRight = hasRight;
        }
    }

    public readonly struct SliceSegment<T>
    {
        public readonly T[] Array;
        public readonly int Offset;
        public readonly int Count;
        public static SliceSegment<T> Empty { get; } = default;
        public bool IsEmpty => Count == 0;
        public SliceSegment(T[] array, int offset, int count)
        {
            Array = array;
            Offset = offset;
            Count = count;
        }
    }
}
