using System.Collections.Generic;
using System.Threading;

namespace GdbControl
{
    public class GdbMemoryWords
    {
        private readonly object DataAvailableLock = new object();

        public uint BaseAddress { get; internal set; }

        private List<uint> data;
        public List<uint> Data
        {
            get { return data; }
            internal set
            {
                lock (DataAvailableLock)
                {
                    data = value;
                    Monitor.PulseAll(DataAvailableLock);
                }
            }
        }

        public bool DataAvailable { get { return Data != null; } }

        public void WaitUntilDataAvailable()
        {
            lock(DataAvailableLock)
            {
                while (!DataAvailable)
                    Monitor.Wait(DataAvailableLock);
            }
        }
    }
}
