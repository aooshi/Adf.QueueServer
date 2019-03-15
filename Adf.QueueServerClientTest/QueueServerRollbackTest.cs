using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Adf;

namespace Adf.QueueServerClientTest
{
    class QueueServerRollbackTest
    {
        //const int SIZE = 25 * 10000;

        const int SIZE = 2 * 10000;

        public void Test()
        {
            var host = "127.0.0.1";
            //var host = "192.168.199.13";
            var port = 231;
            var topic = "/test";

            //
            ushort commitTimeout = 2;
            var client = new Adf.QueueServerClient(host, port, topic, commitTimeout);
            client.Timeout = 30;
            Console.WriteLine("connect {0}:{1}", host, port);
            
            client.Connect(5);

            //
            Push(client);
            
            Console.WriteLine("commit timeout: " + commitTimeout + "s");
            Console.WriteLine("loop");
            for(int i=0;i<int.MaxValue;i++)
            {
                Console.WriteLine();

                var msg = client.Pull();
                Console.WriteLine("msgid: {0}, dup: {1}",msg.MessageId, msg.Duplications);

                if (i % 5 == 0)
                {
                    Console.WriteLine("Sleep(1.5s) and commit");
                    System.Threading.Thread.Sleep(1500);
                    client.Commit(msg.MessageId);

                    if (client.Count() == 0)
                    {
                        Push(client);
                    }
                }
                else
                {
                    Console.WriteLine("Sleep(3s)");
                    System.Threading.Thread.Sleep(3000);
                }
            }

        }

        private void Push(QueueServerClient client)
        {
            var msgId = 0UL;
            //
            msgId = client.RPush("1");
            Console.WriteLine("push " + msgId);
            msgId = client.RPush("2");
            Console.WriteLine("push " + msgId);
            msgId = client.RPush("3");
            Console.WriteLine("push " + msgId);
        }
    }
}