using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Consonya.GdbControl;
using Consonya.Tools.IO;
using DESFireEV2_Testbench.Adapters;
//using NUnit.Framework;
using Consonya.TestTools.MDD;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPRdBuilder.Toolbox.NxpRdLibNetEx;
using DESFireEV2_Testbench;
using System;
using TestTools;

namespace Consonya.GdbControl.Tests
{
    
    
    /// <summary>
    ///This is a test class for GdbFunctionTestsTest and is intended
    ///to contain all GdbFunctionTestsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GdbFunctionTestsTest
    {
        #region MemLayout
        
        /*                   */
        /* NV Memory Layout  */
        /*                   */
        const ushort OSDEFINITIONS_EEPROMBASEADDRESS = 0x8000;
        const ushort OSDEFINITIONS_EEPROMSIZE = 0x21A0;
        const ushort gpphMemoryLayout_FileSystemBottom = OSDEFINITIONS_EEPROMBASEADDRESS;
        const ushort gpphMemoryLayout_FileSystemTop = gpphMemoryLayout_FileSystemBottom + OSDEFINITIONS_EEPROMSIZE;
        const ushort phSystemInformationBlock0Length = 32;
        const ushort phSystemInformationBlock1Length = 32;
        const ushort phMasterFileKeyStoreLength = 0x120;
        const ushort phRestoreAreaLength = 64;
        const ushort gpphMemoryLayout_SysInfoBlock0 = gpphMemoryLayout_FileSystemTop - phSystemInformationBlock0Length;
        const ushort gpphMemoryLayout_SysInfoBlock1 = gpphMemoryLayout_SysInfoBlock0 - phSystemInformationBlock1Length;
        const ushort gpphMemoryLayout_MasterFileKeyStore = gpphMemoryLayout_SysInfoBlock1 - phMasterFileKeyStoreLength;
        const ushort gpphMemoryLayout_RestoreData = gpphMemoryLayout_MasterFileKeyStore - phRestoreAreaLength;

        const ushort refANode = gpphMemoryLayout_SysInfoBlock1 + 24;
        const ushort refBottom = refANode + 2;
        const ushort refTop = refBottom + 2;

        #endregion

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for RunGdbSequence
        ///</summary>
        [TestMethod()]
        public void RunGdbSequence()
        {

            #region Setup
            var devEnvPath = new SubdirPath(@"f:\PROJECT_MYNE\interfaces\Vault\Software\04_Tools\GOP_HAL\12_Devenv");
            var simulatorPath = devEnvPath + new SubdirPath("simulator");
            var gdbPath = devEnvPath + new SubdirPath(@"shared\gdb");
            var nxpRdLibPath = devEnvPath + new SubdirPath(@"shared\nxprdlib");
            var gdbFilePath = new FilePath(gdbPath, new Filename(@"gdb.exe"));

            //var targetProgram =
            //    new FilePath(
            //        new SubdirPath(
            //            @"f:\PROJECT_MYNE\interfaces\Vault\Software\01_Development\DesFireEV2\GOPHal\bin\t508\"),
            //        new Filename("P40_SystemMode_t508"));
            var workingDir = new SubdirPath(@"f:\PROJECT_MYNE\interfaces\Vault\Software\01_Development\DF_EV2\conf\");

            var targetProgram =
                new FilePath(
                    new SubdirPath(
                        @"f:\PROJECT_MYNE\interfaces\Vault\Software\01_Development\DesFireEV2\GOPHal\bin\t508\P40C072"),
                    new Filename("P40_DesFireEV2_C072"));
            //var workingDir = new SubdirPath(@"f:\PROJECT_MYNE\interfaces\Vault\Software\01_Development\DesFireEV2\DesFireEV2\platform\");
            
            var environment = new Dictionary<string, string>
                                  {
                                      {"Path", simulatorPath.ToString() + ";" + gdbPath.ToString() + ";" + nxpRdLibPath.ToString()},
                                      {"DEVENV", devEnvPath.ToString()}
                                  };

            #endregion

            using (var gdbControl = new GdbControl(targetProgram, workingDir, gdbFilePath) { StopGdbTimeoutMillis = 100000 })
            {
                CommandTicket breakpointTicket1;
                CommandTicket breakpointTicket2;
                CommandTicket deleteBreakPointTicket;
                CommandTicket runTicket;
                CommandTicket printTicket;
                CommandTicket evaluateTicket1;
                CommandTicket evaluateTicket2;
                CommandTicket evaluateTicket3;
                CommandTicket createVarTicket1;
                CommandTicket assignVarlTicket1;
                CommandTicket continueTicket;

                gdbControl.ConsoleStreamOutput += (msg) => Debug.Write(msg);
                gdbControl.StderrOutput += (message) => Debug.Write(message);
                gdbControl.LogStreamOutput += (mess) => Debug.Write(mess);

                gdbControl.StartDebugging(environment);
                //var sourceTicket = gdbControl.SendCustomCommandToGdb("source DesFireEV2/platform/.gdbinit_t508");
                var sourceTicket = gdbControl.SendCustomCommandToGdb("source DesFireEV2/platform/.gdbinit");
                sourceTicket.WaitUntilDone();

                #region Connection primitives

                ////var secureMessagingPart = new SecurityViewAdapterTriggerMapper("Secure Messaging part", new SecurityViewAdapter());
                //var applicationPart = new ApplicationManagementViewAdapterTriggerMapper("Application part", new ApplicationManagementViewAdapter());
                //var filePart = new FileManagementViewAdapterTriggerMapper("File part", new FileManagementViewAdapter());
                //var miscPart = new Iso14443ViewAdapterTriggerMapper("Misc part", new Iso14443ViewAdapter());
                //var halPart = new HalViewAdapterTriggerMapper("Hal part", new HalViewAdapter());
                //var gdbPart = new GdbViewAdapterTriggerMapper("Gdb part", new GdbViewAdapter());
                //var card = new CardViewAdapterTriggerMapper("Card", new CardViewAdapter());

                #endregion
                
                breakpointTicket1 = gdbControl.SetBreakpoint("*0xdd");
                breakpointTicket1.WaitUntilDone();

                runTicket = gdbControl.Run();
                runTicket.WaitUntilDone();
                
                evaluateTicket1 = gdbControl.EvaluateExpression("*(&phhalMem_TransitionByte)");
                evaluateTicket1.WaitUntilDone();

                // Check the evaluation.
                const string expectedOutputValue1 = @"149 '\225'";
                var gdbOutputValue1 = DictionaryParser.ParseValue<string, object>(evaluateTicket1.ResultRecord, "value");
                Assert.AreEqual(expectedOutputValue1, gdbOutputValue1);

                printTicket = gdbControl.SendCustomCommandToGdb("print /x *0x1af");
                printTicket.WaitUntilDone();

                createVarTicket1 = gdbControl.SendCustomCommandToGdb("-var-create - * *(&phhalMem_TransitionByte)");
                createVarTicket1.WaitUntilDone();

                assignVarlTicket1 = gdbControl.SendCustomCommandToGdb("-var-assign var1 88");
                assignVarlTicket1.WaitUntilDone();

                evaluateTicket2 = gdbControl.EvaluateExpression("*(&phhalMem_TransitionByte)");
                evaluateTicket2.WaitUntilDone();

                // Check the evaluation.
                const string expectedOutputValue2 = @"88 'X'";
                var gdbOutputValue2 = DictionaryParser.ParseValue<string, object>(evaluateTicket2.ResultRecord, "value");
                Assert.AreEqual(expectedOutputValue2, gdbOutputValue2);

                deleteBreakPointTicket = gdbControl.SendCustomCommandToGdb("-break-delete");
                deleteBreakPointTicket.WaitUntilDone();

                breakpointTicket2 = gdbControl.SetBreakpoint("*0xdf");
                breakpointTicket2.WaitUntilDone();

                continueTicket = gdbControl.Continue();
                continueTicket.WaitUntilDone();

                evaluateTicket3 = gdbControl.SendCustomCommandToGdb("-data-evaluate-expression *(&phhalMem_TransitionByte)");
                evaluateTicket3.WaitUntilDone();

                // Check the evaluation.
                const string expectedOutputValue3 = @"0 '\\000'";
                var gdbOutputValue3 = DictionaryParser.ParseValue<string, object>(evaluateTicket3.ResultRecord, "value");
                Assert.AreEqual(expectedOutputValue3, gdbOutputValue3);


                var hold = "";

                gdbControl.StopDebugging();

            }
        }

        /// <summary>
        ///A test for RunGdbSequence
        ///</summary>
        [TestMethod()]
        public void RunGdbGetVersionExampleSequence()
        {

            #region Setup
            var devEnvPath = new SubdirPath(@"J:\DESFireEV2\Software\04_Tools\GOP_HAL\12_Devenv");
            var simulatorPath = devEnvPath + new SubdirPath(@"t508\simulator");
            var gdbPath = devEnvPath + new SubdirPath(@"shared\gdb");
            var nxpRdLibPath = devEnvPath + new SubdirPath(@"shared\nxprdlib");
            var gdbFilePath = new FilePath(gdbPath, new Filename(@"gdb.exe"));

            var targetProgram =
                new FilePath(
                    new SubdirPath(
                        @"J:\DESFireEV2\Software\01_Development\DesFireEV2\DesFireEV2\P40C072\"),
                    new Filename("P40_DesFireEV2_C072"));
            var workingDir = new SubdirPath(@"J:\DESFireEV2\Software\01_Development\DesFireEV2\");

            //var targetProgram =
            //    new FilePath(
            //        new SubdirPath(
            //            @"f:\PROJECT_MYNE\interfaces\Vault\Software\01_Development\DesFireEV2\GOPHal\bin\t508\P40C072"),
            //        new Filename("P40_DesFireEV2_C072"));
            //
            //var workingDir = new SubdirPath(@"f:\PROJECT_MYNE\interfaces\Vault\Software\01_Development\DF_EV2\conf\");

            var environment = new Dictionary<string, string>
                                  {
                                      {"Path", simulatorPath.ToString() + ";" + gdbPath.ToString() + ";" + nxpRdLibPath.ToString()},
                                      {"DEVENV", devEnvPath.ToString()}
                                  };

            #endregion

            using (var gdbControl = new GdbControl(targetProgram, workingDir, gdbFilePath) { StopGdbTimeoutMillis = 100000 })
            {
                CommandTicket breakpointTicket1;
                CommandTicket breakpointTicket2;
                CommandTicket deleteBreakPointTicket;
                CommandTicket runTicket;
                CommandTicket printTicket;
                CommandTicket evaluateTicket1;
                CommandTicket evaluateTicket2;
                CommandTicket evaluateTicket3;
                CommandTicket createVarTicket1;
                CommandTicket assignVarlTicket1;
                CommandTicket continueTicket;
                CommandTicket stopTicket;

                gdbControl.ConsoleStreamOutput += (msg) => Debug.Write(msg);
                gdbControl.StderrOutput += (message) => Debug.Write(message);
                gdbControl.LogStreamOutput += (mess) => Debug.Write(mess);

                gdbControl.StartDebugging(environment);
                var sourceTicket = gdbControl.SendCustomCommandToGdb("source DesFireEV2/platform/.gdbinit_t508");
                sourceTicket.WaitUntilDone();

                breakpointTicket1 = gdbControl.SendCustomCommandToGdb("-break-insert phCommandSet_GetVersion.c:72");
                breakpointTicket1.WaitUntilDone();
                //var breakpoint1 = gdbControl.SetBreakpoint("phCommandSet_GetVersion.c:72").DedicatedObject;
                //breakpoint1.WaitForAcknowledgement();

                //break in inline function needs an address
                breakpointTicket1 = gdbControl.SetBreakpoint("*0x069e");
                //breakpointTicket1 = gdbControl.SetBreakpoint("*1694");
                breakpointTicket1.WaitUntilDone();
                //var breakpoint2 = gdbControl.SetBreakpoint("*1694").DedicatedObject;
                //breakpoint2.WaitForAcknowledgement();
                
                runTicket = gdbControl.Run();
                runTicket.WaitUntilDone();

                var sutConnector = new AdapterBase(Properties.Resources.TestbenchConfig, Properties.Resources.CardConfig, true);

                byte[] uid;
                byte[] ats;
                byte sak;
                byte more;
                byte[] version;

                sutConnector.Hal.FieldReset().ThrowOnError();
                sutConnector.palI14443p3a.ActivateCard(null, out uid, out sak, out more).ThrowOnError();
                sutConnector.palI14443p4a.ActivateCard(0x08, 0x01, 0x01, 0x01, out ats).ThrowOnError();
                sutConnector.palI14443p4.SetProtocol(0x01, 0x01, 0x00, 0x00, 0x04, 0x08, 0x05).ThrowOnError();
                
                AsyncExchange.BeginAsyncExchange(() => sutConnector.Al.GetVersion(out version));
                
                deleteBreakPointTicket = gdbControl.SendCustomCommandToGdb("-break-delete 1");
                deleteBreakPointTicket.WaitUntilDone();

                continueTicket = gdbControl.Continue();
                continueTicket.WaitUntilDone();

                //-data-evaluate-expression *(&pbIoData)
                //evaluateTicket1 = gdbControl.EvaluateExpression("*(&pbIoData)");
                //evaluateTicket1.WaitUntilDone();
                //var myvalue = evaluateTicket1.ResultRecord["value"];

                //write to address where pbIoData points to
                createVarTicket1 = gdbControl.SendCustomCommandToGdb("-data-write-memory -o 2 531 x 1 0x55");
                createVarTicket1.WaitUntilDone();

                assignVarlTicket1 = gdbControl.SendCustomCommandToGdb("-data-write-memory -o 3 531 x 1 0x66");
                assignVarlTicket1.WaitUntilDone();

                deleteBreakPointTicket = gdbControl.SendCustomCommandToGdb("-data-write-memory -o 4 531 x 1 0x77");
                deleteBreakPointTicket.WaitUntilDone();

                deleteBreakPointTicket = gdbControl.SendCustomCommandToGdb("-break-delete");
                deleteBreakPointTicket.WaitUntilDone();

                continueTicket = gdbControl.Continue();
                continueTicket.WaitUntilDone();

                AsyncExchange.WaitForAsyncExchange();


                sutConnector.Al.GetVersion(out version);
                sutConnector.Al.GetVersion(out version);

                // Check the evaluation.

                gdbControl.KillGdbProcess();
            }
        }

        /// <summary>
        ///...
        ///</summary>
        [TestMethod()]
        [Timeout(5000)]
        public void ATFormatAfterRollForwardAdded()
        {

            #region Setup
            var devEnvPath = new SubdirPath(@"J:\DESFireEV2\Software\04_Tools\GOP_HAL\12_Devenv");
            var simulatorPath = devEnvPath + new SubdirPath(@"t508\simulator");
            var gdbPath = devEnvPath + new SubdirPath(@"shared\gdb");
            var nxpRdLibPath = devEnvPath + new SubdirPath(@"shared\nxprdlib");
            var gdbFilePath = new FilePath(gdbPath, new Filename(@"gdb.exe"));

            var targetProgram =
                new FilePath(
                    new SubdirPath(
                        @"J:\DESFireEV2\Software\01_Development\DesFireEV2\DesFireEV2\P40C072\"),
                    new Filename("P40_DesFireEV2_C072"));
            var workingDir = new SubdirPath(@"J:\DESFireEV2\Software\01_Development\DesFireEV2\");

            var environment = new Dictionary<string, string>
                                  {
                                      {"Path", simulatorPath.ToString() + ";" + gdbPath.ToString() + ";" + nxpRdLibPath.ToString()},
                                      {"DEVENV", devEnvPath.ToString()}
                                  };

            #endregion

            const byte RSTEN1_ADDR = 0x21;

            using (var gdbControl = new GdbControl(targetProgram, workingDir, gdbFilePath) { StopGdbTimeoutMillis = 100000 })
            {
                CommandTicket breakpointTicket1;
                CommandTicket breakpointTicket2;
                CommandTicket breakpointTicket3;
                CommandTicket deleteBreakPointTicket;
                CommandTicket runTicket;
                CommandTicket printTicket;
                CommandTicket evaluateTicket1;
                CommandTicket evaluateTicket2;
                CommandTicket evaluateTicket3;
                CommandTicket createVarTicket1;
                CommandTicket assignVarlTicket1;
                CommandTicket continueTicket;
                CommandTicket stopTicket;

                gdbControl.ConsoleStreamOutput += (msg) => Debug.Write(msg);
                gdbControl.StderrOutput += (message) => Debug.Write(message);
                gdbControl.LogStreamOutput += (mess) => Debug.Write(mess);

                gdbControl.StartDebugging(environment);
                var sourceTicket = gdbControl.SendCustomCommandToGdb("source DesFireEV2/platform/.gdbinit_t508");
                sourceTicket.WaitUntilDone();

                //AT testing flow
                //set breakpoint to label TearingIDs_x:
                breakpointTicket1 = gdbControl.SendCustomCommandToGdb("-break-insert phCommandSet_Format.c:41");
                breakpointTicket1.WaitUntilDone();
                
                breakpointTicket2 = gdbControl.SendCustomCommandToGdb("-break-insert phCommandSet_Format.c:43");
                breakpointTicket2.WaitUntilDone();

                //run
                runTicket = gdbControl.Run();
                runTicket.WaitUntilDone();

                //connect sut and fullfill precondition
                var sutConnector = new AdapterBase(Properties.Resources.TestbenchConfig, Properties.Resources.CardConfig, true);

                byte[] uid;
                byte[] ats;
                byte sak;
                byte more;
                byte[] version;
                byte[] PICCMasterKeyInit = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                sutConnector.Keystore.FormatKeyEntry(0, NxpRdLibNet.KeyStore.KeyType.DES2K3).ThrowOnError();
                sutConnector.Keystore.SetKey(0, 0, NxpRdLibNet.KeyStore.KeyType.DES2K3, PICCMasterKeyInit, 0).ThrowOnError();
                
                sutConnector.Hal.FieldReset().ThrowOnError();
                sutConnector.palI14443p3a.ActivateCard(null, out uid, out sak, out more).ThrowOnError();
                sutConnector.palI14443p4a.ActivateCard(0x08, 0x01, 0x01, 0x01, out ats).ThrowOnError();
                sutConnector.palI14443p4.SetProtocol(0x01, 0x01, 0x00, 0x00, 0x04, 0x08, 0x05).ThrowOnError();
                //...

                //data exchange to meet breakpoint
                sutConnector.Al.Authenticate(NxpRdLibNet.ExchangeOptions.DEFAULT, 0, 0, 0x00, null).ThrowOnError();
                //breakpoint hit with next cmd expected
                NxpRdLibNet.Status_t status = new NxpRdLibNet.Status_t();
                AsyncExchange.BeginAsyncExchange(() => status = sutConnector.Al.FormatPICC());

                //retrieve current data
                var expectedRestoreArea = gdbControl.ReadMemoryWords(gpphMemoryLayout_RestoreData, GdbControl.WordSize.EightBits, phRestoreAreaLength).DedicatedObject;
                expectedRestoreArea.WaitUntilDataAvailable();
                byte[] abExpectedRestoreArea = expectedRestoreArea.Data.Select(item => (byte)item).ToArray();

                //continue
                continueTicket = gdbControl.Continue();
                continueTicket.WaitUntilDone();

                //retrieve current data
                var currentRestoreArea = gdbControl.ReadMemoryWords(gpphMemoryLayout_RestoreData, GdbControl.WordSize.EightBits, phRestoreAreaLength).DedicatedObject;
                currentRestoreArea.WaitUntilDataAvailable();
                byte[] abCurrentRestoreArea = currentRestoreArea.Data.Select(item => (byte)item).ToArray();

                //data manipulation and invalidation
                //write data back
                for (int i = 0; i < abCurrentRestoreArea.Length; i++)
                {
                    var cmd = "-data-write-memory " + "-o " + i.ToString() + " 0x" + gpphMemoryLayout_RestoreData.ToString("X8") + " x 1 " + "0x" + ((ushort)~abCurrentRestoreArea[i]).ToString("X2");
                    var tx = gdbControl.SendCustomCommandToGdb(cmd);
                    tx.WaitUntilDone();
                }

                //delete breakpoints
                deleteBreakPointTicket = gdbControl.SendCustomCommandToGdb("-break-delete");
                deleteBreakPointTicket.WaitUntilDone();

                //set breakpoint according tearing level
                breakpointTicket3 = gdbControl.SendCustomCommandToGdb("-break-insert phExchangeHandlers.c:52");
                //breakpointTicket3 = gdbControl.SendCustomCommandToGdb("-break-insert phExchangeHandlers.c:50");
                breakpointTicket3.WaitUntilDone();

                //rst
                //RSTEN1.val = RSTEN1_EN_LT12_VOLT_MASK
                var t1 = gdbControl.SendCustomCommandToGdb("-data-write-memory 0x21 x 1 0x04");
                t1.WaitUntilDone();
                //SENSORS_ENABLE.val = SENSORS_ENABLE_VOLTSENS_EN_MASK
                var t2 = gdbControl.SendCustomCommandToGdb("-data-write-memory 0x06 x 1 0x08");
                t2.WaitUntilDone();
                //SENSORS_TEST.val = SENSORS_TES_TST_VOLTSENS_EN_MASK
                var t3 = gdbControl.SendCustomCommandToGdb("-data-write-memory 0x07 x 1 0x08");
                t3.WaitUntilDone();

                //continue
                continueTicket = gdbControl.Continue();
                continueTicket.WaitUntilDone();

                AsyncExchange.WaitForAsyncExchange();
                Assert.AreEqual(status, new NxpRdLibNet.Status_t(NxpRdLibNet.Error_CompCode.HAL, NxpRdLibNet.Error_Comm.IO_TIMEOUT));

                sutConnector.palI14443p3a.ActivateCard(null, out uid, out sak, out more).ThrowOnError();
                sutConnector.palI14443p4a.ActivateCard(0x08, 0x01, 0x01, 0x01, out ats).ThrowOnError();
                sutConnector.palI14443p4.SetProtocol(0x01, 0x01, 0x00, 0x00, 0x04, 0x08, 0x05).ThrowOnError();

                //breakpoint according to tearing level reached
                AsyncExchange.BeginAsyncExchange(() => status = sutConnector.Al.GetVersion(out version));
                //original content must be restored
                //retrieve current data
                currentRestoreArea = gdbControl.ReadMemoryWords(gpphMemoryLayout_RestoreData, GdbControl.WordSize.EightBits, phRestoreAreaLength).DedicatedObject;
                currentRestoreArea.WaitUntilDataAvailable();
                abCurrentRestoreArea = currentRestoreArea.Data.Select(item => (byte)item).ToArray();
                //compare
                Assert.IsTrue(abCurrentRestoreArea.SequenceEqual(abExpectedRestoreArea),"restore failure");
                
                AsyncExchange.WaitForAsyncExchange();
                
                gdbControl.KillGdbProcess();

            }
        }

        /// <summary>
        ///...
        ///</summary>
        [TestMethod()]
        [Timeout(5000)]
        public void ATFormatAfterMemSetReferencesUpdated()
        {

            #region Setup
            var devEnvPath = new SubdirPath(@"J:\DESFireEV2\Software\04_Tools\GOP_HAL\12_Devenv");
            var simulatorPath = devEnvPath + new SubdirPath(@"t508\simulator");
            var gdbPath = devEnvPath + new SubdirPath(@"shared\gdb");
            var nxpRdLibPath = devEnvPath + new SubdirPath(@"shared\nxprdlib");
            var gdbFilePath = new FilePath(gdbPath, new Filename(@"gdb.exe"));

            var targetProgram =
                new FilePath(
                    new SubdirPath(
                        @"J:\DESFireEV2\Software\01_Development\DesFireEV2\DesFireEV2\P40C072\"),
                    new Filename("P40_DesFireEV2_C072"));
            var workingDir = new SubdirPath(@"J:\DESFireEV2\Software\01_Development\DesFireEV2\");

            var environment = new Dictionary<string, string>
                                  {
                                      {"Path", simulatorPath.ToString() + ";" + gdbPath.ToString() + ";" + nxpRdLibPath.ToString()},
                                      {"DEVENV", devEnvPath.ToString()}
                                  };

            #endregion

            const byte RSTEN1_ADDR = 0x21;

            using (var gdbControl = new GdbControl(targetProgram, workingDir, gdbFilePath) { StopGdbTimeoutMillis = 100000 })
            {
                CommandTicket breakpointTicket1;
                CommandTicket breakpointTicket2;
                CommandTicket breakpointTicket3;
                CommandTicket deleteBreakPointTicket;
                CommandTicket runTicket;
                CommandTicket printTicket;
                CommandTicket evaluateTicket1;
                CommandTicket evaluateTicket2;
                CommandTicket evaluateTicket3;
                CommandTicket createVarTicket1;
                CommandTicket assignVarlTicket1;
                CommandTicket continueTicket;
                CommandTicket stopTicket;

                gdbControl.ConsoleStreamOutput += (msg) => Debug.Write(msg);
                gdbControl.StderrOutput += (message) => Debug.Write(message);
                gdbControl.LogStreamOutput += (mess) => Debug.Write(mess);

                gdbControl.StartDebugging(environment);
                var sourceTicket = gdbControl.SendCustomCommandToGdb("source DesFireEV2/platform/.gdbinit_t508");
                sourceTicket.WaitUntilDone();

                //AT testing flow
                //set breakpoint to label TearingIDs_x:
                breakpointTicket1 = gdbControl.SendCustomCommandToGdb("-break-insert phCommandSet_Format.c:48");
                breakpointTicket1.WaitUntilDone();

                breakpointTicket2 = gdbControl.SendCustomCommandToGdb("-break-insert phCommandSet_Format.c:50");
                breakpointTicket2.WaitUntilDone();

                //run
                runTicket = gdbControl.Run();
                runTicket.WaitUntilDone();

                //connect sut and fullfill precondition
                var sutConnector = new AdapterBase(Properties.Resources.TestbenchConfig, Properties.Resources.CardConfig, true);

                byte[] uid;
                byte[] ats;
                byte sak;
                byte more;
                byte[] version;
                byte[] PICCMasterKeyInit = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                sutConnector.Keystore.FormatKeyEntry(0, NxpRdLibNet.KeyStore.KeyType.DES2K3).ThrowOnError();
                sutConnector.Keystore.SetKey(0, 0, NxpRdLibNet.KeyStore.KeyType.DES2K3, PICCMasterKeyInit, 0).ThrowOnError();

                sutConnector.Hal.FieldReset().ThrowOnError();
                sutConnector.palI14443p3a.ActivateCard(null, out uid, out sak, out more).ThrowOnError();
                sutConnector.palI14443p4a.ActivateCard(0x08, 0x01, 0x01, 0x01, out ats).ThrowOnError();
                sutConnector.palI14443p4.SetProtocol(0x01, 0x01, 0x00, 0x00, 0x04, 0x08, 0x05).ThrowOnError();
                //...

                //data exchange to meet breakpoint
                sutConnector.Al.Authenticate(NxpRdLibNet.ExchangeOptions.DEFAULT, 0, 0, 0x00, null).ThrowOnError();
                //breakpoint hit with next cmd expected
                NxpRdLibNet.Status_t status = new NxpRdLibNet.Status_t();
                AsyncExchange.BeginAsyncExchange(() => status = sutConnector.Al.FormatPICC());

                //retrieve current data
                var expectedMemRefArea = gdbControl.ReadMemoryWords(refANode, GdbControl.WordSize.EightBits, 8).DedicatedObject;
                expectedMemRefArea.WaitUntilDataAvailable();
                byte[] abExpectedRestoreArea = expectedMemRefArea.Data.Select(item => (byte)item).ToArray();

                //continue
                continueTicket = gdbControl.Continue();
                continueTicket.WaitUntilDone();

                //retrieve current data
                var currentMemRefArea = gdbControl.ReadMemoryWords(refANode, GdbControl.WordSize.EightBits, 8).DedicatedObject;
                currentMemRefArea.WaitUntilDataAvailable();
                byte[] abCurrentMemRefArea = currentMemRefArea.Data.Select(item => (byte)item).ToArray();

                //data manipulation and invalidation
                //write data back
                for (int i = 0; i < abCurrentMemRefArea.Length; i++)
                {
                    var cmd = "-data-write-memory " + "-o " + i.ToString() + " 0x" + refANode.ToString("X8") + " x 1 " + "0x" + ((ushort)~abCurrentMemRefArea[i]).ToString("X2");
                    var tx = gdbControl.SendCustomCommandToGdb(cmd);
                    tx.WaitUntilDone();
                }

                //delete breakpoints
                deleteBreakPointTicket = gdbControl.SendCustomCommandToGdb("-break-delete");
                deleteBreakPointTicket.WaitUntilDone();

                //set breakpoint according tearing level
                breakpointTicket3 = gdbControl.SendCustomCommandToGdb("-break-insert phExchangeHandlers.c:52");
                //breakpointTicket3 = gdbControl.SendCustomCommandToGdb("-break-insert phExchangeHandlers.c:50");
                breakpointTicket3.WaitUntilDone();

                //rst
                //RSTEN1.val = RSTEN1_EN_LT12_VOLT_MASK
                var t1 = gdbControl.SendCustomCommandToGdb("-data-write-memory 0x21 x 1 0x04");
                t1.WaitUntilDone();
                //SENSORS_ENABLE.val = SENSORS_ENABLE_VOLTSENS_EN_MASK
                var t2 = gdbControl.SendCustomCommandToGdb("-data-write-memory 0x06 x 1 0x08");
                t2.WaitUntilDone();
                //SENSORS_TEST.val = SENSORS_TES_TST_VOLTSENS_EN_MASK
                var t3 = gdbControl.SendCustomCommandToGdb("-data-write-memory 0x07 x 1 0x08");
                t3.WaitUntilDone();

                //continue
                continueTicket = gdbControl.Continue();
                continueTicket.WaitUntilDone();

                AsyncExchange.WaitForAsyncExchange();
                Assert.AreEqual(status, new NxpRdLibNet.Status_t(NxpRdLibNet.Error_CompCode.HAL, NxpRdLibNet.Error_Comm.IO_TIMEOUT));

                sutConnector.palI14443p3a.ActivateCard(null, out uid, out sak, out more).ThrowOnError();
                sutConnector.palI14443p4a.ActivateCard(0x08, 0x01, 0x01, 0x01, out ats).ThrowOnError();
                sutConnector.palI14443p4.SetProtocol(0x01, 0x01, 0x00, 0x00, 0x04, 0x08, 0x05).ThrowOnError();

                //breakpoint according to tearing level reached
                AsyncExchange.BeginAsyncExchange(() => status = sutConnector.Al.GetVersion(out version));
                //original content must be restored
                //retrieve current data
                currentMemRefArea = gdbControl.ReadMemoryWords(refANode, GdbControl.WordSize.EightBits, 8).DedicatedObject;
                currentMemRefArea.WaitUntilDataAvailable();
                abCurrentMemRefArea = currentMemRefArea.Data.Select(item => (byte)item).ToArray();
                //compare
                Assert.IsTrue(abCurrentMemRefArea.SequenceEqual(abExpectedRestoreArea), "restore failure");

                AsyncExchange.WaitForAsyncExchange();

                gdbControl.KillGdbProcess();

            }
        }

        /// <summary>
        ///...
        ///</summary>
        [TestMethod()]
        public void InvalidateMemProductInfoGetVersion()
        {

            #region Setup
            var devEnvPath = new SubdirPath(@"J:\DESFireEV2\Software\04_Tools\GOP_HAL\12_Devenv");
            var simulatorPath = devEnvPath + new SubdirPath(@"t508\simulator");
            var gdbPath = devEnvPath + new SubdirPath(@"shared\gdb");
            var nxpRdLibPath = devEnvPath + new SubdirPath(@"shared\nxprdlib");
            var gdbFilePath = new FilePath(gdbPath, new Filename(@"gdb.exe"));

            var targetProgram =
                new FilePath(
                    new SubdirPath(
                        @"J:\DESFireEV2\Software\01_Development\DesFireEV2\DesFireEV2\P40C072\"),
                    new Filename("P40_DesFireEV2_C072"));
            var workingDir = new SubdirPath(@"J:\DESFireEV2\Software\01_Development\DesFireEV2\");

            var environment = new Dictionary<string, string>
                                  {
                                      {"Path", simulatorPath.ToString() + ";" + gdbPath.ToString() + ";" + nxpRdLibPath.ToString()},
                                      {"DEVENV", devEnvPath.ToString()}
                                  };

            #endregion

            const ushort sytemInfoBlock1Address = 0xA160;
            const byte eepromDpwAddress = 0xf0;

            using (var gdbControl = new GdbControl(targetProgram, workingDir, gdbFilePath) { StopGdbTimeoutMillis = 100000 })
            {
                CommandTicket breakpointTicket1;
                CommandTicket breakpointTicket2;
                CommandTicket deleteBreakPointTicket;
                CommandTicket runTicket;
                CommandTicket printTicket;
                CommandTicket evaluateTicket1;
                CommandTicket evaluateTicket2;
                CommandTicket evaluateTicket3;
                CommandTicket createVarTicket1;
                CommandTicket assignVarlTicket1;
                CommandTicket continueTicket;
                CommandTicket stopTicket;

                gdbControl.ConsoleStreamOutput += (msg) => Debug.Write(msg);
                gdbControl.StderrOutput += (message) => Debug.Write(message);
                gdbControl.LogStreamOutput += (mess) => Debug.Write(mess);

                gdbControl.StartDebugging(environment);
                var sourceTicket = gdbControl.SendCustomCommandToGdb("source DesFireEV2/platform/.gdbinit_t508");
                sourceTicket.WaitUntilDone();

                //set breakpoint
                breakpointTicket1 = gdbControl.SendCustomCommandToGdb("-break-insert phConfiguration.c:84");
                breakpointTicket1.WaitUntilDone();
                
                //run
                runTicket = gdbControl.Run();
                runTicket.WaitUntilDone();

                //connect sut and fullfill precondition
                var sutConnector = new AdapterBase(Properties.Resources.TestbenchConfig, Properties.Resources.CardConfig, true);

                byte[] uid;
                byte[] ats;
                byte sak;
                byte more;
                byte[] version;

                sutConnector.Hal.FieldReset().ThrowOnError();
                sutConnector.palI14443p3a.ActivateCard(null, out uid, out sak, out more).ThrowOnError();
                sutConnector.palI14443p4a.ActivateCard(0x08, 0x01, 0x01, 0x01, out ats).ThrowOnError();
                sutConnector.palI14443p4.SetProtocol(0x01, 0x01, 0x00, 0x00, 0x04, 0x08, 0x05).ThrowOnError();
                
                //data exchange to meet breakpoint
                //breakpoint hit with next cmd expected
                NxpRdLibNet.Status_t status = new NxpRdLibNet.Status_t();
                AsyncExchange.BeginAsyncExchange(() => status = sutConnector.Al.GetVersion(out version));

                //data manipulation and invalidation
                var productInfo = gdbControl.ReadMemoryWords(sytemInfoBlock1Address, GdbControl.WordSize.EightBits, 12).DedicatedObject;
                productInfo.WaitUntilDataAvailable();
                byte[] abProductInfo = productInfo.Data.Select(item => (byte)item).ToArray();
                
                //bitwise invert data
                abProductInfo = abProductInfo.Select(item => (byte)(~item)).ToArray();
                
                //change dpw
                var dpw = gdbControl.ReadMemoryWords(eepromDpwAddress, GdbControl.WordSize.SixteenBits, 1).DedicatedObject;
                dpw.WaitUntilDataAvailable();
                ushort originalDpw = (ushort)dpw.Data[0];
                var t1 = gdbControl.SendCustomCommandToGdb("-data-write-memory " + "0x" + eepromDpwAddress.ToString("X8") + " x 2 " + "0x" + ((ushort)~originalDpw).ToString("X4"));
                t1.WaitUntilDone();
                
                //write data back
                for (int i=0;i<abProductInfo.Length;i++)
                {
                    var cmd = "-data-write-memory " + "-o " + i.ToString() + " 0x" + sytemInfoBlock1Address.ToString("X8") + " x 1 " + "0x" + abProductInfo[i].ToString("X2");
                    var tx = gdbControl.SendCustomCommandToGdb(cmd);
                    tx.WaitUntilDone();
                }
                
                //change dpw back to original
                t1 = gdbControl.SendCustomCommandToGdb("-data-write-memory " + "0x" + eepromDpwAddress.ToString("X8") + " x 2 " + "0x" + ((ushort)originalDpw).ToString("X4"));
                t1.WaitUntilDone();
                
                //delete breakpoints
                deleteBreakPointTicket = gdbControl.SendCustomCommandToGdb("-break-delete");
                deleteBreakPointTicket.WaitUntilDone();
                
                //continue
                continueTicket = gdbControl.Continue();
                continueTicket.WaitUntilDone();

                AsyncExchange.WaitForAsyncExchange();
                Assert.AreEqual(status, new NxpRdLibNet.Status_t(NxpRdLibNet.Error_CompCode.HAL, NxpRdLibNet.Error_Comm.IO_TIMEOUT));

                gdbControl.KillGdbProcess();

            }
        }

        /// <summary>
        /// Parses a dictionary object.
        /// </summary>
        public static class DictionaryParser
        {
            public static TKey ParseKey<TKey, TValue>(IDictionary<TKey, TValue> repository, TValue byValue)
            {
                return
                    (TKey)repository.Where(keyPair => keyPair.Value.Equals(byValue)).Select(
                          keyPair => keyPair.Key).FirstOrDefault();
            }

            public static TValue ParseValue<TKey, TValue>(IDictionary<TKey, TValue> repository, TKey byKey)
            {
                TValue result;
                return repository.TryGetValue(byKey, out result) ? result : result;
            }
        }
    }
}
