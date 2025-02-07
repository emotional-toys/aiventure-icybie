using System;
using System.Collections.Generic;
using System.Linq;
using Consonya.MSTestTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GdbControl.Tests
{
    [TestClass]
    public class BreakpointsTests
    {
        [TestMethod]
        public void AddBreakpoint()
        {
            var targetBreakpoints = new Breakpoints();

            var breakpoint1 = new GdbBreakpoint("LocationIdentifier1");
            var breakpoint2 = new GdbBreakpoint("LocationIdentifier2")
            {
                RawInfo = new Dictionary<string, object>
                {
                    {"number", "1"},
                    {"pending", "./main:1"},
                }
            };

            var breakpoint3 = new GdbBreakpoint("LocationIdentifier3")
            {
                RawInfo = new Dictionary<string, object>
                {
                    {"number", "2"},
                    {"address", "0x12345"},
                }
            };

            targetBreakpoints.AddBreakpoint(breakpoint1);
            targetBreakpoints.AddBreakpoint(breakpoint2);
            targetBreakpoints.AddBreakpoint(breakpoint3);

            var testSequence = new Action[]
            {
                () => Assert.AreEqual(3, targetBreakpoints.BreakpointsByLocationIdentifier().Count()), // all breakpoints are registered by their location identifiers
                () => Assert.AreEqual(2, targetBreakpoints.BreakpointsById().Count()), // breakpoints that are already acknowledged are also registered by id
                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsWaitingForAnswer().Count()), // one breakpoint is waiting for answer
                () => Assert.AreEqual(1, targetBreakpoints.PendingBreakpoints().Count()), // one breakpoint is pending
                () => Assert.AreEqual(1, targetBreakpoints.AcknowledgedBreakpoints().Count()), // one breakpoint is fully acknowledged
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void IndexedAccess()
        {
            var targetBreakpoints = new Breakpoints();

            var breakpoint1 = new GdbBreakpoint("LocationIdentifier1");
            var breakpoint2 = new GdbBreakpoint("LocationIdentifier2")
            {
                RawInfo = new Dictionary<string, object>
                {
                    {"number", "1"},
                    {"pending", "./main:1"},
                }
            };

            targetBreakpoints.AddBreakpoint(breakpoint1);
            targetBreakpoints.AddBreakpoint(breakpoint2);

            var testSequence = new Action[]
            {
                () => Assert.AreSame(breakpoint1, targetBreakpoints.Breakpoint("LocationIdentifier1")),
                () => Assert.AreSame(breakpoint2, targetBreakpoints.Breakpoint(1)),
                () => AssertHelpers.AssertThrows(() => { var dummy = targetBreakpoints.Breakpoint("Unknown"); }, typeof(KeyNotFoundException)),
                () => AssertHelpers.AssertThrows(() => { var dummy = targetBreakpoints.Breakpoint(-17); }, typeof(KeyNotFoundException)),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void Clear()
        {
            var targetBreakpoints = new Breakpoints();

            var breakpoint1 = new GdbBreakpoint("LocationIdentifier1");
            var breakpoint2 = new GdbBreakpoint("LocationIdentifier2")
            {
                RawInfo = new Dictionary<string, object>
                {
                    {"number", "1"},
                    {"pending", "./main:1"},
                }
            };

            var breakpoint3 = new GdbBreakpoint("LocationIdentifier3")
            {
                RawInfo = new Dictionary<string, object>
                {
                    {"number", "2"},
                    {"address", "0x12345"},
                }
            };

            targetBreakpoints.AddBreakpoint(breakpoint1);
            targetBreakpoints.AddBreakpoint(breakpoint2);
            targetBreakpoints.AddBreakpoint(breakpoint3);

            targetBreakpoints.Clear();

            var testSequence = new Action[]
            {
                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsByLocationIdentifier().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsById().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsWaitingForAnswer().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.PendingBreakpoints().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.AcknowledgedBreakpoints().Count()),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void RemoveBreakpoint()
        {
            var targetBreakpoints = new Breakpoints();

            var breakpoint1 = new GdbBreakpoint("LocationIdentifier1");
            var breakpoint2 = new GdbBreakpoint("LocationIdentifier2")
            {
                RawInfo = new Dictionary<string, object>
                {
                    {"number", "1"},
                    {"pending", "./main:1"},
                }
            };

            var breakpoint3 = new GdbBreakpoint("LocationIdentifier3")
            {
                RawInfo = new Dictionary<string, object>
                {
                    {"number", "2"},
                    {"address", "0x12345"},
                }
            };

            targetBreakpoints.AddBreakpoint(breakpoint1);
            targetBreakpoints.AddBreakpoint(breakpoint2);
            targetBreakpoints.AddBreakpoint(breakpoint3);

            var testSequence = new Action[]
            {
                () => Assert.AreEqual(3, targetBreakpoints.BreakpointsByLocationIdentifier().Count()),
                () => Assert.AreEqual(2, targetBreakpoints.BreakpointsById().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsWaitingForAnswer().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.PendingBreakpoints().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.AcknowledgedBreakpoints().Count()),
                
                () => targetBreakpoints.RemoveBreakpoint(breakpoint3),

                () => Assert.AreEqual(2, targetBreakpoints.BreakpointsByLocationIdentifier().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsById().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsWaitingForAnswer().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.PendingBreakpoints().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.AcknowledgedBreakpoints().Count()),

                () => targetBreakpoints.RemoveBreakpoint(breakpoint1),

                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsByLocationIdentifier().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsById().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsWaitingForAnswer().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.PendingBreakpoints().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.AcknowledgedBreakpoints().Count()),

                () => targetBreakpoints.RemoveBreakpoint(breakpoint2),

                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsByLocationIdentifier().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsById().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsWaitingForAnswer().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.PendingBreakpoints().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.AcknowledgedBreakpoints().Count()),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void ChangeBreakpointStatus()
        {
            var targetBreakpoints = new Breakpoints();

            var breakpoint1 = new GdbBreakpoint("LocationIdentifier1");

            var breakpointRawInfo1 = new Dictionary<string, object>
            {
                {"number", "1"},
                {"pending", "./main:1"},
            };

            var breakpointRawInfo2 = new Dictionary<string, object>
            {
                {"number", "1"}, // in fact we could even change the number, but we don't want to, because it is unnatural :-)
                {"address", "0x12345"},
            };

            var testSequence = new Action[]
            {
                () => targetBreakpoints.AddBreakpoint(breakpoint1),
                
                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsByLocationIdentifier().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsById().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsWaitingForAnswer().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.PendingBreakpoints().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.AcknowledgedBreakpoints().Count()),

                () => breakpoint1.RawInfo = breakpointRawInfo1,

                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsByLocationIdentifier().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsById().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsWaitingForAnswer().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.PendingBreakpoints().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.AcknowledgedBreakpoints().Count()),

                () => breakpoint1.RawInfo = breakpointRawInfo2,

                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsByLocationIdentifier().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.BreakpointsById().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.BreakpointsWaitingForAnswer().Count()),
                () => Assert.AreEqual(0, targetBreakpoints.PendingBreakpoints().Count()),
                () => Assert.AreEqual(1, targetBreakpoints.AcknowledgedBreakpoints().Count()),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }
    }
}
