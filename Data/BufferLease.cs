using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Data
{
    public sealed class BufferLease<T> : IDisposable
    {
        public T[] Data { get; private set; }
        public int Length { get; private set; }
        public int Capacity => Data?.Length ?? 0;
        public bool IsPooled { get; private set; }
        private readonly ArrayPool<T> Pool;
        private bool Disposed;

        /// <summary>
        /// ArrayPool은 pool.Rent(length) 시, length 이상의 array를 데이터 배열로 빌려주는 경우도 많음.
        /// 데이터를 채울때 앞쪽에서 다 차있고 length보다 data의 실제 배열 사이즈가 더 크다고 놀라지 말 것.(length보다 긴 index는 null로 채워짐)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="length"></param>
        /// <param name="pool"></param>
        /// <param name="pooled"></param>
        private BufferLease(T[] array, ArrayPool<T> pool, bool pooled)
        {
            Data = array;
            Length = 0;
            Pool = pool;
            IsPooled = pooled;
            Disposed = false;
        }

        public static BufferLease<T> Empty { get; } = new BufferLease<T>(System.Array.Empty<T>(), ArrayPool<T>.Shared, pooled:false);

        public static BufferLease<T> FromPooled(int length)
        {
            //if (length <= 0) return Empty;
            var pool = ArrayPool<T>.Shared;
            var buffer = pool.Rent(length);
            return new BufferLease<T>(buffer, pool, pooled: true);
        }

        public void AppendLease(BufferLease<T> lease)
        {
            if (lease == null || lease.Length <= 0) return;
            if (ReferenceEquals(lease.Data, this.Data)) return; //스스로를 추가할 경우
            EnsureCapacity(Length + lease.Length);
            Array.Copy(lease.Data, 0, Data, Length, lease.Length);
            Length += lease.Length;
        }

        public void AppendRange(T[] src, int srcOffset, int count)
        {
            if (count <= 0) return;
            EnsureCapacity(Length + count);
            Array.Copy(src, srcOffset, Data, Length, count);
            Length += count;
        }

        public void AppendData(T src)
        {
            EnsureCapacity(Length + 1);
            Data[Length] = src;
            Length += 1;
        }

        private void EnsureCapacity(int needed)
        {
            if (needed <= Capacity) return;
            int newCap = Capacity == 0 ? Math.Max(needed, 256) : Math.Max(Capacity * 2, needed);
            var newBuf = Pool.Rent(newCap);
            if (Length > 0) Array.Copy(Data, 0, newBuf, 0, Length);
            if (IsPooled && Data != null) Pool.Return(Data, clearArray: !typeof(T).IsValueType);
            Data = newBuf;
            IsPooled = true;
        }

            
        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            
            if(IsPooled && Data != null)
            {
                Pool.Return(Data, clearArray: !typeof(T).IsValueType);
                Data = null;
                Length = 0;
            }
        }
    }
}
