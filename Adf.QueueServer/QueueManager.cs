using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Adf.QueueServer
{
    class QueueManager : IDisposable
    {
        private readonly Dictionary<string, QueueHandler> queueDictionary;
        //
        private readonly Service.ServiceContext serviceContext;
        private readonly LogManager logManager;
        //
        private bool disposed = false;
        //
        private ulong queueId = 0;

        public int Count
        {
            get { return this.queueDictionary.Count; }
        }

        public QueueManager()
        {
            //variable
            this.logManager = Program.LogManager;
            this.serviceContext = Program.ServiceContext;
            //
            this.queueDictionary = new Dictionary<string, QueueHandler>();
            //
            //this.InitializeJournal();
            //this.LoadJournal();
        }

        //private void InitializeJournal()
        //{
        //    //this.journalManager = JournalManager.GetJournalManager(this.logManager, this.dataPath);
        //    //this.journalEnable = !(this.journalManager is JournalDisabled);
        //}

        //private void LoadJournal()
        //{
        //    //this.journalManager.LoadJournal(this.JournalBlockItem);
        //}

        //private void JournalBlockItem(JournalBlockType blockType, BufferSegment bufferSegment)
        //{
        //}

        public QueueHandler AddQueue(string queueName)
        {
            var handler = new QueueHandler(++this.queueId);
            this.queueDictionary.Add(queueName, handler);
            return handler;
        }

        public bool DelQueue(string queueName)
        {
            return this.queueDictionary.Remove(queueName);
        }

        public QueueHandler GetQueue(string queueName)
        {
            QueueHandler queue = null;
            this.queueDictionary.TryGetValue(queueName, out queue);
            return queue;
        }

        public QueueProperty GetProperty(string queueName)
        {
            var queue = this.GetQueue(queueName);
            if (queue == null)
            {
                return null;
            }
            //
            var qp = new QueueProperty();
            //
            qp.name = queueName;
            qp.count = queue.itemQueue.count;
            qp.wait = queue.deliverQueue.count;
            qp.pull = queue.pullCounter;
            //
            return qp;
        }

        public QueueProperty[] GetPropertys(int size)
        {
            var ie = this.queueDictionary.GetEnumerator();
            var count = 0;
            var propertys = new QueueProperty[size];
            //
            while (ie.MoveNext() && count < size)
            {
                var qp = new QueueProperty();
                //
                var item = ie.Current;
                var value = item.Value;

                //
                qp.name = item.Key;
                qp.count = value.itemQueue.count;
                qp.wait = value.deliverQueue.count;
                qp.pull = value.pullCounter;
                
                //
                propertys[count] = qp;
                //
                count++;
            }
            //
            return propertys;
        }

        public void Dispose()
        {
            if (this.disposed == true)
                throw new ObjectDisposedException(this.GetType().Name);

            this.disposed = true;
        }
    }
}