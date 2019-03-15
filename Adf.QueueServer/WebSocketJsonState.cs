using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace Adf.QueueServer
{
    class WebSocketJsonState : IChannel
    {
        private HttpServerWebSocketContext context;
        //
        private List<PullAction> _pulls;
        //
        private bool isAvailable = true;

        //
        public WebSocketJsonState(HttpServerWebSocketContext context)
        {
            this.context = context;
            this._pulls = new List<PullAction>();
        }

        public bool GetAvailable()
        {
            return this.isAvailable;
        }

        public void Disable()
        {
            this.isAvailable = false;
        }

        public List<PullAction> GetPulls()
        {
            return this._pulls;
        }

        public void Send(Action action)
        {
            var json = Action.ToJson(action);
            var data = System.Text.Encoding.UTF8.GetBytes(json);
            //
            this.Send(data);
        }

        private void Send(byte[] data)
        {
            bool result = true;
            try
            {
                this.context.SendAsync(data, WebSocketOpcode.Text, this);
            }
            catch (ObjectDisposedException)
            {
                //ignore network is closed
                result = false;
            }
            catch (System.Net.Sockets.SocketException)
            {
                //ignore network error
                result = false;
            }
            catch (IOException)
            {
                //ignore network error
                result = false;
            }

            //is not available to clear action
            if (result == false)
            {
                this.isAvailable = false;
            }
        }
    }
}