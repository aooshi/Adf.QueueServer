using System;
using System.Collections.Generic;
using System.Text;
using Adf;
using System.Diagnostics;

namespace Adf.QueueServerClientTest
{
    class QueueServerReceiveTest
    {
        const int THREAD_SIZE = 32;

        static int total;
        static System.Threading.EventWaitHandle eh;

        public void Test()
        {
            eh = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);

            var client = new Adf.QueueServerClient("192.168.199.30", 231, "/upush/1/outgoing");
            client.Receive(QueueServerReceiveEvent, THREAD_SIZE);

            Console.WriteLine("Thread Size: " + THREAD_SIZE);

            var stopwatch = new Stopwatch();
            stopwatch.Reset();

            System.Threading.ThreadPool.QueueUserWorkItem(q =>
            {

                while (true)
                {
                    System.Threading.Thread.Sleep(5000);
                    Console.WriteLine("total:{0}, seconds:{1}, {2} loop/s"
                        , total
                        , (double)(stopwatch.ElapsedMilliseconds / 1000)
                        , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                        );
                }
            });


            stopwatch.Start();

            total = 0;
            eh.WaitOne();

            stopwatch.Stop();
            Console.WriteLine("total:{0}, seconds:{1}, {2} loop/s"
                , total
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
            client.Dispose();
        }

        static QueueServerReceiveOption QueueServerReceiveEvent(QueueServerClient client, QueueServerMessageAckArgs args)
        {
            var t = System.Threading.Interlocked.Increment(ref total);
            if (t >= 1000000)
            {
                eh.Set();
            }

            return QueueServerReceiveOption.Nothing;
        }
    }
}
