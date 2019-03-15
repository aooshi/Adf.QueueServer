using System;
using Adf.Service;
using System.Threading;
using System.IO;
using System.Collections;

namespace Adf.QueueServer
{
    class WebSocketBinary
    {
        LogManager logManager;
        ServiceContext serviceContext;

        public WebSocketBinary()
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
            if ("/queue/bin".Equals(context.Path))
            {
                System.Threading.Interlocked.Increment(ref Program.ConnectionCounter);

                context.UserState = new WebSocketBinaryState(context);
            }
        }

        private void WebSocketDisconnected(HttpServerWebSocketContext context)
        {
            if (context.UserState is WebSocketBinaryState)
            {
                System.Threading.Interlocked.Decrement(ref Program.ConnectionCounter);

                this.Disconnection((WebSocketBinaryState)context.UserState);
            }
        }

        private void WebSocketNewMessage(HttpServerWebSocketContext context, WebSocketMessageEventArgs args)
        {
            if (context.UserState is WebSocketBinaryState)
            {
                if (args.Opcode == WebSocketOpcode.Binary)
                {
                    this.ParseBinary(context, args);
                }
            }
        }

        private void ParseBinary(HttpServerWebSocketContext context, WebSocketMessageEventArgs args)
        {
            var buffer = args.Buffer;
            var bufferLength = buffer.Length;

            if (bufferLength == 0)
            {
                Program.LogManager.Warning.WriteTimeLine("receive a empty packet from " + context.GetRemoteNode());
                return;
            }

            //struct
            /*
             * action
             * id
             * queue
             * 
             */

            int length = 0;
            int position = 0;

            byte action = 0;
            string id = null;
            string queue = null;

            //action
            action = buffer[0];
            position = 1;
            //id
            length = Adf.BaseDataConverter.ToUInt16(buffer, position);
            position += 2;
            id = System.Text.Encoding.ASCII.GetString(buffer, position, length);
            position += length;
            //queue
            length = Adf.BaseDataConverter.ToUInt16(buffer, position);
            position += 2;
            queue = System.Text.Encoding.ASCII.GetString(buffer, position, length);
            position += length;

            //
            if (queue == null)
            {
                Program.LogManager.Warning.WriteTimeLine("receive a invalid packet, no set queue from " + context.GetRemoteNode());
            }
            else if (id == null || id == "")
            {
                Program.LogManager.Warning.WriteTimeLine("receive a invalid packet, no set id from " + context.GetRemoteNode());
            }
            else if (action == Action.PULL)
            {
                this.Pull(context, queue, id);
            }
            else if (action == Action.LPUSH)
            {
                length = Adf.BaseDataConverter.ToUInt16(buffer, position);
                position += 2;
                var body = new byte[length];
                Array.Copy(buffer, position, body, 0, length);
                //position += length;

                this.LPush(context, queue, id, body);
            }
            else if (action == Action.RPUSH)
            {
                length = Adf.BaseDataConverter.ToUInt16(buffer, position);
                position += 2;
                var body = new byte[length];
                Array.Copy(buffer, position, body, 0, length);
                //position += length;

                this.RPush(context, queue, id, body);
            }
            else if (action == Action.DELETE)
            {
                this.Delete(context, queue, id);
            }
            else if (action == Action.LCANCEL)
            {
                this.LCancel(context, queue, id);
            }
            else if (action == Action.RCANCEL)
            {
                this.RCancel(context, queue, id);
            }
            else if (action == Action.COUNT)
            {
                this.Count(context, queue, id);
            }
            else if (action == Action.CLEAR)
            {
                this.Clear(context, queue, id);
            }
            else if (action == Action.CREATEQUEUE)
            {
                this.CreateQueue(context, queue, id);
            }
            else if (action == Action.DELETEQUEUE)
            {
                this.DeleteQueue(context, queue, id);
            }
            else
            {
                //ignore
                Program.LogManager.Warning.WriteTimeLine("receive a invalid packet, unknown action " + action + " from " + context.GetRemoteNode());
            }
        }

        private void Disconnection(WebSocketBinaryState state)
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

        private void RPush(HttpServerWebSocketContext context, string queue, string id, byte[] body)
        {
            var action = new PushAction();
            action.actionType = Action.RPUSH;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            action.item = new DataItem()
            {
                body = body
                //,messageId = "" , from action processor
            };
            //
            Program.ActionProcessor.Push(action);
        }

        private void LPush(HttpServerWebSocketContext context, string queue, string id, byte[] body)
        {
            var action = new PushAction();
            action.actionType = Action.LPUSH;
            action.channel = (IChannel)context.UserState;
            action.queue = queue;
            action.id = id;
            action.item = new DataItem()
            {
                body = body
                //,messageId = "" , from action processor
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