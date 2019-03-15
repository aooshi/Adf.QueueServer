using System;
using System.Collections.Generic;

namespace Adf.QueueServer
{
    class QueueHandler
    {
        public Queue<PullAction> deliverQueue;
        public Queue<DataItem> itemQueue;
        public int pullCounter = 0;

        public ulong messageId = 0;
        public readonly ulong queueId = 0;

        public QueueHandler(ulong queueId)
        {
            this.deliverQueue = new Queue<PullAction>();
            this.itemQueue = new Queue<DataItem>();
            this.queueId = queueId;
        }
    }
}