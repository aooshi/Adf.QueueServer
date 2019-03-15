using System;
using System.IO;

namespace Adf.QueueServer
{
    class Program : Adf.Service.IService, Adf.Service.IHttpService
    {
        public static Int64 UPTIME = 0;
        public static string VERSION;
        public static string DATA_PATH;
        //
        public static LogManager LogManager;
        public static Adf.Service.ServiceContext ServiceContext;

        public static WebSocketJson WebSocketJson;
        public static WebSocketBinary WebSocketBinary;
        public static HttpHandler HttpHandler;

        public static ActionProcessor ActionProcessor;
        public static QueueManager QueueManager;
        public static JournalManager JournalManager;

        public static Int32 ConnectionCounter = 0;

        private bool running = false;

        static void Main(string[] args)
        {
            //Test();
            Adf.Service.ServiceHelper.Entry(args);
        }

        //private static void Test()
        //{
        //    var size = 100000;
        //    var q = new Queue();
        //    for (int i = 1; i <= size; i++)
        //    {
        //        var item = new Item() { data = new byte[0], id = (ulong)i, duplicates = 0, queue = 1, expires = 1, command = 1 };
        //        q.Enqueue(item);
        //    }
        //    for (int i = 1; i <= size; i++)
        //    {
        //        var item = q.Dequeue();
        //        if (item.id != (ulong)i)
        //        {
        //        }
        //    }
        //}

        public void Start(Service.ServiceContext serviceContext)
        {
            Program.UPTIME = Adf.UnixTimestampHelper.ToInt64Timestamp();
            //
            var version = this.GetType().Assembly.GetName().Version;
            Program.VERSION = string.Concat(version.Major, ".", version.Minor, ".", version.Build);
            //
            Program.ServiceContext = serviceContext;
            Program.LogManager = serviceContext.LogManager;
            //
            Program.DATA_PATH = Adf.ConfigHelper.GetSetting("DataPath");
            if (Program.DATA_PATH == "")
            {
                throw new ConfigException("no configuration DataPath item");
            }
            else if (System.IO.Directory.Exists(Program.DATA_PATH) == false)
            {
                System.IO.Directory.CreateDirectory(Program.DATA_PATH);
            }
            //
            Program.ActionProcessor = new QueueServer.ActionProcessor();
            Program.QueueManager = new QueueServer.QueueManager();
            Program.JournalManager = new QueueServer.JournalManager();
            ////20*10000 * 5120 = 950M 
            ////允许20万个同时交换数， 允许最大消息包4KB, 预留1024字节于头
            //Program.BufferPool = new MemberPool<ArraySegment<byte>>(20 * 10000);
            //Program.BufferPool.Creater += () => { return new ArraySegment<byte>(new byte[5120]); };
            //
            serviceContext.LogManager.Message.WriteTimeLine("LogPath:" + serviceContext.LogManager.Path);
            serviceContext.LogManager.Message.WriteTimeLine("LogFlushInterval:" + serviceContext.LogManager.FlushInterval);
            serviceContext.LogManager.Message.WriteTimeLine("DataPath:" + Program.DATA_PATH);
            //
            if (serviceContext.HAEnable == true)
            {
                serviceContext.ToMaster += new EventHandler(this.ToMaster);
                serviceContext.ToSlave += new EventHandler(this.ToSlave);
                serviceContext.ToRestore += new EventHandler(this.ToRestore);
                serviceContext.ToWitness += new EventHandler(this.ToWitness);
            }
            else
            {
                this.Start();
            }
        }

        public void Stop(Service.ServiceContext serviceContext)
        {
            this.Stop();
        }

        private void ToMaster(object sender, EventArgs e)
        {
            this.Start();
        }

        private void ToSlave(object sender, EventArgs e)
        {
            this.StartSlave();
        }

        private void ToRestore(object sender, EventArgs e)
        {
            this.Stop();
        }

        private void ToWitness(object sender, EventArgs e)
        {
            //
        }

