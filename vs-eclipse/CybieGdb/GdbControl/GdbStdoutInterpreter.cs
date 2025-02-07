using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Consonya.Tools.ExtensionMethods;

namespace GdbControl
{
    public class GdbStdoutInterpreter
    {
        // the different starting characters for different kinds of messages...
        private const char AsyncExecOutputStart = '*';
        private const char AsyncStatusOutputStart = '+';
        private const char AsyncNotificationOutputStart = '=';
        private const char ConsoleStreamOutputStart = '~';
        private const char TargetStreamOutputStart = '@';
        private const char LogStreamOutputStart = '&';
        private const char ResultRecordStart = '^';
        private const char ConsoleStreamOutputBlockDelimiterStart = '(';
        private const string ConsoleStreamOutputBlockDelimiterLine = "gdb)"; // rest of (gdb) - the opening brace was already consumed by the dispatcher

        // the different prefixes for interpreted async notifications
        private const string BreakpointCreatedNotification = "breakpoint-created";
        private const string BreakpointDeletedNotification = "breakpoint-deleted";
        private const string BreakpointModifiedNotification = "breakpoint-modified";

        // result-record treatment prefixes
        private const string DonePrefix = "done"; // if there is data in the result record we have something line done,value="42"
        private const string ErrorPrefix = "error";
        private const string RunningPrefix = "running";

        private readonly Queue<char> GdbOutputDataBuffer = new Queue<char>();

        private Action HandleCurrentGdbOutputData { get; set; }
        private readonly global::GdbControl.GdbControl GdbControlInstance;

        public GdbStdoutInterpreter(global::GdbControl.GdbControl gdbControlInstance)
        {
            GdbControlInstance = gdbControlInstance;
            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;
        }

        /// <summary>
        /// Called on receiving data on gdb's stdout
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Diagnostics.DataReceivedEventArgs"/> instance containing the event data.</param>
        internal void OnGdbOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (args.Data == null)
                return;

            args.Data.DoWithEach(GdbOutputDataBuffer.Enqueue);

            while (GdbOutputDataBuffer.Count > 0)
                HandleCurrentGdbOutputData();
        }

        /// <summary>
        /// Root of the state-machine that is used to read and interpret gdb's data on stdout. This one is used
        /// to find the correct reader according to the prefix that gdb sends
        /// </summary>
        private void DistributeGdbOutputDataByPrefix()
        {
            switch (GdbOutputDataBuffer.Dequeue())
            {
                case AsyncExecOutputStart:
                    HandleCurrentGdbOutputData = ReadAsyncExecOutput;
                    return;
                case AsyncStatusOutputStart:
                    HandleCurrentGdbOutputData = ReadAsyncStatusOutput;
                    return;
                case AsyncNotificationOutputStart:
                    HandleCurrentGdbOutputData = ReadAsyncNotificationOutput;
                    return;
                case ConsoleStreamOutputStart:
                    HandleCurrentGdbOutputData = ReadConsoleStreamOutput;
                    return;
                case TargetStreamOutputStart:
                    HandleCurrentGdbOutputData = ReadTargetStreamOutput;
                    return;
                case LogStreamOutputStart:
                    HandleCurrentGdbOutputData = ReadLogStreamOutput;
                    return;
                case ResultRecordStart:
                    HandleCurrentGdbOutputData = ReadResultRecordOutput;
                    return;
                case ConsoleStreamOutputBlockDelimiterStart:
                    HandleCurrentGdbOutputData = ReadConsoleStreamOutputBlockDelimiter;
                    return;
                default:
                    GdbOutputDataBuffer.Clear(); // ignore this line
                    return;
            }
        }

        /// <summary>
        /// Reads the data on gdb's stdout interpreted as async-notification
        /// </summary>
        private void ReadAsyncNotificationOutput()
        {
            var data = new string(GdbOutputDataBuffer.Select(character => character).ToArray());
            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            InterpretAsyncNotification(data);

            GdbControlInstance.InvokeAsyncNotification(data);
        }

        /// <summary>
        /// Reads the data on gdb's stdout interpreted as async-exec
        /// </summary>
        private void ReadAsyncExecOutput()
        {
            var data = new string(GdbOutputDataBuffer.Select(character => character).ToArray());
            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            GdbControlInstance.InvokeAsyncExec(data);
        }

