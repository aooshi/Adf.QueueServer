using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Adf;
using System.Collections;

namespace Adf.QueueServerClientTest
{
    class QueueServerBinaryTest
    {
        //const int SIZE = 25 * 10000;

        const int SIZE = 20 * 10000;
        //const int SIZE = 20;

        const string END = "END";

        Stopwatch stopwatch;

        public void Test()
        {
            var host = "";
            host = "127.0.0.1";
            //host = "192.168.199.13";
            //host = "192.168.199.30";

            var port = 6230;

            //var ws = new WebSocketHandler(port);

            var client = new Adf.WebSocketClient(host, port, "/queue/bin");

            client.Closed += new EventHandler<WebSocketCloseEventArgs>(this.Closed);
            client.Connectioned += new EventHandler(this.Connectioned);
            client.Error += new EventHandler<WebSocketErrorEventArgs>(this.Error);
            client.Message += new EventHandler<WebSocketMessageEventArgs>(this.Message);
            client.Connection();

            this.stopwatch = Stopwatch.StartNew();
            this.receiveCounter = 0;

            var line = "";

            while (true)
            {
                if (line == "rpush" || line == "r")
                {
                    this.RPush(client);
                }
                else if (line == "lpush" || line == "l")
                {
                    this.LPush(client);
                }
                else if (line == "pull" || line == "p")
                {
                    this.Pull(client);
                }
                else if (line == "delete" || line == "d")
                {
                    this.Delete(client);
                }
                else if (line == "clear")
                {
                    this.Clear(client);
                }
                else if (line == "count")
                {
                    this.Count(client);
                }
                else if (line == "cq")
                {
                    this.CreateQ(client);
                }
                else if (line == "dq")
                {
                    this.DeleteQ(client);
                }
                else if (line == "end")
                {
                    Environment.Exit(0);
                    return;
                }
                else
                {
                    Console.WriteLine("input cq/de/rpush/lpush/pull/delete/clear/count/end");
                }

                line = Console.ReadLine();
            }
        }

        private void CreateQ(WebSocketClient client)
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            var queue = "/test/text/1";
            var id = "1";
            //
            var packet = Adf.QueueServerEncoder.CreateQueue(queue, id);

            client.SendAsync(packet, null);
            //client.Send(json);

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} delete completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void DeleteQ(WebSocketClient client)
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            var queue = "/test/text/1";
            var id = "1";
            //
            var packet = Adf.QueueServerEncoder.DeleteQueue(queue, id);