        private void LoadAction()
        {
            //try
            //{
            //    this.LoadData(Program.ServiceContext);
            //    this.LoadJournal(Program.ServiceContext);

            //    var rows = 0;
            //    for (int i = 0; i < Program.NumberManagers.Length; i++)
            //    {
            //        rows += Program.NumberManagers[i].Count;
            //    }

            //    Program.ServiceContext.LogManager.Message.WriteTimeLine("All " + rows + " keys");

            //    //listen
            //    Program.Listen = new Listen();
            //}
            //catch (System.Threading.ThreadAbortException)
            //{
            //    //ignore
            //}
        }

        private void LoadJournal(Service.ServiceContext serviceContext)
        {
            //serviceContext.LogManager.Message.WriteTimeLine("Load journal begin");

            //var manager = Program.Journal;
            //var rows = 0;
            //manager.LoadJournal((byte flag, DataItem item) =>
            //{
            //    var key = System.Text.Encoding.ASCII.GetString(item.key_buffer, 0, item.key_length);
            //    var hashCode = HashHelper.GetHashCode(key);
            //    var cacheManger = NumberManager.GetManager(key, hashCode);

            //    if (flag == BlockFlag.DATA)
            //    {
            //        cacheManger.Set(key, hashCode, item);
            //    }
            //    else if (flag == BlockFlag.FREE)
            //    {
            //        cacheManger.Remove(key, hashCode);
            //    }

            //    rows++;
            //});

            //serviceContext.LogManager.Message.WriteTimeLine("Load journal " + rows + " all rows");
            //serviceContext.LogManager.Message.WriteTimeLine("Load journal end");

            ////
            //if (manager.Enable)
            //{
            //    manager.Open();
            //}
        }

        private void LoadData(Service.ServiceContext serviceContext)
        {
            //serviceContext.LogManager.Message.WriteTimeLine("Load data begin");

            //var manager = Program.DataManager;
            //var rows = 0;
            //manager.LoadData((byte flag, DataItem item) =>
            //{
            //    if (flag == BlockFlag.DATA)
            //    {
            //        var key = System.Text.Encoding.ASCII.GetString(item.key_buffer, 0, item.key_length);
            //        var hashCode = HashHelper.GetHashCode(key);
            //        var cacheManger = NumberManager.GetManager(key, hashCode);
            //        //
            //        cacheManger.Set(key, hashCode, item);
            //        rows++;
            //    }
            //});

            //serviceContext.LogManager.Message.WriteTimeLine("Load data " + rows + " rows");
            //serviceContext.LogManager.Message.WriteTimeLine("Load data end");
        }

        private void InitializeJournal(Service.ServiceContext serviceContext)
        {
            //string dataPath = Program.DATA_PATH;
            //LogManager logManager = serviceContext.LogManager;
            ////
            //JournalManager manager = new JournalManager(logManager, dataPath);
            //var jl = Adf.ConfigHelper.GetSetting("Journal", "disk");
            ////
            //if ("memory".Equals(jl, StringComparison.OrdinalIgnoreCase))
            //{
            //    serviceContext.LogManager.Message.WriteTimeLine("Journal: disabled");
            //    manager.Enable = false;
            //}
            //else if ("disk".Equals(jl, StringComparison.OrdinalIgnoreCase))
            //{
            //    serviceContext.LogManager.Message.WriteTimeLine("Journal: enabled");
            //    manager.Enable = true;
            //}
            //else
            //{
            //    throw new ConfigException("Journal configuration invalid");
            //}
            ////
            //Program.Journal = manager;
        }

        private void InitializeSlave()
        {
            //var serviceContext = Program.ServiceContext;
            //var port = Adf.ConfigHelper.GetSettingAsInt("Port", 201);
            //var logManager = serviceContext.LogManager;

            //try
            //{

            //    while (Program.ServiceContext.ServiceState == Service.ServiceState.Slave)
            //    {
            //        var master = serviceContext.GetMaster();
            //        if (master == null)
            //            break;

            //        try
            //        {
            //            this.replicationClient.Dispose();
            //        }
            //        catch { }


            //        System.Threading.Thread.Sleep(5000);

            //        //
            //        try
            //        {
            //            this.replicationClient = new ReplicationClient(serviceContext.LogManager
            //                , Program.DATA_PATH
            //                , master
            //                , port);

            //            this.replicationClient.IOError += new EventHandler<ReplicationExceptionEventArgs>(this.ReplicationClientIOError);
            //            this.replicationClient.Request();

            //            var success = this.replicationClient.GetMain();
            //            if (success == true)
            //            {
            //                success = this.replicationClient.GetJournal();
            //            }

            //            if (success == true)
            //            {
            //                success = this.replicationClient.Sync();
            //            }

            //            if (success == true)
            //            {
            //                break;
            //            }
            //        }
            //        catch (Exception exception)
            //        {
            //            logManager.Message.WriteTimeLine("connection master failure, " + exception.Message + ".");
            //        }
            //    }
            //}
            //catch (System.Threading.ThreadAbortException)
            //{
            //    //ignore
            //}
        }

