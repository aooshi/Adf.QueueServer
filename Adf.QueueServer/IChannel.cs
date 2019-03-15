using System;
using System.Collections.Generic;

namespace Adf.QueueServer
{
    interface IChannel
    {
        bool GetAvailable();
        void Disable();

        List<PullAction> GetPulls();

        void Send(Action action);
    }
}