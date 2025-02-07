using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GdbControl.Tests
{
    [TestClass]
    public class GdbMemoryWordsTests
    {
        [TestMethod]
        [Timeout(5000)]
        public void WaitUntilDataAvailable()
        {
            var targetMemoryWords = new GdbMemoryWords();

            var tasksWaitingUntilDataAvailable = 0;
            var tasksWithAvailableData = 0;

            var waitAction = new Action(
                () =>
                {
                    Interlocked.Increment(ref tasksWaitingUntilDataAvailable);
                    targetMemoryWords.WaitUntilDataAvailable();
                    Interlocked.Increment(ref tasksWithAvailableData);
                });

            Task.Factory.StartNew(waitAction);
            Task.Factory.StartNew(waitAction);
            Task.Factory.StartNew(waitAction);
            Task.Factory.StartNew(waitAction);

            while (tasksWaitingUntilDataAvailable < 4)
                Thread.Sleep(100);

            Thread.Sleep(500);
            Assert.AreEqual(0, tasksWithAvailableData); // just to make sure that they don't rush through without waiting for the done flag...

            targetMemoryWords.Data = new List<uint>();

            while (tasksWithAvailableData < 4)
                Thread.Sleep(100);
        }
    }
}
