using System.Collections.Generic;
using System.Threading;

namespace GdbControl
{
    public class CommandTicket
    {
        private readonly object DoneLock = new object();

        public readonly string Command;
        
        public enum ResultTypes
        {
            Pending = 0,
            Done,
            Error,
            Running,
            Unknown
        }

        public ResultTypes ResultType { get; internal set; }

        private IDictionary<string, object> resultRecord;
        public IDictionary<string, object> ResultRecord
        {
            get { return resultRecord; }
            /*internal*/ set
            {
                lock (DoneLock)
                {
                    resultRecord = value;
                    Monitor.PulseAll(DoneLock);
                }
            }
        }

        //private IDictionary<string, object> consoleRecord;
        //public IDictionary<string, object> ConsoleRecord
        //{
        //    get { return consoleRecord; }
        //    internal set
        //    {
        //        lock (DoneLock)
        //        {
        //            consoleRecord = value;
        //            Monitor.PulseAll(DoneLock);
        //        }
        //    }
        //}

        public CommandTicket(string command)
        {
            Command = command;
        }

        public bool Done
        {
            get { return ResultRecord != null; }
        }

        public void WaitUntilDone()
        {
            lock (DoneLock)
            {
                while (!Done)
                    Monitor.Wait(DoneLock);
            }
        }
    }

    public class CommandTicket<TDedicatedObject> : CommandTicket
    {
        public readonly TDedicatedObject DedicatedObject;

        public CommandTicket(string command, TDedicatedObject dedicatedObject)
            : base(command)
        {
            DedicatedObject = dedicatedObject;
        }

    }
}
