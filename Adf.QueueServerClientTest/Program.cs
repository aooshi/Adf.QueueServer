using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adf.QueueServerClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var test = new Adf.QueueServerClientTest.QueueServerHttpTest();
            //test.Test();

            var test2 = new Adf.QueueServerClientTest.QueueServerBinaryTest();
            test2.Test();

            //var test3 = new Adf.QueueServerClientTest.QueueServerJsonTest();
            //test3.Test();
        }
    }
}