        /// <summary>
        /// Reads the data on gdb's stdout interpreted as async-status
        /// </summary>
        private void ReadAsyncStatusOutput()
        {
            var data = new string(GdbOutputDataBuffer.Select(character => character).ToArray());
            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            GdbControlInstance.InvokeAsyncStatus(data);
        }

        /// <summary>
        /// Reads the data on gdb's stdout interpreted as console-stream output
        /// </summary>
        private void ReadConsoleStreamOutput()
        {
            var data = global::GdbControl.GdbControl.InterpretBufferAsCString(GdbOutputDataBuffer);
            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;
            GdbControlInstance.InvokeConsoleStreamOutput(data);
        }

        /// <summary>
        /// Console-stream output can be multiline, so it is interpreted as a block followed by a delimiter
        /// </summary>
        private void ReadConsoleStreamOutputBlockDelimiter()
        {
            var obtainedDelimiter = new string(GdbOutputDataBuffer.Select(character => character).ToArray());
            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            if (!obtainedDelimiter.StartsWith(ConsoleStreamOutputBlockDelimiterLine)) // have to use StartsWith rather than Equals because some gdb versions send a trailing space whereas others don't
                return; // just ignore it...
            GdbControlInstance.InvokeConsoleStreamOutputBlockFinished();
        }
        /// <summary>
        /// Reads the data on gdb's stdout interpreted as target-stream output (routed through by gdb)
        /// </summary>
        private void ReadTargetStreamOutput()
        {
            var data = global::GdbControl.GdbControl.InterpretBufferAsCString(GdbOutputDataBuffer);
            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;
            GdbControlInstance.InvokeTargetStreamOutput(data);
        }
        /// <summary>
        /// Reads the data on gdb's stdout interpreted as log-stream output
        /// </summary>
        private void ReadLogStreamOutput()
        {
            var data = global::GdbControl.GdbControl.InterpretBufferAsCString(GdbOutputDataBuffer);
            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;
            GdbControlInstance.InvokeLogStreamOutput(data);
        }
        /// <summary>
        /// Reads the data on gdb's stdout interpreted as a result record
        /// </summary>
        private void ReadResultRecordOutput()
        {
            var data = new string(GdbOutputDataBuffer.Select(character => character).ToArray());

            IDictionary<string, object> resultDict = null;
            var resultType = CommandTicket.ResultTypes.Unknown;

            if (data.StartsWith(DonePrefix))
            {
                resultType = CommandTicket.ResultTypes.Done;
                resultDict = ExtractResultDictIfAny(DonePrefix.Length, data);
            }
            else if (data.StartsWith(ErrorPrefix))
            {
                resultType = CommandTicket.ResultTypes.Error;
                resultDict = ExtractResultDictIfAny(ErrorPrefix.Length, data);
            }
            else if (data.StartsWith(RunningPrefix))
            {
                resultType = CommandTicket.ResultTypes.Running;
                resultDict = ExtractResultDictIfAny(RunningPrefix.Length, data);
            }
            else
            {
                var idxOfComma = data.IndexOf(',');
                resultDict = ExtractResultDictIfAny(idxOfComma, data);
            }

            if (resultDict == null) // no data...
                resultDict = new Dictionary<string, object>();

            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            var currentCommandTicket = GdbControlInstance.PendingCommandTickets.Dequeue(); // result-record is here - command is done, so dequeue it from the Pending-queue

            UpdateDedicatedObjectIfNecessary((dynamic)currentCommandTicket, resultDict);

            currentCommandTicket.ResultType = resultType;
            currentCommandTicket.ResultRecord = resultDict; // this automatically notifies all clients who are waiting for "Done"
            GdbControlInstance.InvokeResultRecord(data);
        }

        private IDictionary<string, object> ExtractResultDictIfAny(int prefixLength, string stringRep)
        {
            if ((stringRep.Length > prefixLength) && (stringRep[prefixLength] == ',')) // there is a result-record
                return new ResultRecordParser().Parse(stringRep.Substring(prefixLength + 1)); // don't forget to skip the comma...
            return null;
        }

