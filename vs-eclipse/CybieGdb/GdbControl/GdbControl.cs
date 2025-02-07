using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Consonya.Tools;
using Consonya.Tools.IO;
using GdbControl;

namespace GdbControl
{
    public class GdbControl : IDisposable
    {
        private const string DefaultGdbPath = "gdb.exe";
        // Start using the machine-interface interpreter.
        // For a description of mi see http://sourceware.org/gdb/current/onlinedocs/gdb/GDB_002fMI.html#GDB_002fMI
        private const string GdbArgs = "--interpreter mi"; 

        // The different commands for gdb.
        private const string QuitGdbCmd = "quit";
        private const string RunCmd = "run";
        private const string ContinueCmd = "continue";
        private const string StopCmd = "stop";
        private const string SetBreakpointCmd = "break";
        private const string DeleteBreakpointCmd = "delete";
        private const string EvaluateDataCmd = "-data-evaluate-expression";
        private const string SetDataCmd = "set";
        private const string ReadMemoryCmd = "-data-read-memory";

        public enum WordSize
        {
            EightBits = 1,
            SixteenBits = 2,
            ThirtyTwoBits = 4
        }

        private readonly GdbStdoutInterpreter StdoutInterpreter;
        private readonly DisposeHandler DisposeHandler;

        public const int DefaultStopGdbTimeoutMillis = 5000;
        public int StopGdbTimeoutMillis { get; set; }

        public readonly FilePath GdbPath;
        public readonly FilePath DebuggedProgramPath;
        public readonly SubdirPath WorkingDirectory;

        private Process GdbProcess { get; set; }
        private Barrier GdbExitSyncBarrier { get; set; }
        private readonly object StopGdbReentranceLock = new object();

        public event Action<string> StderrOutput;
        public event Action<string> ResultRecord;
        public event Action<string> AsyncExecOutput;
        public event Action<string> AsyncStatus;
        public event Action<string> AsyncNotification;
        public event Action<string> ConsoleStreamOutput;
        public event Action ConsoleStreamOutputBlockFinished;
        public event Action<string> LogStreamOutput;
        public event Action<string> TargetStreamOutput;

        public readonly Breakpoints Breakpoints = new Breakpoints();
        internal readonly Queue<CommandTicket> PendingCommandTickets = new Queue<CommandTicket>();

