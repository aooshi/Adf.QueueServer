using System;

namespace Adf.QueueServer
{
    class Hash<T>
    {
        private int capacity = 0;
        //
        private int[] buckets;
        //slots
        private int[] hashCodes;
        private int[] nexts;
        private T[] items;
        //
        private int size = 0;
        private int lastIndex = 0;
        private int freeIndex = -1;
        //
        public uint count = 0;

        //
        public Hash(int capacity)
        {
            this.buckets = new int[capacity];
            this.hashCodes = new int[capacity];
            this.nexts = new int[capacity];
            this.items = new T[capacity];
            this.size = capacity;
            this.capacity = capacity;
        }

        public bool Add(T item)
        {
            int hashCode = Adf.BaseDataConverter.Bits31(item.GetHashCode());
            int bucket = hashCode % this.size;
            for (int i = this.buckets[bucket] - 1; i >= 0; i = this.nexts[i])
            {
                if (this.hashCodes[i] == hashCode && item.Equals(this.items[i]))
                {
                    return false;
                }
            }

            //
            int index;
            if (this.freeIndex >= 0)
            {
                index = this.freeIndex;
                this.freeIndex = this.nexts[index];
            }
            else
            {
                if (this.lastIndex == this.size)
                {
                    ResetCapacity();
                    bucket = hashCode % this.size;
                }
                index = this.lastIndex;
                this.lastIndex++;
            }
            //
            this.hashCodes[index] = hashCode;
            this.items[index] = item;
            this.nexts[index] = this.buckets[bucket] - 1;
            //
            this.buckets[bucket] = index + 1;
            this.count++;
            //
            return true;
        }

        public bool Remove(T item)
        {
            int hashCode = Adf.BaseDataConverter.Bits31(item.GetHashCode());
            int bucket = hashCode % this.size;
            int last = -1;
            for (int i = this.buckets[bucket] - 1; i >= 0; last = i, i = this.nexts[i])
            {
                if (this.hashCodes[i] == hashCode && item.Equals(this.items[i]))
                {
                    //remove
                    if (last < 0)
                    {
                        this.buckets[bucket] = this.nexts[i] + 1;
                    }
                    else
                    {
                        this.nexts[last] = this.nexts[i];
                    }
                    this.hashCodes[i] = -1;
                    this.items[i] = default(T);
                    this.nexts[i] = this.freeIndex;
                    //
                    this.count--;
                    if (this.count == 0)
                    {
                        this.lastIndex = 0;
                        this.freeIndex = -1;
                    }
                    else
                    {
                        this.freeIndex = i;
                    }

                    return true;
                }
            }
            //
            return false;
        }

        private void ResetCapacity()
        {
            int newSize = this.size + (int)(this.capacity * 1.25);

            int[] newHashCodes = new int[newSize];
            int[] newNexts = new int[newSize];
            T[] newItems = new T[newSize];

            //
            Array.Copy(this.hashCodes, 0, newHashCodes, 0, this.lastIndex);
            Array.Copy(this.nexts, 0, newNexts, 0, this.lastIndex);
            Array.Copy(this.items, 0, newItems, 0, this.lastIndex);

            //
            int[] newBuckets = new int[newSize];
            int bucket = 0;
            for (int i = 0; i < this.lastIndex; i++)
            {
                bucket = newHashCodes[i] % newSize;
                newNexts[i] = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }

            //
            this.buckets = newBuckets;
            //
            this.hashCodes = newHashCodes;
            this.nexts = newNexts;
            this.items = newItems;
            //
            this.size = newSize;
        }

        //public bool GetOne(ref UInt64 item)
        //{
        //    for (int i = 0; i < this.lastIndex; i++)
        //    {
        //        if (this.hashCodes[i] >= 0)
        //        {
        //            item = this.items[i];
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        public T[] ToArray()
        {
            int num = 0;
            T[] arr = new T[this.count];
            for (int i = 0; i < this.lastIndex && num < this.count; i++)
            {
                if (this.hashCodes[i] >= 0)
                {
                    arr[num] = this.items[i];
                    num++;
                }
            }
            return arr;
        }
    }
}