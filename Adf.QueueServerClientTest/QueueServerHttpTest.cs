using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Adf;
using System.Collections;

namespace Adf.QueueServerClientTest
{


    class QueueServerHttpTest
    {

        const string TOPIC = "/test/text/1";

        //const int SIZE = 25 * 10000;

        const int SIZE = 2 * 10000;

        const string END = "END";

        Stopwatch stopwatch;
        string host = "";
        int port = 80;

        public void Test()
        {
            host = "127.0.0.1";
            //host = "192.168.199.13";
            //host = "192.168.199.30";

            port = 6230;

            this.stopwatch = Stopwatch.StartNew();
            this.receiveCounter = 0;

            var line = "";

            while (true)
            {
                if (line == "rpush" || line == "r")
                {
                    this.RPush();
                }
                else if (line == "lpush" || line == "l")
                {
                    this.LPush();
                }
                else if (line == "pull" || line == "p")
                {
                    this.Pull();
                }
                else if (line == "delete" || line == "d")
                {
                    this.Delete();
                }
                else if (line == "clear")
                {
                    this.Clear();
                }
                else if (line == "count")
                {
                    this.Count();
                }
                else if (line == "end")
                {
                    Environment.Exit(0);
                    return;
                }
                else
                {
                    Console.WriteLine("input rpush/lpush/pull/delete/clear/end");
                }

                line = Console.ReadLine();
            }
        }

        private void Count()
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = TOPIC;
                var id = i.ToString();
                //
                var url = "http://" + this.host + ":" + this.port + "/queue/count?queue=" + queue + "&requestid=" + i.ToString();

                var data = Adf.HttpClient.Instance.GetString(url);
                this.Message(data);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} delete completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void Clear()
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = TOPIC;
                var id = i.ToString();
                //
                var url = "http://" + this.host + ":" + this.port + "/queue/clear?queue=" + queue + "&requestid=" + i.ToString();

                var data = Adf.HttpClient.Instance.GetString(url);
                this.Message(data);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} delete completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void Delete()
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = TOPIC;
                var id = i.ToString();
                //
                var url = "http://" + this.host + ":" + this.port + "/queue/delete?queue=" + queue + "&requestid=" + i.ToString();

                var data = Adf.HttpClient.Instance.GetString(url);
                this.Message(data);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} delete completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void Pull()
        {
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = TOPIC;
                var id = i.ToString();
                //
                var url = "http://" + this.host + ":" + this.port + "/queue/pull?queue=" + queue + "&requestid=" + i.ToString();

                var hc = new Adf.HttpClient();
                //set timeout
                hc.Timeout = System.Threading.Timeout.Infinite;
                var data = hc.GetString(url);

                this.Message(data);
            }

            Console.WriteLine("send {0} pull completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void RPush()
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = TOPIC;
                var id = i.ToString();
                var body = "rpush" + i;
                //
                var url = "http://" + this.host + ":" + this.port + "/queue/rpush?queue=" + queue + "&requestid=" + i.ToString();

                var data = Adf.HttpClient.Instance.Post(url, body, "application/octet-stream");
                this.Message(data);
                //Console.WriteLine("a:" + stopwatch.ElapsedMilliseconds);
                //Console.WriteLine(data);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} rpush completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void LPush()
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = TOPIC;
                var id = i.ToString();
                var body = "lpush" + i;
                //
                var url = "http://" + this.host + ":" + this.port + "/queue/rpush?queue=" + queue + "&requestid=" + i.ToString();

                var data = Adf.HttpClient.Instance.Post(url, body, "application/octet-stream");
                this.Message(data);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} lpush completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        int receiveCounter = 0;
        int resultSuccess = 0;

        private void Message(string message)
        {
            var table = Adf.JsonHelper.Deserialize<Hashtable>(message);
            var queue = table["queue"] as string;
            var result = table["result"] as string;
            var action = table["action"] as string;

            if (action == "rpush")
            {
                if (this.receiveCounter == SIZE)
                {
                    Console.WriteLine("input rpush/lpush/pull/delete/clear/count/end");
                }
                else if (this.receiveCounter % 10000 == 0)
                {
                    Console.WriteLine(message);
                }
            }
            else if (action == "lpush")
            {
                if (this.receiveCounter == SIZE)
                {
                    Console.WriteLine("input rpush/lpush/pull/delete/clear/count/end");
                }
                else if (this.receiveCounter % 10000 == 0)
                {
                    Console.WriteLine(message);
                }
            }
            else if (action == "pull")
            {
                if (this.receiveCounter == SIZE)
                {
                    Console.WriteLine("input rpush/lpush/pull/delete/clear/count/end");
                }
                else if (this.receiveCounter % 10000 == 0)
                {
                    Console.WriteLine(message);
                }
            }
            else if (action == "delete")
            {
                if (this.receiveCounter == SIZE)
                {
                    Console.WriteLine("input rpush/lpush/pull/delete/clear/count/end");
                }
                else if (this.receiveCounter % 10000 == 0)
                {
                    Console.WriteLine(message);
                }
            }
            else if (action == "count")
            {
                if (this.receiveCounter == SIZE)
                {
                    Console.WriteLine("input rpush/lpush/pull/delete/clear/count/end");
                }
                else if (this.receiveCounter % 10000 == 0)
                {
                    Console.WriteLine(message);
                }
            }
            else if (action == "clear")
            {
                if (this.receiveCounter == SIZE)
                {
                    Console.WriteLine("input rpush/lpush/pull/delete/clear/count/end");
                }
                else if (this.receiveCounter % 10000 == 0)
                {
                    Console.WriteLine(message);
                }
            }
            else
            {
                Console.WriteLine(message);
            }


            if (result == QueueServerAction.OK)
            {
                this.resultSuccess++;
            }
            else
            {
                Console.WriteLine(result);
            }
        }
    }
}