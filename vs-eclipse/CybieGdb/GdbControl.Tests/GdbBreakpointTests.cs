using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Consonya.MSTestTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GdbControl.Tests
{
    [TestClass]
    public class GdbBreakpointTests
    {
        [TestMethod]
        public void ConstructionAndRawInfo()
        {
            var breakpointRecordDict1 = new Dictionary<string, object> // this is a sample breakpoint-record as sent by gdb
            {
                { "number", "1"},
                { "type", "breakpoint"},
                { "disp", "keep"},
                { "enabled", "yes"},
                { "addr", "0x00001234"},
                { "at", "main+3"},
                { "times", "0"},
                { "original-location", "main"},
            };

            var targetBreakpoint0 = new GdbBreakpoint("whatever");
            var targetBreakpoint1 = new GdbBreakpoint("whateverLocationIdentifier") {RawInfo = breakpointRecordDict1};

            var testSequence = new Action[]
            {
                () => Assert.AreEqual("whatever", targetBreakpoint0.LocationIdentifier), // the default-one is taken that we pass during construction
                () => Assert.IsFalse(targetBreakpoint0.Acknowledged),
                () => Assert.IsNull(targetBreakpoint0.Id),
                () => Assert.IsNull(targetBreakpoint0.Address),

                () => Assert.AreEqual("main", targetBreakpoint1.LocationIdentifier), // no matter, what we pass during construction - if there is an acknowledged location identifier we take it!
                () => Assert.IsTrue(targetBreakpoint1.Acknowledged),
                () => Assert.AreEqual(1, targetBreakpoint1.Id),
                () => Assert.AreEqual(0x00001234u, targetBreakpoint1.Address),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void InfoChangeEvents()
        {
            var events = new List<string>();

            var breakpointRecordDict = new Dictionary<string, object>
            {
                { "number", "1"},
                { "type", "breakpoint"},
                { "disp", "keep"},
                { "enabled", "yes"},
                { "addr", "0x00001234"},
                { "at", "main+3"},
                { "times", "0"},
                { "original-location", "main"},
            };

            var targetBreakpoint = new GdbBreakpoint("whatever");
            targetBreakpoint.PreInfoChange += bkpt => events.Add("pre");
            targetBreakpoint.InfoChanged += bkpt => events.Add("changed");

            targetBreakpoint.RawInfo = breakpointRecordDict;

            AssertHelpers.AssertAreSequenceEqual(new[]{"pre", "changed"}, events);
        }

        [TestMethod]
        [Timeout(5000)]
        public void WaitForAcknowledgement()
        {
            var targetBreakpoint1 = new GdbBreakpoint("whatever");

            var breakpointRecordDict = new Dictionary<string, object> // just a dummy... 
            {
                { "number", "1"},
                { "original-location", "whatever"},
            };
            var targetBreakpoint2 = new GdbBreakpoint("whatever") {RawInfo = breakpointRecordDict};

            var breakpoint2Acknowledged = false;

            var tasksWaitingForAcknowledgmentOfBreakpoint1 = 0;
            var tasksWithAcknowledgedBreakpoint1 = 0;

            Task.Factory.StartNew(
                () =>
                {
                    targetBreakpoint2.WaitForAcknowledgement();
                    breakpoint2Acknowledged = true;
                }); // this one is already acknowledged, so there must not be any waiting for anything...

            var waitForBkpt1Action = new Action(
                () =>
                {
                    Interlocked.Increment(ref tasksWaitingForAcknowledgmentOfBreakpoint1);
                    targetBreakpoint1.WaitForAcknowledgement();
                    Interlocked.Increment(ref tasksWithAcknowledgedBreakpoint1);
                });


            var waitingTask1 = Task.Factory.StartNew(waitForBkpt1Action);
            var waitingTask2 = Task.Factory.StartNew(waitForBkpt1Action);
            var waitingTask3 = Task.Factory.StartNew(waitForBkpt1Action);
            var waitingTask4 = Task.Factory.StartNew(waitForBkpt1Action);

            while (tasksWaitingForAcknowledgmentOfBreakpoint1 < 4) // wait until all the tasks are waiting for acknowledgement...
                Thread.Sleep(50);

            // test, whether they are really waiting and not just rushing through...
            Thread.Sleep(500);
            Assert.AreEqual(0, tasksWithAcknowledgedBreakpoint1);
            
            // now unblock them
            targetBreakpoint1.RawInfo = breakpointRecordDict;

            while (tasksWithAcknowledgedBreakpoint1 < 4) // wait until they all signal that they received their acknowledgement
                Thread.Sleep(50);

            Assert.IsTrue(breakpoint2Acknowledged); // and don't forget to check, whether the other task ran through
        }

        [TestMethod]
        [Timeout(5000)]
        public void WaitForDeletion()
        {
            var targetBreakpoint1 = new GdbBreakpoint("whatever");

            var tasksWaitingForDeletion = 0;
            var tasksWithConfirmedDeletion = 0;

            var waitForDeletionAction = new Action(
                () =>
                {
                    Interlocked.Increment(ref tasksWaitingForDeletion);
                    targetBreakpoint1.WaitForDeletion();
                    Interlocked.Increment(ref tasksWithConfirmedDeletion);
                });


            var waitingTask1 = Task.Factory.StartNew(waitForDeletionAction);
            var waitingTask2 = Task.Factory.StartNew(waitForDeletionAction);
            var waitingTask3 = Task.Factory.StartNew(waitForDeletionAction);
            var waitingTask4 = Task.Factory.StartNew(waitForDeletionAction);

            while (tasksWaitingForDeletion < 4) // wait until all the tasks are waiting for acknowledgement...
                Thread.Sleep(50);

            // test, whether they are really waiting and not just rushing through...
            Thread.Sleep(500);
            Assert.AreEqual(0, tasksWithConfirmedDeletion);

            // now unblock them
            targetBreakpoint1.Deleted = true;

            while (tasksWithConfirmedDeletion < 4) // wait until they all signal that they received their acknowledgement
                Thread.Sleep(50);
        }

        [TestMethod]
        [Timeout(5000)]
        public void WaitForBreakpointHit()
        {
            var rawBreakpointInfo = new Dictionary<string, object> // just a dummy... 
            {
                {"number", "1"},
                {"original-location", "whatever"},
                {"times", "0"},
            };

            var targetBreakpoint1 = new GdbBreakpoint("whatever") {RawInfo = rawBreakpointInfo};

            var tasksWaitingForHit = 0;
            var tasksWithConfirmedHit = 0;

            var waitForDeletionAction = new Action(
                () =>
                {
                    Interlocked.Increment(ref tasksWaitingForHit);
                    targetBreakpoint1.WaitForHitsToExceed(0);
                    Interlocked.Increment(ref tasksWithConfirmedHit);
                });


            var waitingTask1 = Task.Factory.StartNew(waitForDeletionAction);
            var waitingTask2 = Task.Factory.StartNew(waitForDeletionAction);
            var waitingTask3 = Task.Factory.StartNew(waitForDeletionAction);
            var waitingTask4 = Task.Factory.StartNew(waitForDeletionAction);

            while (tasksWaitingForHit < 4) // wait until all the tasks are waiting for acknowledgement...
                Thread.Sleep(50);

            // test, whether they are really waiting and not just rushing through...
            Thread.Sleep(500);
            Assert.AreEqual(0, tasksWithConfirmedHit);

            // now unblock them
            rawBreakpointInfo["times"] = "1";
            targetBreakpoint1.RawInfo = rawBreakpointInfo;

            while (tasksWithConfirmedHit < 4) // wait until they all signal that they received their acknowledgement
                Thread.Sleep(50);
        }
    }
}