        //private void ReplicationClientIOError(object sender, ReplicationExceptionEventArgs e)
        //{
        //    this.slaveThread = new System.Threading.Thread(this.InitializeSlave);
        //    this.slaveThread.IsBackground = true;
        //    this.slaveThread.Start();
        //}

        private void Start()
        {
            lock (this)
            {
                if (this.running == true)
                {
                    this.Stop();
                }
                //load init queue
                this.LoadInitQueue();

                Program.WebSocketJson = new WebSocketJson();
                Program.WebSocketBinary = new WebSocketBinary();
                Program.HttpHandler = new HttpHandler();

                //Program.NumberManagers = new NumberManager[Program.HASH_POOL_SIZE];
                //for (int i = 0; i < Program.HASH_POOL_SIZE; i++)
                //{
                //    Program.NumberManagers[i] = new NumberManager();
                //}

                ////
                //this.InitializeData(Program.ServiceContext);
                //this.InitializeJournal(Program.ServiceContext);

                ////load data
                //this.loadThread = new System.Threading.Thread(this.LoadAction);
                //this.loadThread.IsBackground = true;
                //this.loadThread.Start();

                //
                this.running = true;
            }
        }

        private void StartSlave()
        {
            lock (this)
            {
                if (this.running == true)
                {
                    this.Stop();
                }

                //this.InitializeData(Program.ServiceContext);
                //this.InitializeJournal(Program.ServiceContext);

                ////
                //this.slaveThread = new System.Threading.Thread(this.InitializeSlave);
                //this.slaveThread.IsBackground = true;
                //this.slaveThread.Start();

                //
                this.running = true;
            }
        }

        private void Stop()
        {
            lock (this)
            {
                //try
                //{
                //    if (this.loadThread != null)
                //    {
                //        this.loadThread.Abort();
                //    }
                //}
                //catch (System.Threading.ThreadStateException)
                //{
                //    //
                //}

                //try
                //{
                //    if (this.slaveThread != null)
                //    {
                //        this.slaveThread.Abort();
                //    }
                //}
                //catch (System.Threading.ThreadStateException)
                //{
                //    //
                //}

                //if (this.replicationClient != null)
                //{
                //    this.replicationClient.Dispose();
                //}

                //if (Program.Listen != null)
                //{
                //    Program.Listen.Dispose();
                //}

                //if (Program.Journal != null)
                //{
                //    Program.Journal.Dispose();
                //}

                //if (Program.ClosedQueue != null)
                //{
                //    Program.ClosedQueue.WaitCompleted();
                //}

                //if (Program.ClosedQueue != null)
                //{
                //    Program.ClosedQueue.Dispose();
                //}

                //if (Program.DataManager != null)
                //{
                //    Program.DataManager.Dispose();
                //}

                //if (Program.NumberManagers != null)
                //{
                //    for (int i = 0; i < Program.HASH_POOL_SIZE; i++)
                //    {
                //        Program.NumberManagers[i] = null;
                //    }
                //}

                this.running = false;
            }
        }

        //功能用于使用非持久化时，初始的队列
        private void LoadInitQueue()
        {
            var root = System.IO.Path.GetDirectoryName(typeof(HttpHandler).Assembly.Location);
            var file = System.IO.Path.Combine(root, "initqueue.txt");

            if (System.IO.File.Exists(file))
            {
                using (var reader = new StreamReader(file))
                {
                    var line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        Program.QueueManager.AddQueue(line);

                        Program.LogManager.Message.WriteTimeLine("Init queue: " + line);
                    }
                }
            }
        }


        public System.Net.HttpStatusCode HttpProcess(HttpServerContext httpContext)
        {
            return Program.HttpHandler.Request(httpContext);
        }
    }
}