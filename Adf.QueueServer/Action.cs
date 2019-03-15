using System;
using System.IO;

namespace Adf.QueueServer
{
    class Action
    {
        public const byte LPUSH = 1;
        public const byte RPUSH = 2;
        public const byte DELETE = 3;
        public const byte PULL = 4;
        public const byte CLEAR = 5;
        public const byte COUNT = 6;
        public const byte LCANCEL = 7;
        public const byte RCANCEL = 8;
        public const byte CREATEQUEUE = 9;
        public const byte DELETEQUEUE = 10;
        //
        public const byte DELIVER = 11;
        public const byte DISCONNECT = 12;
        public const byte HOME = 13;
        //
        public const string RESULT_OK = "ok";

        public byte actionType = 0;
        public string id = null;
        public string queue = null;

        public IChannel channel = null;
        public QueueHandler queueHandler = null;

        public string result = null;


        public virtual byte[] ToBytes()
        {
            int position = 0;
            return this.GetHeaders(0, ref position);
        }

        public byte[] GetHeaders(int extendLength, ref int position)
        {
            byte[] id_data = System.Text.Encoding.ASCII.GetBytes(this.id);
            byte[] queue_data = System.Text.Encoding.ASCII.GetBytes(this.queue);
            byte[] result_data = System.Text.Encoding.ASCII.GetBytes(this.result);
            //
            int idl = id_data.Length;
            int qdl = queue_data.Length;
            int rdl = result_data.Length;
            //
            byte[] data = null;
            //7 = action + id len + queue len + result len + extend length
            data = new byte[7 + idl + qdl + rdl + extendLength];
            //
            position = 0;
            data[position] = this.actionType;
            position += 1;

            //id
            Adf.BaseDataConverter.ToBytes((ushort)idl, data, position);
            position += 2;
            Array.Copy(id_data, 0, data, position, idl);
            position += idl;
            //queue
            Adf.BaseDataConverter.ToBytes((ushort)qdl, data, position);
            position += 2;
            Array.Copy(queue_data, 0, data, position, qdl);
            position += qdl;
            //result
            Adf.BaseDataConverter.ToBytes((ushort)rdl, data, position);
            position += 2;
            Array.Copy(result_data, 0, data, position, rdl);
            position += rdl;

            //
            return data;
        }

        public static string ToJson(Action action)
        {
            var table = new System.Collections.Hashtable(7);
            if (action.actionType == Action.PULL)
            {
                var pullAction = (PullAction)action;
                table.Add("action", "pull");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
                table.Add("body", System.Text.Encoding.UTF8.GetString(pullAction.item.body));
                table.Add("duplications", pullAction.item.duplications);
                table.Add("messageid", pullAction.item.messageId);
            }
            else if (action.actionType == Action.LPUSH)
            {
                var pushAction = (PushAction)action;
                table.Add("action", "lpush");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
                table.Add("messageid", pushAction.item.messageId);
            }
            else if (action.actionType == Action.RPUSH)
            {
                var pushAction = (PushAction)action;
                table.Add("action", "rpush");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
                table.Add("messageid", pushAction.item.messageId);
            }
            else if (action.actionType == Action.DELETE)
            {
                table.Add("action", "delete");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
            }
            else if (action.actionType == Action.LCANCEL)
            {
                table.Add("action", "lcancel");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
            }
            else if (action.actionType == Action.RCANCEL)
            {
                table.Add("action", "rcancel");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
            }
            else if (action.actionType == Action.CLEAR)
            {
                var clearAction = (ClearAction)action;
                table.Add("action", "clear");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
                table.Add("count", clearAction.count);
            }
            else if (action.actionType == Action.COUNT)
            {
                var clearAction = (CountAction)action;
                table.Add("action", "count");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
                table.Add("count", clearAction.count);
            }
            else if (action.actionType == Action.CREATEQUEUE)
            {
                table.Add("action", "createqueue");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
            }
            else if (action.actionType == Action.DELETEQUEUE)
            {
                table.Add("action", "deletequeue");
                table.Add("requestid", action.id);
                table.Add("queue", action.queue);
                table.Add("result", action.result);
            }
            else
            {
                throw new NotSupportedException("not support action type " + action.actionType);
            }

            return Adf.JsonHelper.Serialize(table);
        }
    }

    class HomeAction : Action
    {
        public QueueProperty[] propertys = null;
    }

    class PushAction : Action
    {
        public DataItem item = null;

        public override byte[] ToBytes()
        {
            int position = 0;
            int length = 0;
            byte[] data = null;

            if (Action.RESULT_OK == this.result)
            {
                length = 8;
                data = base.GetHeaders(length, ref position);
                //
                Adf.BaseDataConverter.ToBytes(this.item.messageId, data, position);
                //position += 8;
            }
            else
            {
                data = base.GetHeaders(length, ref position);
            }

            return data;
        }
    }

    class PullAction : Action
    {
        public DataItem item = null;

        public override byte[] ToBytes()
        {
            int position = 0;
            int length = 0;
            byte[] data = null;

            if (Action.RESULT_OK == this.result)
            {
                var bdl = this.item.body.Length;
                //12 = body len + dup + mid size
                length = 12 + bdl;
                data = base.GetHeaders(length, ref position);
                //body
                Adf.BaseDataConverter.ToBytes((ushort)bdl, data, position);
                position += 2;
                Array.Copy(this.item.body, 0, data, position, bdl);
                position += bdl;
                //duplications
                Adf.BaseDataConverter.ToBytes(this.item.duplications, data, position);
                position += 2;
                //messageid
                Adf.BaseDataConverter.ToBytes(this.item.messageId, data, position);
                //position += 8;
            }
            else
            {
                data = base.GetHeaders(length, ref position);
            }

            return data;
        }
    }

    class CountAction : Action
    {
        public int count = 0;

        public override byte[] ToBytes()
        {
            int position = 0;
            int length = 0;
            byte[] data = null;
            //
            if (Action.RESULT_OK == this.result)
            {
                length = 4;
                data = base.GetHeaders(length, ref position);
                //
                Adf.BaseDataConverter.ToBytes(this.count, data, position);
                //position += 4;
            }
            else
            {
                data = base.GetHeaders(length, ref position);
            }
            //
            return data;
        }
    }

    class ClearAction : Action
    {
        public int count = 0;
        //
        public override byte[] ToBytes()
        {
            int position = 0;
            int length = 0;
            byte[] data = null;
            //
            if (Action.RESULT_OK == this.result)
            {
                length = 4;
                data = base.GetHeaders(length, ref position);
                //
                Adf.BaseDataConverter.ToBytes(this.count, data, position);
                //position += 4;
            }
            else
            {
                data = base.GetHeaders(length, ref position);
            }
            //
            return data;
        }
    }
}