            client.SendAsync(packet, null);
            //client.Send(json);

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} delete completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void Count(WebSocketClient client)
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = "/test/text/1";
                var id = i.ToString();
                //
                var packet = Adf.QueueServerEncoder.Count(queue, id);

                client.SendAsync(packet, null);
                //client.Send(json);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} delete completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void Clear(WebSocketClient client)
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = "/test/text/1";
                var id = i.ToString();
                //
                var packet = Adf.QueueServerEncoder.Clear(queue, id);

                client.SendAsync(packet, null);
                //client.Send(json);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} delete completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void Delete(WebSocketClient client)
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = "/test/text/1";
                var id = i.ToString();
                //
                var packet = Adf.QueueServerEncoder.Delete(queue, id);

                client.SendAsync(packet, null);
                //client.Send(json);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} delete completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void Pull(WebSocketClient client)
        {
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = "/test/text/1";
                var id = i.ToString();
                //
                var packet = Adf.QueueServerEncoder.Pull(queue, id);

                client.SendAsync(packet, null);
            }

            Console.WriteLine("send {0} pull completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void RPush(WebSocketClient client)
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = "/test/text/1";
                var id = i.ToString();
                var body = System.Text.Encoding.UTF8.GetBytes("rpush" + i);
                //
                var packet = Adf.QueueServerEncoder.RPush(queue, id, body);

                client.SendAsync(packet, null);
                //client.Send(json);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} rpush completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void LPush(WebSocketClient client)
        {
            this.resultSuccess = 0;
            this.receiveCounter = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();

            for (int i = 0; i < SIZE; i++)
            {
                var queue = "/test/text/1";
                var id = i.ToString();
                var body = System.Text.Encoding.UTF8.GetBytes("rpush" + i);
                //
                var packet = Adf.QueueServerEncoder.LPush(queue, id, body);

                client.SendAsync(packet, null);
                //client.Send(json);
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

        private void Message(object sender, WebSocketMessageEventArgs e)
        {
            if (e.Opcode == WebSocketOpcode.Binary)
            {
                this.receiveCounter++;

                var result = Adf.QueueServerEncoder.Decode(e.Buffer);

                //var table = Adf.JsonHelper.Deserialize<Hashtable>(e.Message);
                //var queue = table["queue"] as string;
                //var action = table["action"] as string;

                if (result.Action == QueueServerAction.RPUSH)
                {
                    if (this.receiveCounter == SIZE)
                    {
                        Console.WriteLine("recv {0} rpush ack,success:{3}, seconds:{1}, {2} loop/s"
                            , SIZE
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , this.resultSuccess);
                        Console.WriteLine("input cq/dq/rpush/lpush/pull/delete/clear/count/end");
                    }
                    //else if (this.receiveCounter % 10000 == 0)
                    //{
                    //    Console.WriteLine(e.Message);
                    //}
                }
                else if (result.Action == QueueServerAction.LPUSH)
                {
                    if (this.receiveCounter == SIZE)
                    {
                        Console.WriteLine("recv {0} lpush ack,success:{3}, seconds:{1}, {2} loop/s"
                            , SIZE
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , this.resultSuccess);
                        Console.WriteLine("input cq/dq/rpush/lpush/pull/delete/clear/count/end");
                    }
                    //else if (this.receiveCounter % 10000 == 0)
                    //{
                    //    Console.WriteLine(e.Message);
                    //}
                }
                else if (result.Action == QueueServerAction.PULL)
                {
                    if (this.receiveCounter == SIZE)
                    {
                        Console.WriteLine("recv {0} pull ack, success:{3}, seconds:{1}, {2} loop/s"
                            , SIZE
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , this.resultSuccess
                            );
                        Console.WriteLine("input cq/dq/rpush/lpush/pull/delete/clear/count/end");
                    }
                    //else if (this.receiveCounter % 10000 == 0)
                    //{
                    //    Console.WriteLine(e.Message);
                    //}
                    else
                    {
                        Console.WriteLine(result.GetBodyString());
                    }
                }
                else if (result.Action == QueueServerAction.DELETE)
                {
                    if (this.receiveCounter == SIZE)
                    {
                        Console.WriteLine("recv {0} delete ack, success:{3}, seconds:{1}, {2} loop/s"
                            , SIZE
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , this.resultSuccess
                            );
                        Console.WriteLine("input cq/dq/rpush/lpush/pull/delete/clear/count/end");
                    }
                    //else if (this.receiveCounter % 10000 == 0)
                    //{
                    //    Console.WriteLine(e.Message);
                    //}
                }
                else if (result.Action == QueueServerAction.COUNT)
                {
                    if (this.receiveCounter == SIZE)
                    {
                        Console.WriteLine("recv {0} count ack, success:{3}, seconds:{1}, {2} loop/s"
                            , SIZE
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , this.resultSuccess
                            );
                        Console.WriteLine("input cq/dq/rpush/lpush/pull/delete/clear/count/end");
                    }
                    //else if (this.receiveCounter % 10000 == 0)
                    //{
                    //    Console.WriteLine(e.Message);
                    //}
                }
                else if (result.Action == QueueServerAction.CLEAR)
                {
                    if (this.receiveCounter == SIZE)
                    {
                        Console.WriteLine("recv {0} clear ack, success:{3}, seconds:{1}, {2} loop/s"
                            , SIZE
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , this.resultSuccess
                            );
                        Console.WriteLine("input cq/dq/rpush/lpush/pull/delete/clear/count/end");
                    }
                    //else if (this.receiveCounter % 10000 == 0)
                    //{
                    //    Console.WriteLine(e.Message);
                    //}
                }
                else
                {
                    Console.WriteLine(e.Message);
                }

                //
                if (result.Result == QueueServerAction.OK)
                {
                    this.resultSuccess++;
                }
                else
                {
                    Console.WriteLine(result.Result);
                }

                //if (e.Message == END)
                //{
                //    Console.WriteLine("recv {0} completed, seconds:{1}, {2} loop/s"
                //        , SIZE
                //        , (double)(stopwatch.ElapsedMilliseconds / 1000)
                //        , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                //        );
                //}
            }
        }

        private void Error(object sender, WebSocketErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.Exception.Message);
        }

        private void Connectioned(object sender, EventArgs e)
        {
            WebSocketClient client = (WebSocketClient)sender;
            Console.WriteLine("Connected: {0}:{1}", client.Host, client.Port);
        }

        private void Closed(object sender, WebSocketCloseEventArgs e)
        {
            Console.WriteLine("Closed: " + e.Reason);
        }
    }
}