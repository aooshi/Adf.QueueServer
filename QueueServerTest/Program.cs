using System;

namespace QueueServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestQueue();
        }

        private static void TestQueue()
        {
            ulong size = 100 * 10000;
            //size = 15;

            while (true)
            {
                var queue = new Queue();

                for (ulong i = 0; i < size; i++)
                {
                    //queue.Push(new Item() { id = i });
                    //queue.Restore(new Item() { id = i });


                    //queue.Push(i);
                    queue.Restore(i);
                }


                for (ulong i = size; i < size * 2; i++)
                {
                    //queue.Push(new Item() { id = i });
                    //queue.Restore(new Item() { id = i });


                    queue.Push(i);
                    //queue.Restore(i);
                }

                //                for (ulong i = 0; i < size; i++)
                while (queue.count > 0)
                {
                    //var item = queue.Pull();
                    //if (item.id != i)
                    //{
                    //    Console.WriteLine(item.id + ":" + i);
                    //}

                    //还原 restore
                    for (ulong i = size - 1; i >= 0; i--)
                    {
                        var item = queue.Pull();
                        if (item != i)
                        {
                            Console.WriteLine("R:" + item + ":" + i);
                        }

                        if (i == 0)
                            break;
                    }

                    //还原 push
                    for (ulong i = size; i < size * 2; i++)
                    {
                        var item = queue.Pull();
                        if (item != i)
                        {
                            Console.WriteLine("P:"+item + ":" + i);
                        }
                    }
                }

                queue.Push(2);
                queue.Restore(1);
                queue.Push(3);

                Console.WriteLine("1:" + queue.Pull());
                Console.WriteLine("2:" + queue.Pull());
                Console.WriteLine("3:" + queue.Pull());

                Console.WriteLine("complete " + size);
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}