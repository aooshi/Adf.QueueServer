using System;

namespace Adf.QueueServer
{
    class DataItem
    {
        public byte[] body = null;
        public ushort duplications = 0;
        public UInt64 messageId = 0;
    }
}