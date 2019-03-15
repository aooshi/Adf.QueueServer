using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

namespace Adf.QueueServer
{
    class ActionProcessor
    {
        LogManager logManager;

        Queue<Action> queue;

        Thread processorThread;
        EventWaitHandle processorWaitHandle;
        EventWaitHandle processorEndWaitHandle;

        bool disposed = false;

        public ActionProcessor()
        {
            this.logManager = Program.LogManager;
            this.queue = new Queue<Action>();
            //
            this.processorWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.processorEndWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            //
            this.processorThread = new Thread(this.DeliverProcessor);
            this.processorThread.IsBackground = true;
            this.processorThread.Start();
        }

        private void DeliverProcessor()
        {
            while (this.disposed == false)
            {
                this.processorWaitHandle.WaitOne();

                if (this.disposed == true)
                {
                    break;
                }

                Action action = null;

                //
                lock (this.queue)
                {
                    if (this.queue.count == 0)
                    {
                        this.processorWaitHandle.Reset();
                        continue;
                    }

                    action = this.queue.Pull();
                    action.queueHandler = Program.QueueManager.GetQueue(action.queue);
                }

                //
                if (action.actionType == Action.DELETE)
                {
                    this.Delete(action);
                }
                else if (action.actionType == Action.PULL)
                {
                    this.Pull((PullAction)action);
                }
                else if (action.actionType == Action.RPUSH)
                {
                    this.RPush((PushAction)action);
                }
                else if (action.actionType == Action.DELIVER)
                {
                    this.Deliver(action);
                }
                else if (action.actionType == Action.LPUSH)
                {
                    this.LPush((PushAction)action);
                }
                else if (action.actionType == Action.LCANCEL)
                {
                    this.LCancel(action);
                }
                else if (action.actionType == Action.RCANCEL)
                {
                    this.RCancel(action);
                }
                else if (action.actionType == Action.DISCONNECT)
                {
                    this.Disconnect(action);
                }
                else if (action.actionType == Action.HOME)
                {
                    this.Home((HomeAction)action);
                }
                else if (action.actionType == Action.CLEAR)
                {
                    this.Clear((ClearAction)action);
                }
                else if (action.actionType == Action.COUNT)
                {
                    this.Count((CountAction)action);
                }
                else if (action.actionType == Action.CREATEQUEUE)
                {
                    this.CreateQueue(action);
                }
                else if (action.actionType == Action.DELETEQUEUE)
                {
                    this.DeleteQueue(action);
                }
                else
                {
                    //ignore
                }
            }
            //
            this.processorEndWaitHandle.WaitOne();
        }

        private void Home(HomeAction action)
        {
            if (string.IsNullOrEmpty(action.queue) == false)
            {
                var p = Program.QueueManager.GetProperty(action.queue);
                if (p == null)
                    action.propertys = new QueueProperty[0];
                else
                    action.propertys = new QueueProperty[] { p };
            }
            else
            {
                action.propertys = Program.QueueManager.GetPropertys(100);
            }
            action.channel.Send(action);
        }

        private void Disconnect(Action action)
        {
            this.DO_Disconnect(action);
        }

        private void LPush(PushAction action)
        {
            if (action.queueHandler == null)
            {
                action.result = "queue not exists.";
                action.channel.Send(action);
            }
            else
            {
                action.item.messageId = ++action.queueHandler.messageId;
                this.DO_LPush(action);
            }
        }

        private void RPush(PushAction action)
        {
            if (action.queueHandler == null)
            {
                action.result = "queue not exists.";
                action.channel.Send(action);
            }
            else
            {
                action.item.messageId = ++action.queueHandler.messageId;
                this.DO_RPush(action);
            }
        }

        private void Deliver(Action action)
        {
            var queueHandler = action.queueHandler;
            if (queueHandler == null)
            {
                //ignore
                return;
            }

            if (queueHandler.itemQueue.count == 0)
            {
                //no item
                return;
            }

            if (queueHandler.deliverQueue.count == 0)
            {
                //no deliver
                return;
            }

            this.DO_Deliver(action);
        }

        private void Pull(PullAction action)
        {
            if (action.queueHandler == null)
            {
                action.result = "queue not exists.";
                action.channel.Send(action);
            }
            else
            {
                this.DO_Pull(action);
            }
        }

        private void CreateQueue(Action action)
        {
            this.DO_CreateQueue(action);
        }

        private void DeleteQueue(Action action)
        {
            this.DO_DeleteQueue(action);
        }

        private void Delete(Action action)
        {
            this.DO_Delete(action);
        }

        private void LCancel(Action action)
        {
            this.DO_LCancel(action);
        }

        private void RCancel(Action action)
        {
            this.DO_RCancel(action);
        }

        private void Clear(ClearAction action)
        {
            this.DO_Clear(action);
        }

        private void Count(CountAction action)
        {
            this.DO_Count(action);
        }

        private void DO_RPush(PushAction action)
        {
            action.result = Action.RESULT_OK;
            //join data
            action.queueHandler.itemQueue.RPush(action.item);
            //
            action.channel.Send(action);

            //trigger deliver
            if ((action.queueHandler.deliverQueue.count == 0) == false)
            {
                this.PushDeliver(action.queue);
            }
        }

        private void DO_LPush(PushAction action)
        {
            action.result = Action.RESULT_OK;
            //join data
            action.queueHandler.itemQueue.LPush(action.item);
            //
            action.channel.Send(action);

            //trigger deliver
            if ((action.queueHandler.deliverQueue.count == 0) == false)
            {
                this.PushDeliver(action.queue);
            }
        }

        private void DO_Pull(PullAction action)
        {
            //add deliver to queue
            action.queueHandler.deliverQueue.RPush(action);

            //
            this.PushDeliver(action.queue);
        }

