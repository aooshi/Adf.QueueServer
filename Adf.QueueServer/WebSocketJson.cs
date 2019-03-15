using System;
using Adf.Service;
using System.Threading;
using System.IO;
using System.Collections;

namespace Adf.QueueServer
{
    class WebSocketJson
    {
        LogManager logManager;
        ServiceContext serviceContext;

        public WebSocketJson()
        {
            this.logManager = Program.LogManager;
            this.serviceContext = Program.ServiceContext;
            //
            this.serviceContext.HttpServer.WebSocketConnectioned += new HttpServerWebSocketCallback(WebSocketConnectioned);
            this.serviceContext.HttpServer.WebSocketNewMessage += new HttpServerWebSocketMessage(WebSocketNewMessage);
            //this.serviceContext.HttpServer.WebSocketSendCompleted += new EventHandler<WebSocketSendEventArgs>(WebSocketSendCompleted);
            this.serviceContext.HttpServer.WebSocketDisconnected += new HttpServerWebSocketCallback(WebSocketDisconnected);
        }

        //private void WebSocketSendCompleted(object sender, WebSocketSendEventArgs e)
        //{
        //    //if (e.UserState is WebSocketTextState)
        //    //{
        //    //    var state = (WebSocketTextState)e.UserState;
        //    //    state.SendCommpleted();
        //    //}
        //}

        private void WebSocketConnectioned(HttpServerWebSocketContext context)
        {
            if ("/queue/json".Equals(context.Path))
            {
                System.Threading.Interlocked.Increment(ref Program.ConnectionCounter);

                context.UserState = new WebSocketJsonState(context);
            }
        }

        private void WebSocketDisconnected(HttpServerWebSocketContext context)
        {
            if (context.UserState is WebSocketJsonState)
            {
                System.Threading.Interlocked.Decrement(ref Program.ConnectionCounter);

                this.Disconnection((WebSocketJsonState)context.UserState);

            }
        }

        private void WebSocketNewMessage(HttpServerWebSocketContext context, WebSocketMessageEventArgs args)
        {
            if (context.UserState is WebSocketJsonState)
            {
                if (args.Opcode == WebSocketOpcode.Text)
                {
                    this.ParseText(context, args);
                }
            }
        }

        private void ParseText(HttpServerWebSocketContext context, WebSocketMessageEventArgs args)
        {
            Hashtable table = Adf.JsonHelper.DeserializeBase(args.Message) as Hashtable;
            //
            if (table == null)
            {
                Program.LogManager.Warning.WriteTimeLine("receive a empty packet from " + context.GetRemoteNode());
                return;
            }
            //
            string action = table["action"] as string;
            string queue = table["queue"] as string;
            string id = table["requestid"] as string;
            //
            if (queue == null)
            {
                Program.LogManager.Warning.WriteTimeLine("receive a invalid packet, no set queue from " + context.GetRemoteNode());
            }
            else if (id == null || id == "")
            {
                Program.LogManager.Warning.WriteTimeLine("receive a invalid packet, no set id from " + context.GetRemoteNode());
            }
            else if (action == "pull")
            {
                this.Pull(context, queue, id);
            }
            else if (action == "lpush")
            {
                string body = table["body"] as string;

                this.LPush(context, queue, id, body);
            }
            else if (action == "rpush")
            {
                string body = table["body"] as string;

                this.RPush(context, queue, id, body);
            }
            else if (action == "delete")
            {
                this.Delete(context, queue, id);
            }
            else if (action == "lcancel")
            {
                this.LCancel(context, queue, id);
            }
            else if (action == "rcancel")
            {
                this.RCancel(context, queue, id);
            }
            else if (action == "clear")
            {
                this.Clear(context, queue, id);
            }
            else if (action == "count")
            {
                this.Count(context, queue, id);
            }
            else if (action == "createqueue")
            {
                this.CreateQueue(context, queue, id);
            }
            else if (action == "deletequeue")
            {
                this.DeleteQueue(context, queue, id);
            }
            else
            {
                //ignore
                Program.LogManager.Warning.WriteTimeLine("receive a invalid packet, unknown action " + action + " from " + context.GetRemoteNode());
            }
        }

        private void Disconnection(WebSocketJsonState state)
        {
            var action = new Action();
            action.actionType = Action.DISCONNECT;
            action.channel = (IChannel)state;
            action.queue = "";
            //
            Program.ActionProcessor.Push(action);
        }

        private void CreateQueue(HttpServerWebSocketContext context, string queue, string id)
        {
            var action = new Action();
            action.actionType = Action.CREATEQUEUE;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
        }

        private void DeleteQueue(HttpServerWebSocketContext context, string queue, string id)
        {
            var action = new Action();
            action.actionType = Action.DELETEQUEUE;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
        }

        private void Delete(HttpServerWebSocketContext context, string queue, string id)
        {
            var action = new Action();
            action.actionType = Action.DELETE;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
        }

        private void LCancel(HttpServerWebSocketContext context, string queue, string id)
        {
            var action = new Action();
            action.actionType = Action.LCANCEL;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
        }

        private void RCancel(HttpServerWebSocketContext context, string queue, string id)
        {
            var action = new Action();
            action.actionType = Action.RCANCEL;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
        }

        private void Clear(HttpServerWebSocketContext context, string queue, string id)
        {
            var action = new ClearAction();
            action.actionType = Action.CLEAR;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
        }

        private void Count(HttpServerWebSocketContext context, string queue, string id)
        {
            var action = new CountAction();
            action.actionType = Action.COUNT;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
        }

        private void RPush(HttpServerWebSocketContext context, string queue, string id, string body)
        {
            if (body == null)
            {
                Program.LogManager.Warning.WriteTimeLine("receive a invalid packet, body empty from " + context.GetRemoteNode());
                return;
            }

            //
            var action = new PushAction();
            action.actionType = Action.RPUSH;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            action.item = new DataItem()
            {
                body = System.Text.Encoding.UTF8.GetBytes(body)
                //   , messageId = "" form action processor
            };
            //
            Program.ActionProcessor.Push(action);
        }

        private void LPush(HttpServerWebSocketContext context, string queue, string id, string body)
        {
            if (body == null)
            {
                Program.LogManager.Warning.WriteTimeLine("receive a invalid packet, body empty from " + context.GetRemoteNode());
                return;
            }

            //
            var action = new PushAction();
            action.actionType = Action.LPUSH;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            action.item = new DataItem()
            {
                body = System.Text.Encoding.UTF8.GetBytes(body)
                //   , messageId = "" form action processor
            };
            //
            Program.ActionProcessor.Push(action);
        }

        private void Pull(HttpServerWebSocketContext context, string queue, string id)
        {
            var action = new PullAction();
            action.actionType = Action.PULL;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
        }
    }
}