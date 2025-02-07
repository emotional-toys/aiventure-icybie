using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Consonya.Tools.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Consonya.MSTestTools;

namespace GdbControl.Tests
{
    [TestClass]
    public class GdbControlTests
    {
        public static readonly SubdirPath DefaultAppRootPath = new SubdirPath(System.Environment.CurrentDirectory);
        public static readonly SubdirPath DebuggedProgramRelativePath = new SubdirPath(@".\gdb");
        public static readonly SubdirPath DebuggedProgramRelativeWorkingDir = DebuggedProgramRelativePath;
        public static readonly Filename DebuggedProgramFilename = new Filename("GdbControl.Sanity.exe");
        public static readonly SubdirPath GdbAbsolutePath = new SubdirPath(@"C:\MinME\bin");
        public static readonly Filename GdbFilename = new Filename("gdb.exe");
        public static readonly Dictionary<string, string> Environment = new Dictionary<string, string>
                                                                   {
                                                                       {"Path", "F:/PROJECT_MYNE/interfaces/Vault/Software/04_Tools/GOP_HAL/12_Devenv/shared/nxprdlib"},
                                                                       {"DEVENV", "F:/PROJECT_MYNE/interfaces/Vault/Software/04_Tools/GOP_HAL/12_Devenv"}
                                                                   };
        private List<string> AsyncExecOut { get; set; }
        private List<string> AsyncNotification { get; set; }
        private List<string> AsyncStatus { get; set; }
        private List<string> ConsoleStreamOut { get; set; }
        private List<string> LogStreamOut { get; set; }
        private List<string> ResultRecord { get; set; }
        private List<string> StderrOut { get; set; }
        private List<string> TargetStreamOut { get; set; }
        private List<int> ConsoleStreamOutputBlockMarkers { get; set; }
        public bool DebuggingStarted;

