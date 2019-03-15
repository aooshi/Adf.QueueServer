using System;

namespace Adf.QueueServer
{
    class Queue<T>
    {
        private const int CAPACITY = 128;
        private const float FACTOR = 1.25f;
        //private const int CAPACITY = 128;
        //private const int MAX_CAPACITY = 10000;

        private T[] items;
        private int length;
        private int head;
        private int tail;
        public int count;

        public Queue()
        {
            this.items = new T[CAPACITY];
            this.length = CAPACITY;
            this.head = 0;
            this.tail = 0;
            this.count = 0;
        }

        public void Clear()
        {
            this.items = new T[CAPACITY];
            this.length = CAPACITY;
            this.head = 0;
            this.tail = 0;
            this.count = 0;
        }

        public void LPush(T item)
        {
            if (this.count == this.length)
            {
                //uint newSize = 0;
                //if (this.length / 2 > MAX_CAPACITY)
                //    newSize = this.length + MAX_CAPACITY;
                //else
                //    newSize = this.length * 2;

                int newSize = (int)(this.length * FACTOR);

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
                //this.head--;
                //if (this.head < 0)
                //    this.head = this.length - 1;

                if ((int)(this.head - 1) < 0)
                    this.head = this.length - 1;
                else
                    this.head--;

                this.items[this.head] = item;
            }
            this.count++;
        }

        public void RPush(T item)
        {
            if (this.count == this.length)
            {
                //uint newSize = 0;
                //if (this.length / 2 > MAX_CAPACITY)
                //    newSize = this.length + MAX_CAPACITY;
                //else
                //    newSize = this.length * 2;
                
                int newSize = (int)(this.length * FACTOR);

                this.SetCapacity(newSize);
            }

            this.items[this.tail] = item;
            this.tail = (this.tail + 1) % this.length;
            this.count++;
        }

        public T Pull()
        {
            var item = this.items[this.head];
            this.items[this.head] = default(T);
            this.head = (this.head + 1) % this.length;
            this.count--;
            return item;
        }

        private void SetCapacity(int capacity)
        {
            T[] newItems = new T[capacity];
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