using System;

namespace QueueServerTest
{
    //class Item
    //{
    //    public int id;
    //}

    class Queue
    {
        private const int CAPACITY = 5;
        private const int MAX_CAPACITY = 1000;

        private UInt64[] items;
        private int length;
        private int head;
        private int tail;
        public int count;

        public Queue()
        {
            this.items = new UInt64[CAPACITY];
            this.length = CAPACITY;
            this.head = 0;
            this.tail = 0;
            this.count = 0;
        }

        public void Clear()
        {
            this.items = new UInt64[CAPACITY];
            this.length = CAPACITY;
            this.head = 0;
            this.tail = 0;
            this.count = 0;
        }

        public void Restore(UInt64 item)
        {
            if (this.count == this.length)
            {
                int newSize = 0;
                if (this.length / 2 > MAX_CAPACITY)
                    newSize = this.length + MAX_CAPACITY;
                else
                    newSize = this.length * 2;

                this.SetCapacity(newSize);
            }

            //
            if (this.count == 0)
            {
                this.items[this.tail] = item;
                this.tail = (this.tail + 1) % this.length;
            }
            else
            {
                this.head--;
                if (this.head < 0)
                    this.head = this.length - 1;

                this.items[this.head] = item;
            }
            this.count++;
        }

        public void Push(UInt64 item)
        {
            if (this.count == this.length)
            {
                int newSize = 0;
                if (this.length / 2 > MAX_CAPACITY)
                    newSize = this.length + MAX_CAPACITY;
                else
                    newSize = this.length * 2;
                
                this.SetCapacity(newSize);
            }

            this.items[this.tail] = item;
            this.tail = (this.tail + 1) % this.length;
            this.count++;
        }

        public UInt64 Pull()
        {
            var item = this.items[this.head];
            this.items[this.head] = 0;
            this.head = (this.head + 1) % this.length;
            this.count--;
            return item;
        }

        private void SetCapacity(int capacity)
        {
            UInt64[] newItems = new UInt64[capacity];
            if (this.count > 0)
            {
                if (this.head < this.tail)
                {
                    Array.Copy(this.items, this.head, newItems, 0, this.count);
                }
                else
                {
                    Array.Copy(this.items, this.head, newItems, 0, this.length - this.head);
                    Array.Copy(this.items, 0, newItems, this.length - this.head, this.tail);
                }
            }

            this.items = newItems;
            this.head = 0;
            this.tail = (this.count == capacity) ? 0 : this.count;
            this.length = capacity;
        }
    }
}