        private void UpdateDedicatedObjectIfNecessary(CommandTicket<GdbMemoryWords> currentCommandTicket, IDictionary<string, object> resultDict)
        {
            if (!resultDict.ContainsKey("memory")) // nothing to do
                return;

            // The result is stored in the memory-list, which itself is a list of records, each containing one row of the result.
            // Each of the row-records has a member addr telling us the base-address and a member data, which is a list of the values
            // We requested memory as a 1-row, multi-column vector
            var memoryWords = currentCommandTicket.DedicatedObject;

            var memoryRow = (IDictionary<string, object>)((List<object>) resultDict["memory"])[0];
            var baseAddressString = (string)memoryRow["addr"];

            uint baseAddress;
            // For whatever reason the leading "0x" is a problem, even when specifying NumberStyles.AllowHexSpecifier, so we use Substring instead
            uint.TryParse(baseAddressString.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out baseAddress); // and if it can't be parsed correctly it will be 0...

            memoryWords.BaseAddress = baseAddress;

            var rawValueList = (List<object>) memoryRow["data"];

            var valueList = new List<uint>();
            foreach (var rawValue in rawValueList)
            {
                uint parsedVal;
                // For whatever reason the leading "0x" is a problem, even when specifying NumberStyles.AllowHexSpecifier, so we use Substring instead
                uint.TryParse(((string)rawValue).Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsedVal); // and if it can't be parsed correctly it will be 0...

                valueList.Add(parsedVal);
            }

            memoryWords.Data = valueList;
        }

        private void UpdateDedicatedObjectIfNecessary(CommandTicket currentCommandTicket, IDictionary<string, object> resultDict)
        {
            // nothing to do for the other tickets
            return;
        }
        /// <summary>
        /// Interprets the async notification.
        /// </summary>
        /// <param name="data">The data.</param>
        private void InterpretAsyncNotification(string data)
        {
            if (data.StartsWith(BreakpointCreatedNotification) || data.StartsWith(BreakpointModifiedNotification))
            {
                OnBreakpointUpdated(data);
                return;
            }

            if (data.StartsWith(BreakpointDeletedNotification))
            {
                OnBreakpointDeleted(data);
                return;
            }
        }
        /// <summary>
        /// Reacts on a gdb breakpoint-created message and updates the internal structures
        /// </summary>
        /// <param name="dataRecord">The data record.</param>
        /// <remarks>a breakpoint record comes in the form: breakpoint-created,bkpt={number="1",type="breakpoint",disp="keep",enabled="y",addr="0x0040138f",at="main+3",times="0",original-location="main"}</remarks>
        private void OnBreakpointUpdated(string dataRecord)
        {
            // look for the opening brace of the bkpt-info
            var bkptInfoStart = dataRecord.IndexOf('{');
            var bkptInfoEnd = dataRecord.IndexOf('}'); // we don't rely on the fact that the } is the last char - there can be trailing spaces...
            var bkptInfoData = dataRecord.Substring(bkptInfoStart + 1, bkptInfoEnd - bkptInfoStart - 1);

            var updatedRecord = new ResultRecordParser().Parse(bkptInfoData);
            var intermediateBreakpoint = new GdbBreakpoint("dummy") {RawInfo = updatedRecord};

            // gdb always delivers the breakpoint-data with the original-location attribute set, so the "dummy" location from above is for sure overridden...
            // (otherwise we wouldn't have a chance to find the correct breakpoint anyway...)
            var breakpointToUpdate = GdbControlInstance.Breakpoints.Breakpoint(intermediateBreakpoint.LocationIdentifier);
            breakpointToUpdate.RawInfo = intermediateBreakpoint.RawInfo; // this triggers all required update-operations
        }
        /// <summary>
        /// Reacts on a gdb breakpoint-deleted message and updates the internal structures
        /// </summary>
        /// <param name="dataRecord">The data record.</param>
        private void OnBreakpointDeleted(string dataRecord)
        {
            // look for the opening brace of the bkpt-info
            var bkptIdStart = dataRecord.IndexOf("id=\"", StringComparison.InvariantCulture);
            var bkptIdEnd = dataRecord.IndexOf('"', bkptIdStart + 4); // we don't rely on the fact that the closing " is the last char - there can be trailing spaces...
            var bkptId = int.Parse(dataRecord.Substring(bkptIdStart + 4, bkptIdEnd - bkptIdStart - 4));

            var breakpointToDelete = GdbControlInstance.Breakpoints.Breakpoint(bkptId);
            breakpointToDelete.Deleted = true;
            GdbControlInstance.Breakpoints.RemoveBreakpoint(breakpointToDelete);
        }

    }
}