        [TestMethod]
        public void Construction()
        {
            var debuggedProgramPath = new FilePath(new SubdirPath(@"c:\whatever\dir"), new Filename(@"whateverProgram"));
            
            var workingDir = new SubdirPath(@"f:\PROJECT_MYNE\interfaces\Vault\Software\01_Development\DesFireEV2\");
            var gdbPath = new FilePath(new SubdirPath(@"c:\Users\nxp51553\myneProjectDirectory\gdb\gdb.exe"), new Filename(@"GnuDebugger"));

            var targetGdbControl = new global::GdbControl.GdbControl(null, workingDir, gdbPath);

            
        }

        public global::GdbControl.GdbControl CreateGdbControlAndConnectEvents(Dictionary<string, string> environment)
        {
            var targetProgram =
                new FilePath(
                    new SubdirPath(
                        @"c:\aeonWorkingDirectory\bph\cybie\vs\CybieGdb\Debug\"),
                    new Filename("GdbControl.Sanity.exe"));//STOPPED HERE
            var workingDir = new SubdirPath(@"f:\PROJECT_MYNE\interfaces\Vault\Software\01_Development\DesFireEV2\");
            var gdbPath = new FilePath(new SubdirPath(@"C:\MinME\bin"), new Filename(@"gdb.exe"));
            // connect to old hal and get running to set breakpoints


            AsyncExecOut = new List<string>();
            AsyncNotification = new List<string>();
            AsyncStatus = new List<string>();
            ConsoleStreamOut = new List<string>();
            LogStreamOut = new List<string>();
            ResultRecord = new List<string>();
            StderrOut = new List<string>();
            TargetStreamOut = new List<string>();
            ConsoleStreamOutputBlockMarkers = new List<int>();

            //var debuggedProgramPath = new FilePath(DefaultAppRootPath + DebuggedProgramRelativePath, DebuggedProgramFilename);
            //var workingDir = DefaultAppRootPath + DebuggedProgramRelativeWorkingDir;
            //var gdbPath = new FilePath(GdbAbsolutePath, GdbFilename);

            var gdbControl = new global::GdbControl.GdbControl(targetProgram, workingDir, gdbPath) { StopGdbTimeoutMillis = 100000 }; // make the timeout really long in order to make sure the testcase signals a problem if e.g. gdb doesn't stop
            //var gdbControl = new GdbControl(null, workingDir, gdbPath) { StopGdbTimeoutMillis = 100000 };
            
            gdbControl.AsyncExecOutput += AsyncExecOut.Add;
            gdbControl.AsyncNotification += AsyncNotification.Add;
            gdbControl.AsyncStatus += AsyncStatus.Add;
            gdbControl.ConsoleStreamOutput += ConsoleStreamOut.Add;
            gdbControl.LogStreamOutput += LogStreamOut.Add;
            gdbControl.ResultRecord += ResultRecord.Add;
            gdbControl.StderrOutput += StderrOut.Add;
            gdbControl.TargetStreamOutput += TargetStreamOut.Add;
            gdbControl.ConsoleStreamOutputBlockFinished += () => ConsoleStreamOutputBlockMarkers.Add(ConsoleStreamOut.Count);

            

            return gdbControl;
        }

        [TestMethod]
        [Timeout(5000)]
        public void StartStopDebugging()
        {
            using (var targetGdbControl = CreateGdbControlAndConnectEvents(Environment))
            {
                targetGdbControl.StartDebugging();
                // StopDebugging or also Dispose, if necessary, synchronize with gdb's exit-status. Therefore we can be
                // sure that all data sent from gdb definitely arrived in our process when the method returns.
                targetGdbControl.StopDebugging();
            }

            Thread.Sleep(2000); // sorry, but there is no way to synchronize on delivery of all async-reads, so we simply wait... 

            var testSequence = new Action[]
            {
                () => Assert.AreEqual(1, ConsoleStreamOutputBlockMarkers.Count), // we only expect one block: the one with gdb's output about its version, copyright, etc. (see below)
                () => Assert.AreEqual(ConsoleStreamOutputBlockMarkers[0], ConsoleStreamOut.Count), // many lines about gdb, its version, copyright, etc.
                () => Assert.IsTrue(ConsoleStreamOut[0].StartsWith("GNU gdb")), // and those lines start with the debugger's signature...
                
                () => Assert.AreEqual(1, AsyncNotification.Count), // 1 async notification about an added thread-group
                () => Assert.IsTrue(AsyncNotification[0].StartsWith("thread-group-added")),

                () => Assert.AreEqual(1, LogStreamOut.Count), // 1 log output: our "quit" command
                () => Assert.IsTrue(LogStreamOut[0].StartsWith("quit")),

                () => Assert.AreEqual(0, AsyncExecOut.Count),
                () => Assert.AreEqual(0, AsyncStatus.Count),
                () => Assert.AreEqual(0, ResultRecord.Count),
                () => Assert.AreEqual(0, StderrOut.Count),
                () => Assert.AreEqual(0, TargetStreamOut.Count),
            };
            Consonya.MSTestTools.AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        [Timeout(5000)]
        public void SendCustomCommandToGdb()
        {
            CommandTicket ticket1;
            CommandTicket ticket2;
            CommandTicket ticket3;
            CommandTicket ticket4;

            using (var targetGdbControl = CreateGdbControlAndConnectEvents(Environment))
            {
                targetGdbControl.StartDebugging();

                // Start gdb file setup_devenv.cmd check to use simulator, shared, gdb, (christopher)

                targetGdbControl.ConsoleStreamOutput += (msg) => System.Diagnostics.Debug.Write(msg);

                var sourceTicket = targetGdbControl.SendCustomCommandToGdb("source DesFireEV2/platform/.gdbinit_t508");
                sourceTicket.WaitUntilDone();
                
                var runTicket = targetGdbControl.Run();
                runTicket.WaitUntilDone();
                ticket1 = targetGdbControl.SendCustomCommandToGdb("output 420");
                ticket2 = targetGdbControl.SendCustomCommandToGdb("-data-evaluate-expression 420");
                ticket3 = targetGdbControl.SendCustomCommandToGdb("-data-evaluate-expression 667");
                //ticket4 = targetGdbControl.SendCustomCommandToGdb("break *0xdd");
                
                // if we wanted, we could wait for each ticket, but due to our synchronization mechanism it's enough to wait for the last
                // one to be sure that the others are set to done
                ticket1.WaitUntilDone();
                ticket2.WaitUntilDone(); 
                ticket3.WaitUntilDone();
                //ticket4.WaitUntilDone();

                targetGdbControl.StopDebugging();
            }

            Thread.Sleep(2000); // sorry, but there is no way to synchronize on delivery of all async-reads, so we simply wait... 

            var testSequence = new Action[]
                {
                    () => Assert.AreEqual(0, ticket1.ResultRecord.Count), // output 42 does not deliver a result, it only triggers output (that we would find in gdb's output stream)
                    () => Assert.AreEqual("420", ticket2.ResultRecord["value"]), // -data-evaluate-expression delivers a result
                    () => Assert.AreEqual("667", ticket3.ResultRecord["value"]), // -data-evaluate-expression delivers a result
                };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }
        // Next after duplicating what was established in the 'ExecuteCommmand' WinForm project.
        [TestMethod]
        [Timeout(5000)]
        public void SetBreakpoint()
        {
            using (var targetGdbControl = CreateGdbControlAndConnectEvents(Environment))
            {
                targetGdbControl.StartDebugging();

                var breakpoint1 = targetGdbControl.SetBreakpoint("main").DedicatedObject;
                var breakpoint2 = targetGdbControl.SetBreakpoint("Function1").DedicatedObject;
                var breakpoint3 = targetGdbControl.SetBreakpoint("Function2").DedicatedObject;

                // wait until the breakpoints are acknowledged from gdb
                breakpoint1.WaitForAcknowledgement();
                breakpoint2.WaitForAcknowledgement();
                breakpoint3.WaitForAcknowledgement(); 

                var testSequence = new Action[]
                {
                    () => Assert.AreEqual(1, targetGdbControl.Breakpoints.Breakpoint("main").Id), // the first breakpoint gets id 1
                    () => Assert.AreEqual(2, targetGdbControl.Breakpoints.Breakpoint("Function1").Id), // the second breakpoint gets id 2
                    () => Assert.AreEqual(3, targetGdbControl.Breakpoints.Breakpoint("Function2").Id), // the third breakpoint gets id 3
                };
                AssertHelpers.ExecuteTestSequence(testSequence);

                targetGdbControl.StopDebugging();
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void DeleteBreakpoint()
        {
            using (var targetGdbControl = CreateGdbControlAndConnectEvents(Environment))
            {
                targetGdbControl.StartDebugging();

                var breakpoint1 = targetGdbControl.SetBreakpoint("main").DedicatedObject;

                // wait until the breakpoint is acknowledged from gdb
                breakpoint1.WaitForAcknowledgement();

                targetGdbControl.DeleteBreakpoint(breakpoint1);

                breakpoint1.WaitForDeletion();

                var testSequence = new Action[]
                {
                    () => Assert.IsTrue(breakpoint1.Deleted),
                };
                AssertHelpers.ExecuteTestSequence(testSequence);

                targetGdbControl.StopDebugging();
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void RunStopContinueCommands()
        {
            using (var targetGdbControl = CreateGdbControlAndConnectEvents(Environment))
            {
                targetGdbControl.StartDebugging();

                var breakpointInfo = targetGdbControl.SetBreakpoint("main").DedicatedObject;
                var currentBreakpointHits = breakpointInfo.HitCount;
                var ticket2 = targetGdbControl.Run(); // (re-)runs the debuggee
                breakpointInfo.WaitForHitsToExceed(currentBreakpointHits); // waits for the next breakpoint hit
                var ticket3 = targetGdbControl.Continue(); // continues after the hit
                ticket3.WaitUntilDone();

                targetGdbControl.StopDebugging();
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void EvaluateExpressionCommand()
        {
            using (var targetGdbControl = CreateGdbControlAndConnectEvents(Environment))
            {
                targetGdbControl.StartDebugging();

                var ticket1 = targetGdbControl.EvaluateExpression("420");
                ticket1.WaitUntilDone();

                var ticket2 = targetGdbControl.EvaluateExpression("globalVar1");
                ticket2.WaitUntilDone();

                var testSequence = new Action[]
                {
                    () => Assert.AreEqual("42", ticket1.ResultRecord["value"]),
                    () => Assert.AreEqual(0x01234567.ToString(CultureInfo.InvariantCulture), ticket2.ResultRecord["value"]),
                };
                AssertHelpers.ExecuteTestSequence(testSequence);

                targetGdbControl.StopDebugging();
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void SetDataExpressionCommand()
        {
            using (var targetGdbControl = CreateGdbControlAndConnectEvents(Environment))
            {
                targetGdbControl.StartDebugging();
                var breakpoint = targetGdbControl.SetBreakpoint("main").DedicatedObject;
                targetGdbControl.Run(); // Attention! The program must be running, otherwise data cannot be set!
                breakpoint.WaitForHitsToExceed(0); // although technically not necessary, it is a good idea to have the program stopped in a breakpoint when changing data for the sake of data-integrity :-)

                var ticket0 = targetGdbControl.SetDataExpression("globalVar1=667");
                var ticket1 = targetGdbControl.SetDataExpression("((char*)&globalVar3)[2]=88");

                var ticket2 = targetGdbControl.EvaluateExpression("globalVar1");
                ticket2.WaitUntilDone();

                var ticket3 = targetGdbControl.EvaluateExpression("((char*)&globalVar3)[2]");
                ticket3.WaitUntilDone();

                var testSequence = new Action[]
                {
                    () => Assert.AreEqual("667", ticket2.ResultRecord["value"]),
                    () => Assert.IsTrue(((string)ticket3.ResultRecord["value"]).StartsWith("88")), // evaluate gives us something like "88 X" with 'X' being the character with code 88
                };
                AssertHelpers.ExecuteTestSequence(testSequence);

                targetGdbControl.StopDebugging();
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void ReadMemoryWords()
        {
            using (var targetGdbControl = CreateGdbControlAndConnectEvents(Environment))
            {
                targetGdbControl.StartDebugging();

                var ticket1 = targetGdbControl.EvaluateExpression("&globalVar1"); // query the address of the variable...
                ticket1.WaitUntilDone();
                var addressOfVar1 = uint.Parse(((string) ticket1.ResultRecord["value"]).Substring(2), NumberStyles.HexNumber);

                var result1 = targetGdbControl.ReadMemoryWords(addressOfVar1, global::GdbControl.GdbControl.WordSize.ThirtyTwoBits, 1).DedicatedObject;
                result1.WaitUntilDataAvailable();

                var ticket2 = targetGdbControl.EvaluateExpression("&globalVar3"); // query the address of the variable...
                ticket2.WaitUntilDone();
                var addressOfVar3 = uint.Parse(((string)ticket2.ResultRecord["value"]).Substring(2), NumberStyles.HexNumber);

                var result2 = targetGdbControl.ReadMemoryWords(addressOfVar3, global::GdbControl.GdbControl.WordSize.EightBits, 4).DedicatedObject;
                result2.WaitUntilDataAvailable();

                var testSequence = new Action[]
                {
                    () => Assert.AreEqual(0x01234567u, result1.Data[0]),
                    () => AssertHelpers.AssertAreSequenceEqual(new uint[]{0x10, 0x20, 0x30, 0x40 }, result2.Data),
                };
                AssertHelpers.ExecuteTestSequence(testSequence);

                targetGdbControl.StopDebugging();
            }
        }

        
    }
}
