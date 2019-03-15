using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace Adf.QueueServer
{
    class WebSocketBinaryState : IChannel
    {
        private HttpServerWebSocketContext context;
        //
        private List<PullAction> _pulls;
        //
        private bool isAvailable = true;

        //
        public WebSocketBinaryState(HttpServerWebSocketContext context)
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
            var data = action.ToBytes();

            this.Send(data);
        }

        private void Send(byte[] data)
        {
            bool result = true;
            try
            {
                this.context.SendAsync(data, WebSocketOpcode.Binary, this);
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