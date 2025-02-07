using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Consonya.Tools;
using Consonya.Tools.ExtensionMethods;
using Consonya.Tools.IO;

// Syntax: break -c (condition)
// Need: Context handling
// Messages: How to parse?
// Rainer for P40 target license

namespace Commands
{
    public class GdbControl : IDisposable
    {
        private const string DefaultGdbPath = "gdb.exe";
        private const string GdbArgs = "--interpreter mi"; 
        // start using the "machine interface" interpreter (for a description of mi see http://sourceware.org/gdb/current/onlinedocs/gdb/GDB_002fMI.html#GDB_002fMI)

        // the different commands for gdb
        private const string QuitGdbCmd = "quit";
        private const string SetBreakpointCmd = "break";

        // the different starting characters for different kinds of messages...
        private const char AsyncExecOutputStart = '*';
        private const char AsyncStatusOutputStart = '+';
        private const char AsyncNotificationOutputStart = '=';
        private const char ConsoleStreamOutputStart = '~';
        private const char TargetStreamOutputStart = '@';
        private const char LogStreamOutputStart = '&';
        private const char ResultRecordStart = '^';
        private const char ConsoleStreamOutputBlockDelimiterStart = '(';
        private const string ConsoleStreamOutputBlockDelimiterLine = "gdb)"; 
        // rest of (gdb) - the opening brace was already consumed by the dispatcher

        // the different prefixes for interpreted async notifications
        private const string BreakpointNotification = "breakpoint-created";

        private readonly Queue<char> GdbOutputDataBuffer = new Queue<char>();

        private Action HandleCurrentGdbOutputData { get; set; }

        private readonly DisposeHandler DisposeHandler;

        public readonly FilePath GdbPath;
        public readonly FilePath DebuggedProgramPath;
        public readonly SubdirPath WorkingDirectory;

        private Process GdbProcess { get; set; }

        public event Action<string> StderrOutput;

        public event Action<string> ResultRecord;
        public event Action<string> AsyncExec;
        public event Action<string> AsyncStatus;
        public event Action<string> AsyncNotification;
        public event Action<string> ConsoleStreamOutput;
        public event Action ConsoleStreamOutputBlockFinished;
        public event Action<string> LogStreamOutput;
        public event Action<string> TargetStreamOutput;

        public readonly Dictionary<int, GdbBreakpoint> Breakpoints = new Dictionary<int, GdbBreakpoint>();

        public GdbControl(FilePath debuggedProgramPath = null, SubdirPath workingDirectory = null, FilePath gdbPath = null)
        {
            DisposeHandler = new DisposeHandler(this, Cleanup);

            DebuggedProgramPath = debuggedProgramPath;
            WorkingDirectory = workingDirectory ?? new SubdirPath(Environment.CurrentDirectory);
            GdbPath = gdbPath ?? new FilePath(DefaultGdbPath);

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;
        }

        /// <summary>
        /// Starts the debugging. This method shares component already written.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public void StartDebugging(StringDictionary environment = null)
        {
            if (GdbProcess != null)
                throw new InvalidOperationException("debugging session already running");

            GdbProcess = new Process
                             {
                                 StartInfo =
                                     {
                                         FileName = GdbPath.ToString(),
                                         Arguments = GdbArgs + " \"" + DebuggedProgramPath + "\"",
                                         // What path should be chosen?
                                         WorkingDirectory = WorkingDirectory.ToString(),
                                         UseShellExecute = false,
                                         RedirectStandardInput = true,
                                         RedirectStandardOutput = true,
                                         RedirectStandardError = true,
                                         CreateNoWindow = true,
                                     },
                                 EnableRaisingEvents = true,
                             };

            if (environment != null)
                foreach (DictionaryEntry variable in environment)
                    GdbProcess.StartInfo.EnvironmentVariables[(string)variable.Key] = (string)variable.Value;

            GdbProcess.Disposed += (sender, args) => GdbProcess = null;
            GdbProcess.ErrorDataReceived += OnGdbErrorDataReceived;
            GdbProcess.OutputDataReceived += OnGdbOutputDataReceived;

            GdbProcess.Start();

            GdbProcess.BeginErrorReadLine();
                // never forget that, otherwise there will be no notification on stdout or stderr data!
            GdbProcess.BeginOutputReadLine();
        }

        public void StopDebugging()
        {
            if (GdbProcess == null)
                return;

            GdbProcess.StandardInput.WriteLine(QuitGdbCmd);
            GdbProcess.Dispose();
            GdbProcess = null;
        }

        public void SetBreakpoint(string methodName)
        {
            GdbProcess.StandardInput.WriteLine(SetBreakpointCmd + " " + methodName);
        }

        public void SendCustomCommand(string command)
        {
            GdbProcess.StandardInput.WriteLine(command);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            DisposeHandler.DisposeRequested();
        }

        ~GdbControl()
        {
            DisposeHandler.DestructorCalled();
        }

        #endregion

        private void Cleanup()
        {
            GdbProcess.Dispose();
        }

        private void OnGdbErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (args.Data == null) // happens from time to time...
                return;

            InvokeStderrOutput(args.Data);
        }

        private void OnGdbOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (args.Data == null) // happens when gdb is closed...
                return;

            args.Data.DoWithEach(GdbOutputDataBuffer.Enqueue);