        private void DO_Deliver(Action action)
        {
            var pullAction = action.queueHandler.deliverQueue.Pull();
            var channelAvailable = pullAction.channel.GetAvailable();
            if (channelAvailable == true)
            {
                var dataItem = action.queueHandler.itemQueue.Pull();
                dataItem.duplications++;

                //set data
                pullAction.result = Action.RESULT_OK;
                pullAction.item = dataItem;

                //pullAction.actionType = Action.TYPE_PULL;
                //this function only allow input pull action

                var pulls = pullAction.channel.GetPulls();

                //add item to pull pool.
                pulls.Add(pullAction);

                action.queueHandler.pullCounter++;

                //send
                pullAction.channel.Send(pullAction);
            }
            else
            {
                //next deliver
                this.Deliver(action);
            }
        }

        private void DO_CreateQueue(Action action)
        {
            var queueHandler = action.queueHandler;
            if (action.queue == "")
            {
                action.result = "no set queue name.";
            }
            else if (queueHandler == null)
            {
                action.queueHandler = Program.QueueManager.AddQueue(action.queue);
                action.result = Action.RESULT_OK;
            }
            else
            {
                action.result = "queue exists.";
            }
            //
            action.channel.Send(action);
        }

        private void DO_DeleteQueue(Action action)
        {
            var queueHandler = action.queueHandler;
            if (queueHandler == null)
            {
                action.result = "queue not exists.";
            }
            else
            {
                Program.QueueManager.DelQueue(action.queue);
                action.result = Action.RESULT_OK;
            }
            //
            action.channel.Send(action);
        }

        private void DO_Delete(Action action)
        {
            var pulls = action.channel.GetPulls();
            var workActionIndex = pulls.FindIndex(m => { return m.id == action.id; });
            if (workActionIndex == -1)
            {
                action.result = "message was not pulled.";
            }
            else
            {
                //remove
                pulls.RemoveAt(workActionIndex);
                //
                var queueHandler = action.queueHandler;
                if (queueHandler == null)
                {
                    action.result = "queue not exists.";
                }
                else
                {
                    action.result = Action.RESULT_OK;
                    queueHandler.pullCounter--;
                }
            }
            //
            action.channel.Send(action);
        }

        private void DO_LCancel(Action action)
        {
            var pulls = action.channel.GetPulls();
            var workActionIndex = pulls.FindIndex(m => { return m.id == action.id; });
            if (workActionIndex == -1)
            {
                action.result = "message was not pulled.";
            }
            else
            {
                //get work action
                var workAction = pulls[workActionIndex];
                //remove work action from pulls
                pulls.RemoveAt(workActionIndex);
                //
                var queueHandler = action.queueHandler;
                if (queueHandler == null)
                {
                    action.result = "queue not exists.";
                }
                else
                {
                    action.result = Action.RESULT_OK;
                    //add work action to item
                    queueHandler.itemQueue.LPush(workAction.item);
                    queueHandler.pullCounter--;
                }
            }
            //
            action.channel.Send(action);
        }

        private void DO_RCancel(Action action)
        {
            var pulls = action.channel.GetPulls();
            var workActionIndex = pulls.FindIndex(m => { return m.id == action.id; });
            if (workActionIndex == -1)
            {
                action.result = "message was not pulled.";
            }
            else
            {
                //get work action
                var workAction = pulls[workActionIndex];
                //remove work action from pulls
                pulls.RemoveAt(workActionIndex);
                //
                var queueHandler = action.queueHandler;
                if (queueHandler == null)
                {
                    action.result = "queue not exists.";
                }
                else
                {
                    action.result = Action.RESULT_OK;
                    //add work action to item
                    queueHandler.itemQueue.RPush(workAction.item);
                    queueHandler.pullCounter--;
                }
            }
            //
            action.channel.Send(action);
        }

        private void DO_Clear(ClearAction action)
        {
            var queueHandler = action.queueHandler;
            if (queueHandler == null)
            {
                action.result = "queue not exists.";
            }
            else
            {
                var count = queueHandler.itemQueue.count;
                //
                action.result = Action.RESULT_OK;
                action.count = count;
                //
                queueHandler.itemQueue.Clear();
            }
            //
            action.channel.Send(action);
        }

        private void DO_Count(CountAction action)
        {
            var queueHandler = action.queueHandler;
            if (queueHandler == null)
            {
                action.result = "queue not exists.";
            }
            else
            {
                var count = queueHandler.itemQueue.count;
                //
                action.result = Action.RESULT_OK;
                action.count = count;
            }
            //
            action.channel.Send(action);
        }

        private void DO_Disconnect(Action action)
        {
            action.channel.Disable();
            //
            var pulls = action.channel.GetPulls();
            //
            lock (this.queue)
            {
                for (int i = pulls.Count - 1; i > -1; i--)
                {
                    var action2 = pulls[i];
                    //
                    var queueHandler = Program.QueueManager.GetQueue(action2.queue);
                    if (queueHandler != null)
                    {
                        queueHandler.itemQueue.LPush(action2.item);
                        queueHandler.pullCounter--;
                        //
                        var action3 = new Action();
                        action3.queue = action2.queue;
                        action3.actionType = Action.DELIVER;
                        this.queue.RPush(action3);
                    }
                }
                //
                pulls.Clear();
            }
        }

        private void PushDeliver(string queue)
        {
            var action2 = new Action();
            action2.queue = queue;
            action2.actionType = Action.DELIVER;

            //
            this.Push(action2);
        }

        public void Push(Action action)
        {
            lock (this.queue)
            {
                this.queue.RPush(action);
                this.processorWaitHandle.Set();
            }
        }
    }
}