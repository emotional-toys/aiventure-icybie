using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GdbControl.Tests
{
    [TestClass]
    public class CommandTicketTests
    {
        [TestMethod]
        [Timeout(5000)]
        public void WaitUntilDone()
        {
            var targetTicket = new CommandTicket("dummy");

            var tasksWaitingUntilDone = 0;
            var tasksReceivedDone = 0;

            var waitAction = new Action(
                () =>
                {
                    Interlocked.Increment(ref tasksWaitingUntilDone);
                    targetTicket.WaitUntilDone();
                    Interlocked.Increment(ref tasksReceivedDone);
                });

            Task.Factory.StartNew(waitAction);
            Task.Factory.StartNew(waitAction);
            Task.Factory.StartNew(waitAction);
            Task.Factory.StartNew(waitAction);

            while (tasksWaitingUntilDone < 4)
                Thread.Sleep(100);

            Thread.Sleep(500);
            Assert.AreEqual(0, tasksReceivedDone); // just to make sure that they don't rush through without waiting for the done flag...

            targetTicket.ResultRecord = new Dictionary<string, object>();

            while(tasksReceivedDone < 4)
                Thread.Sleep(100);
        }
    }
}