        public GdbControl(FilePath debuggedProgramPath = null, SubdirPath workingDirectory = null, FilePath gdbPath = null)
        {
            DisposeHandler = new DisposeHandler(this, Cleanup);
            StdoutInterpreter = new GdbStdoutInterpreter(this);

            DebuggedProgramPath = debuggedProgramPath;
            WorkingDirectory = workingDirectory ?? new SubdirPath(Environment.CurrentDirectory);
            GdbPath = gdbPath ?? new FilePath(DefaultGdbPath);

            StopGdbTimeoutMillis = DefaultStopGdbTimeoutMillis;
        }
        /// <summary>
        /// Starts debugging.
        /// </summary>
        /// <param name="environment">Desired environment variables</param>
        /// <exception cref="InvalidOperationException">Thrown if debugging is already in progress</exception>
        public void StartDebugging(Dictionary<string, string> environment = null)
        {
            if (GdbProcess != null)
                throw new InvalidOperationException("debugging session already running.");

            GdbProcess = new Process
            {
                StartInfo =
                {
                    FileName = GdbPath.ToString(),
                    Arguments = GdbArgs + " \"" + DebuggedProgramPath + "\"",
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
            {
                foreach (var environmentVar in environment)
                    GdbProcess.StartInfo.EnvironmentVariables[environmentVar.Key] = environmentVar.Value;
            }

            GdbProcess.Exited += OnGdbExited;
            GdbProcess.ErrorDataReceived += OnGdbErrorDataReceived;
            GdbProcess.OutputDataReceived += StdoutInterpreter.OnGdbOutputDataReceived;

            GdbProcess.Start();

            GdbProcess.BeginErrorReadLine();
            GdbProcess.BeginOutputReadLine();
        }

        private void OnGdbExited(object sender, EventArgs args)
        {
            if (GdbExitSyncBarrier != null)
                GdbExitSyncBarrier.SignalAndWait(StopGdbTimeoutMillis);
        }
        /// <summary>
        /// Synchronously stops debugging and discards the current session
        /// </summary>
        /// <returns>The command ticket of the stop-command.</returns>
        public CommandTicket StopDebugging()
        {
            var ticket = new CommandTicket(QuitGdbCmd);

            lock (StopGdbReentranceLock)
            {
                if (GdbProcess == null)
                    return null;

                SendCommandToGdb(ticket); // this causes gdb to exit
                
                GdbExitSyncBarrier = new Barrier(2);
                GdbExitSyncBarrier.SignalAndWait(StopGdbTimeoutMillis); // synchronized shutdown: wait until gdb tells us that it exited
                GdbExitSyncBarrier.Dispose();
                GdbExitSyncBarrier = null;

                GdbProcess.Dispose();
                GdbProcess = null;

                Breakpoints.Clear();
            }
            return ticket;
        }
        /// <summary>
        /// Tells gdb to run the currently loaded program.
        /// </summary>
        /// <returns>The command ticket of the run command.</returns>
        public CommandTicket Run()
        {
            var ticket = new CommandTicket(RunCmd);
            SendCommandToGdb(ticket);
            return ticket;
        }
        /// <summary>
        /// Tells gdb to stop the currently loaded program.
        /// </summary>
        /// <returns>The command ticket of the stop command.</returns>
        public CommandTicket Stop()
        {
            var ticket = new CommandTicket(StopCmd);
            SendCommandToGdb(ticket);
            return ticket;
        }
        /// <summary>
        /// Tells gdb to continue the currently stopped program.
        /// </summary>
        /// <returns>The command ticket of the continue command.</returns>
        public CommandTicket Continue()
        {
            var ticket = new CommandTicket(ContinueCmd);
            SendCommandToGdb(ticket);
            return ticket;
        }
        /// <summary>
        /// Tells gdb to evaluate the given expression.
        /// </summary>
        /// <param name="expression">The expression to be evaluated.</param>
        /// <remarks>Don't forget to implicitly quote the expression.</remarks>
        public CommandTicket EvaluateExpression(string expression)
        {
            var ticket = new CommandTicket(EvaluateDataCmd + " " + expression);
            SendCommandToGdb(ticket);
            return ticket;
        }
        /// <summary>
        /// Tells gdb to set data by evaluating the given expression.
        /// </summary>
        /// <param name="expression">The expression to be evaluated.</param>
        /// <remarks>Don't forget to implicitly quote the expression.</remarks>
        public CommandTicket SetDataExpression(string expression)
        {
            var ticket = new CommandTicket(SetDataCmd + " " + expression);
            SendCommandToGdb(ticket);
            return ticket;
        }
        /// <summary>
        /// Reads the memory words.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="wordSize">Size of the word.</param>
        /// <param name="numWords">The number of words.</param>
        public CommandTicket<GdbMemoryWords> ReadMemoryWords(uint address, WordSize wordSize, int numWords)
        {
            var ticket = new CommandTicket<GdbMemoryWords>(
                ReadMemoryCmd // the command
                + " 0x" + address.ToString("X8") // arg0: address in hex
                + " x " // arg1: word-format - we want hex-numbers
                + " " + ((int)wordSize).ToString(CultureInfo.InvariantCulture) // arg2: word-size
                + " 1 " + numWords.ToString(CultureInfo.InvariantCulture), // arg3: rows (we want 1) + arg4: cols (the given number of words)
                new GdbMemoryWords()
                );
            SendCommandToGdb(ticket);
            return ticket;
        }
        /// <summary>
        /// Sets a breakpoint.
        /// </summary>
        /// <param name="locationIdentifier">Identifier of the breakpoint location, i.e. the name of the method or an address.</param>
        /// <returns>The command ticket of the set breakpoint command including the resulting breakpoint object.</returns>
        public CommandTicket<GdbBreakpoint> SetBreakpoint(string locationIdentifier)
        {
            // Create a placeholder and register it, so that it can be filled when the answer arrives.
            var breakpoint = new GdbBreakpoint(locationIdentifier);
            Breakpoints.AddBreakpoint(breakpoint);

            var ticket = new CommandTicket<GdbBreakpoint>(SetBreakpointCmd + " " + locationIdentifier, breakpoint);
            SendCommandToGdb(ticket);
            return ticket;
        }
        /// <summary>
        /// Deletes the breakpoint.
        /// </summary>
        /// <param name="breakpoint">The breakpoint.</param>
        public CommandTicket<GdbBreakpoint> DeleteBreakpoint(GdbBreakpoint breakpoint)
        {
            if (!breakpoint.Acknowledged)
                throw new InvalidOperationException("can't delete unacknowledged breakpoint");

            var ticket = new CommandTicket<GdbBreakpoint>(DeleteBreakpointCmd + " " + breakpoint.Id, breakpoint);
            SendCommandToGdb(ticket);
            return ticket;
        }
        /// <summary>
        /// Sends a custom command to gdb
        /// </summary>
        /// <param name="command">The command.</param>
        /// <exception cref="ArgumentNullException">Thrown if command is null</exception>
        public CommandTicket SendCustomCommandToGdb(string command)
        {
            if (command == null)
                throw new ArgumentNullException();

            var ticket = new CommandTicket(command);
            SendCommandToGdb(ticket);
            return ticket;
        }
        /// <summary>
        /// Sends the command to GDB.
        /// </summary>
        /// <param name="ticket">The command ticket.</param>
        private void SendCommandToGdb(CommandTicket ticket)
        {
            PendingCommandTickets.Enqueue(ticket);
            GdbProcess.StandardInput.WriteLine(ticket.Command);
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

        /// <summary>
        /// Cleans up this instance.
        /// </summary>
        private void Cleanup()
        {
            if (GdbProcess != null)
                StopDebugging();
        }
        /// <summary>
        /// Called on receiving data on gdb's stderr.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Diagnostics.DataReceivedEventArgs"/> instance containing the event data.</param>
        private void OnGdbErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (args.Data == null)
                return;

            InvokeStderrOutput(args.Data);
        }
        /// <summary>
        /// Kills the GDB process.
        /// </summary>
        public void KillGdbProcess()
        {
            lock (StopGdbReentranceLock)
            {
                GdbExitSyncBarrier = new Barrier(2);

                // GdbExitSyncBarrier.SignalAndWait(StopGdbTimeoutMillis); // synchronized shutdown: wait until gdb tells us that it exited

                GdbExitSyncBarrier.Dispose();
                GdbExitSyncBarrier = null;

                // Now that it exited, we can dispose it (ok, if we ran into the timeout, we dispose it anyway.
                GdbProcess.Kill();
                GdbProcess.Dispose();
                GdbProcess = null;

                Breakpoints.Clear();
            }
        }
        /// <summary>
        /// Interprets the buffer as C string.
        /// </summary>
        internal static string InterpretBufferAsCString(Queue<char> buffer)
        {
            buffer.Dequeue(); // throw away the leading quote

            var data = new string(buffer.Take(buffer.Count - 1).Select(character => character).ToArray()); // don't take the last element because it's the trailing quote...
            buffer.Clear();

            data = data.Replace("\\\"", "\""); // unescape escaped quotes...
            data = data.Replace(@"\n", Environment.NewLine); // gdb outputs \n as a string (i.e. '\' + 'n'), so we convert it back to newline again...
            return data;
        }
        /// <summary>
        /// Event-invocator for StderrOutput
        /// </summary>
        /// <param name="data">The data.</param>
        private void InvokeStderrOutput(string data)
        {
            if (StderrOutput == null)
                return;
            StderrOutput(data);
        }
        /// <summary>
        /// Event-invocator for ResultRecord
        /// </summary>
        /// <param name="data">The data.</param>
        internal void InvokeResultRecord(string data)
        {
            if (ResultRecord == null)
                return;
            ResultRecord(data);
        }
        /// <summary>
        /// Event-invocator for AsyncExec
        /// </summary>
        /// <param name="data">The data.</param>
        internal void InvokeAsyncExec(string data)
        {
            if (AsyncExecOutput == null)
                return;
            AsyncExecOutput(data);
        }
        /// <summary>
        /// Event-invocator for AsyncNotification
        /// </summary>
        /// <param name="data">The data.</param>
        internal void InvokeAsyncNotification(string data)
        {
            if (AsyncNotification == null)
                return;
            AsyncNotification(data);
        }
        /// <summary>
        /// Event-invocator for AsyncStatus
        /// </summary>
        /// <param name="data">The data.</param>
        internal void InvokeAsyncStatus(string data)
        {
            if (AsyncStatus == null)
                return;
            AsyncStatus(data);
        }
        /// <summary>
        /// Event-invocator for ConsoleStreamOutput
        /// </summary>
        /// <param name="data">The data.</param>
        internal void InvokeConsoleStreamOutput(string data)
        {
            if (ConsoleStreamOutput == null)
                return;
            ConsoleStreamOutput(data);
        }
        /// <summary>
        /// Event-invocator for ConsoleStreamOutputBlockFinished
        /// </summary>
        internal void InvokeConsoleStreamOutputBlockFinished()
        {
            if (ConsoleStreamOutputBlockFinished == null)
                return;
            ConsoleStreamOutputBlockFinished();
        }
        /// <summary>
        /// Event-invocator for LogStreamOutput
        /// </summary>
        /// <param name="data">The data.</param>
        internal void InvokeLogStreamOutput(string data)
        {
            if (LogStreamOutput == null)
                return;
            LogStreamOutput(data);
        }
        /// <summary>
        /// Event-invocator for TargetStreamOutput
        /// </summary>
        /// <param name="data">The data.</param>
        internal void InvokeTargetStreamOutput(string data)
        {
            if (TargetStreamOutput == null)
                return;
            TargetStreamOutput(data);
        }
    }
}