            while (GdbOutputDataBuffer.Count > 0)
                HandleCurrentGdbOutputData();
        }

        private void DistributeGdbOutputDataByPrefix()
        {
            switch(GdbOutputDataBuffer.Dequeue())
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

        private void ReadAsyncNotificationOutput()
        {
            var data = new string(GdbOutputDataBuffer.Select(character => character).ToArray());
            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            InterpretAsyncNotification(data);

            InvokeAsyncNotification(data);
        }

        private void ReadAsyncExecOutput()
        {
            var data = new string(GdbOutputDataBuffer.Select(character => character).ToArray());
            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            InvokeAsyncExec(data);
        }

        private void ReadAsyncStatusOutput()
        {
            var data = new string(GdbOutputDataBuffer.Select(character => character).ToArray());
            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            InvokeAsyncStatus(data);
        }

        private void ReadConsoleStreamOutput()
        {
            var data = InterpretBufferAsCString();
            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;
            InvokeConsoleStreamOutput(data);
        }

        private void ReadConsoleStreamOutputBlockDelimiter()
        {
            var obtainedDelimiter = new string(GdbOutputDataBuffer.Select(character => character).ToArray());
            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            if (!obtainedDelimiter.StartsWith(ConsoleStreamOutputBlockDelimiterLine)) // have to use StartsWith rather than Equals because some gdb versions send a trailing space whereas others don't
                return; // just ignore it...
            InvokeConsoleStreamOutputBlockFinished();
        }

        private void ReadTargetStreamOutput()
        {
            var data = InterpretBufferAsCString();
            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;
            InvokeTargetStreamOutput(data);
        }

        private void ReadLogStreamOutput()
        {
            var data = InterpretBufferAsCString();
            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;
            InvokeLogStreamOutput(data);
        }

        private void ReadResultRecordOutput()
        {
            var data = new string(GdbOutputDataBuffer.Select(character => character).ToArray());
            GdbOutputDataBuffer.Clear();

            HandleCurrentGdbOutputData = DistributeGdbOutputDataByPrefix;

            InvokeResultRecord(data);
        }

        private string InterpretBufferAsCString()
        {
            GdbOutputDataBuffer.Dequeue(); // throw away the leading quote

            var data = new string(GdbOutputDataBuffer.Take(GdbOutputDataBuffer.Count - 1).Select(character => character).ToArray()); // don't take the last element because it's the trailing quote...
            GdbOutputDataBuffer.Clear();

            data = data.Replace("\\\"", "\""); // unescape escaped quotes...
            data = data.Replace(@"\n", Environment.NewLine); // gdb outputs \n as a string (i.e. '\' + 'n'), so we convert it back to newline again...
            return data;
        }

        private void InterpretAsyncNotification(string data)
        {
            if (data.StartsWith(BreakpointNotification))
            {
                RegisterBreakpoint(data);
            }
        }
        /// <summary>
        /// Registers the breakpoint.
        /// </summary>
        /// <param name="dataRecord">The data record.</param>
        /// <remarks>a breakpoint record comes in the form: breakpoint-created,bkpt={number="1",type="breakpoint",disp="keep",
        /// enabled="y",addr="0x0040138f",at="main+3",times="0",original-location="main"}</remarks>
        private void RegisterBreakpoint(string dataRecord)
        {
            // look for the opening brace of the bkpt-info
            var bkptInfoStart = dataRecord.IndexOf('{');
            var bkptInfoEnd = dataRecord.IndexOf('}'); // we don't rely on the fact that the } is the last char - there can be trailing spaces...
            var bkptInfoData = dataRecord.Substring(bkptInfoStart + 1, bkptInfoEnd - bkptInfoStart - 1);
            
            var recordEntries = RecordToDict(bkptInfoData);
            var breakpoint = new GdbBreakpoint(recordEntries);
            Breakpoints.Add(breakpoint.Id, breakpoint);
        }

        private IDictionary<string, string> RecordToDict(string bkptInfoData)
        {
            var parts = bkptInfoData.Split(new[] {','});
            var ret = new Dictionary<string, string>();
            foreach (var part in parts)
            {
                var separatorPos = part.IndexOf('=');
                var key = part.Substring(0, separatorPos);
                var value = part.Substring(separatorPos + 2, part.Length - separatorPos - 3); // cut the leading and trailing quotes
                ret.Add(key, value);
            }
            return ret;
        }

        private void InvokeStderrOutput(string data)
        {
            if (StderrOutput == null)
                return;
            StderrOutput(data);
        }

        private void InvokeResultRecord(string data)
        {
            if (ResultRecord == null)
                return;
            ResultRecord(data);
        }

        private void InvokeAsyncExec(string data)
        {
            if (AsyncExec == null)
                return;
            AsyncExec(data);
        }

        private void InvokeAsyncNotification(string data)
        {
            if (AsyncNotification == null)
                return;
            AsyncNotification(data);
        }

        private void InvokeAsyncStatus(string data)
        {
            if (AsyncStatus == null)
                return;
            AsyncStatus(data);
        }

        private void InvokeConsoleStreamOutput(string data)
        {
            if (ConsoleStreamOutput == null)
                return;
            ConsoleStreamOutput(data);
        }

        private void InvokeConsoleStreamOutputBlockFinished()
        {
            if (ConsoleStreamOutputBlockFinished == null)
                return;
            ConsoleStreamOutputBlockFinished();
        }

        private void InvokeLogStreamOutput(string data)
        {
            if (LogStreamOutput == null)
                return;
            LogStreamOutput(data);
        }

        private void InvokeTargetStreamOutput(string data)
        {
            if (TargetStreamOutput == null)
                return;
            TargetStreamOutput(data);
        }
    }
}
