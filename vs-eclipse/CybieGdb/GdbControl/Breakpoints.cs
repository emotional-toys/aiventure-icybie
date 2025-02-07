using System.Collections.Generic;
using System.Linq;

namespace GdbControl
{
    public class Breakpoints
    {
        private readonly Dictionary<string, GdbBreakpoint> WaitingForAnswer = new Dictionary<string, GdbBreakpoint>();
        private readonly Dictionary<int, GdbBreakpoint> Pending = new Dictionary<int, GdbBreakpoint>();
        private readonly Dictionary<int, GdbBreakpoint> Acknowledged = new Dictionary<int, GdbBreakpoint>();
        
        private readonly SortedDictionary<string, GdbBreakpoint> AllBreakpointsByLocationIdentifier = new SortedDictionary<string, GdbBreakpoint>();
        private readonly SortedDictionary<int, GdbBreakpoint> AllBreakpointsById = new SortedDictionary<int, GdbBreakpoint>();

        private readonly object DictOperationLock = new object();

        public void AddBreakpoint(GdbBreakpoint breakpoint)
        {
            breakpoint.PreInfoChange += RemoveBreakpointFromAllDictionaries;
            breakpoint.InfoChanged += AddBreakpointToAppropriateDictionaries;
            AddBreakpointToAppropriateDictionaries(breakpoint);
        }

        public void RemoveBreakpoint(GdbBreakpoint breakpoint)
        {
            breakpoint.PreInfoChange -= RemoveBreakpointFromAllDictionaries;
            breakpoint.InfoChanged -= AddBreakpointToAppropriateDictionaries;
            RemoveBreakpointFromAllDictionaries(breakpoint);
        }

        public void Clear()
        {
            WaitingForAnswer.Clear();
            Pending.Clear();
            Acknowledged.Clear();
            AllBreakpointsByLocationIdentifier.Clear();
            AllBreakpointsById.Clear();
        }

        private void RemoveBreakpointFromAllDictionaries(GdbBreakpoint breakpoint)
        {
            AllBreakpointsByLocationIdentifier.Remove(breakpoint.LocationIdentifier);
            WaitingForAnswer.Remove(breakpoint.LocationIdentifier);

            if (breakpoint.Id == null)
                return;

            var breakpointId = breakpoint.Id.Value;
            AllBreakpointsById.Remove(breakpointId);
            Pending.Remove(breakpointId);
            Acknowledged.Remove(breakpointId);
        }

        private void AddBreakpointToAppropriateDictionaries(GdbBreakpoint breakpoint)
        {
            lock (DictOperationLock)
            {
                AllBreakpointsByLocationIdentifier.Add(breakpoint.LocationIdentifier, breakpoint);

                if (!breakpoint.Acknowledged)
                {
                    WaitingForAnswer.Add(breakpoint.LocationIdentifier, breakpoint);
                    return;
                }

                AllBreakpointsById.Add(breakpoint.Id.Value, breakpoint);

                if (breakpoint.Pending)
                    Pending.Add(breakpoint.Id.Value, breakpoint);
                else
                    Acknowledged.Add(breakpoint.Id.Value, breakpoint);
            }
        }

        public GdbBreakpoint Breakpoint(string locationIdentifier)
        {
            return AllBreakpointsByLocationIdentifier[locationIdentifier];
        }

        public GdbBreakpoint Breakpoint(int breakpointId)
        {
            return AllBreakpointsById[breakpointId];
        }

        public IEnumerable<KeyValuePair<string, GdbBreakpoint>> BreakpointsWaitingForAnswer()
        {
            lock (DictOperationLock)
            {
                return WaitingForAnswer.ToArray();
            }
        }

        public IEnumerable<KeyValuePair<int, GdbBreakpoint>> PendingBreakpoints()
        {
            lock (DictOperationLock)
            {
                return Pending.ToArray();
            }
        }

        public IEnumerable<KeyValuePair<int, GdbBreakpoint>> AcknowledgedBreakpoints()
        {
            lock (DictOperationLock)
            {
                return Acknowledged.ToArray();
            }
        }

        public IEnumerable<KeyValuePair<string, GdbBreakpoint>> BreakpointsByLocationIdentifier()
        {
            lock (DictOperationLock)
            {
                return AllBreakpointsByLocationIdentifier.ToArray();
            }
        }

        public IEnumerable<KeyValuePair<int, GdbBreakpoint>> BreakpointsById()
        {
            lock (DictOperationLock)
            {
                return AllBreakpointsById.ToArray();
            }
        }
    }
}
