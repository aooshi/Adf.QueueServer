using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Adf;
using System.Collections;

namespace Adf.QueueServerClientTest
{
    class QueueServerJsonTest
    {
        //const int SIZE = 25 * 10000;

        const int SIZE = 20;// * 10000;

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

            var client = new Adf.WebSocketClient(host, port, "/queue/json");

            client.Closed += new EventHandler<WebSocketCloseEventArgs>(this.Closed);
            client.Connectioned += new EventHandler(this.Connectioned);
            client.Error += new EventHandler<WebSocketErrorEventArgs>(this.Error);
            client.Message += new EventHandler<WebSocketMessageEventArgs>(this.Message);
            client.Connection();

            this.stopwatch = Stopwatch.StartNew();
            this.receiveCounter = 0;

            var line = "push";

            while (true)
            {
                if (line == "push")
                {
                    this.Push(client);
                }
                else if (line == "pull")
                {
                    this.Pull(client);
                }
                else if (line == "delete")
                {
                    this.Delete(client);
                }
                else if (line == "end")
                {
                    Environment.Exit(0);
                    return;
                }
                else
                {
                    Console.WriteLine("input end/pull/delete");
                }

                line = Console.ReadLine();
            }
        }

        private void Delete(WebSocketClient client)
        {
            this.receiveCounter = 0;
            this.stopwatch.Reset();

            for (int i = 0; i < SIZE; i++)
            {

                var table = new System.Collections.Hashtable();
                table.Add("action", "delete");
                table.Add("queue", "/test/text/1");
                table.Add("requestid", i.ToString());

                var json = Adf.JsonHelper.Serialize(table);

                client.SendAsync(json, null);
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

            for (int i = 0; i < SIZE; i++)
            {
                var table = new System.Collections.Hashtable();
                table.Add("action", "pull");
                table.Add("queue", "/test/text/1");
                table.Add("requestid", i.ToString());
                
                var json = Adf.JsonHelper.Serialize(table);
                client.SendAsync(json, null);
            }

            Console.WriteLine("send {0} pull completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        private void Push(WebSocketClient client)
        {
            this.receiveCounter = 0;
            this.stopwatch.Reset();

            for (int i = 0; i < SIZE; i++)
            {

                var table = new System.Collections.Hashtable();
                table.Add("action", "rpush");
                table.Add("queue", "/test/text/1");
                table.Add("requestid", i.ToString());
                table.Add("body", "rpush" + i);

                var json = Adf.JsonHelper.Serialize(table);

                client.SendAsync(json, null);
                //client.Send(json);
            }

            //client.Send(END);
            //client.SendAsync(END, null);

            Console.WriteLine("send {0} push completed, seconds:{1}, {2} loop/s"
                , SIZE
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }

        int receiveCounter = 0;

        private void Message(object sender, WebSocketMessageEventArgs e)
        {
            if (e.Opcode == WebSocketOpcode.Text)
            {
                this.receiveCounter++;

                var table = Adf.JsonHelper.Deserialize<Hashtable>(e.Message);
                var queue = table["queue"] as string;
                var action = table["action"] as string;

                if (action == "rpush")
                {
                    if (this.receiveCounter == SIZE)
                    {
                        Console.WriteLine("recv {0} push ack, seconds:{1}, {2} loop/s"
                            , SIZE
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            );
                        Console.WriteLine("input pull");
                    }
                    else if (this.receiveCounter % 10000 == 0)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (action == "pull")
                {
                    if (this.receiveCounter == SIZE)
                    {
                        Console.WriteLine("recv {0} pull ack, seconds:{1}, {2} loop/s"
                            , SIZE
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            );
                        Console.WriteLine("input delete");
                    }
                    else if (this.receiveCounter % 10000 == 0)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (action == "delete")
                {
                    if (this.receiveCounter == SIZE)
                    {
                        Console.WriteLine("recv {0} delete ack, seconds:{1}, {2} loop/s"
                            , SIZE
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            );
                        Console.WriteLine("input end to end");
                    }
                    else if (this.receiveCounter % 10000 == 0)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    Console.WriteLine(e.Message);
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