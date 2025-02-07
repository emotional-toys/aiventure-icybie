using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Consonya.Tools.IO;
using ExecuteCommands;

namespace AccessGdb
{
    /// <summary>
    /// Connects a .NET instance to gdb.
    /// </summary>
    public partial class MainForm : Form
    {
        public static Process Proc { get; set; }
        public bool First { get; set; }

        public MainForm()
        {
            InitializeComponent();
            outputTextBox.ScrollBars = ScrollBars.Vertical;
            
        }

        private void SendCommandButtonClick(object sender, EventArgs e)
        {
            if (useDefaultCmd.Checked)
            {
                // Send the command to the cmd window.
                outputTextBox.Text = ExecuteCmd(sendCommandTextBox.Text);
            }
            else if (!useDefaultCmd.Checked)
            {
                // Prepare the command.
                var command = sendCommandTextBox.Text;
                // Format and send the command to gdb.
                outputTextBox.Text = ExecuteCommander(command);
            }
        }
        /// <summary>
        /// Executes the command to gdb.
        /// </summary>
        /// <param name="command">The command.</param>
        public string ExecuteCommander(string command)
        {
            const string gdbDevPath = "F:/PROJECT_MYNE/interfaces/Vault/Software/04_Tools/98_Scripts/setup_devenv.cmd";
            const string gdbPath = @"F:\PROJECT_MYNE\interfaces\Vault\Software\04_Tools\GOP_HAL\12_Devenv\shared\gdb\gdb.exe";
            const string gdbSimPath = @"f:\PROJECT_MYNE\interfaces\Vault\Software\01_Development\DF_EV2\start_gdb_mi.cmd";
            const string gdbPrimPath = @"c:\Users\nxp51553\myneProjectDirectory\gdb\gdb.exe";

            if (Proc == null)
            {

                Proc = new Process
                           {
                               StartInfo =
                                   {
                                       FileName = gdbPrimPath,
                                       RedirectStandardError = true,
                                       RedirectStandardInput = true,
                                       RedirectStandardOutput = true,
                                       UseShellExecute = false,
                                       CreateNoWindow = false,
                                       //Arguments = @"cd ./conf --command=./.gdbinit --interpreter mi"
                                       //Arguments = "cd ./conf --interpreter mi"
                                       Arguments = @"--interpreter mi"

                                   }
                                   
                           };
                
                Proc.StartInfo.EnvironmentVariables["Path"] =
                    @"F:\PROJECT_MYNE\interfaces\Vault\Software\04_Tools\98_Scripts\..\GOP_HAL\12_Devenv\shared\buildtools\;F:\PROJECT_MYNE\interfaces\Vault\Software\04_Tools\98_Scripts\..\GOP_HAL\12_Devenv\shared\gdb\;F:\PROJECT_MYNE\interfaces\Vault\Software\04_Tools\98_Scripts\..\GOP_HAL\12_Devenv\simulator\;F:\PROJECT_MYNE\interfaces\Vault\Software\04_Tools\98_Scripts\..\GOP_HAL\12_Devenv\shared\scripts\;F:\PROJECT_MYNE\interfaces\Vault\Software\04_Tools\98_Scripts\..\GOP_HAL\12_Devenv\shared\simserver\;F:\PROJECT_MYNE\interfaces\Vault\Software\04_Tools\98_Scripts\..\GOP_HAL\12_Devenv\shared\nxprdlib\;C:\Program Files (x86)\Target Compiler Technologies\p40-11R1\bin\WINbin;C:\Perl64\site\bin;C:\Perl64\bin;C:\Windows\system32;C:\Windows;C:\Windows\System32\Wbem;C:\Windows\System32\WindowsPowerShell\v1.0\;C:\Program Files\Common Files\Microsoft Shared\Microsoft Online Services;C:\ProgramFiles (x86)\ENOVIA\Synchronicity\DesignSyncV6R2011x\bin;C:\Program Files\TortoiseSVN\bin;C:\Program Files (x86)\Enterprise Vault\EVClient\;C:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\;C:\Program Files\Microsoft SQL Server\100\Tools\Binn\;C:\Program Files\Microsoft SQL Server\100\DTS\Binn\;C:\Program Files (x86)\Enterprise Vault\EVClient\;C:\Program Files (x86)\Mono-2.10.8\bin;C:\Program Files\Microsoft Network Monitor 3\;C:\Program Files\Network Monitor Experts\TCP Analyzer 1.2\";
                Proc.Start();
                
                First = false;
           }
            var processId = Proc.Id;
            
            Proc.StandardInput.WriteLine(command);

            // Must quit the input process to connect to the one behind it.
            //if (First == false)
            Proc.StandardInput.WriteLine("q");

            var response = Proc.StandardOutput.ReadToEnd();
            Proc.WaitForExit();
            First = true;
            return response;
        }
        /// <summary>
        /// Executes the cmd for *dos* window.
        /// </summary>
        /// <param name="command">The command.</param>
        public string ExecuteCmd(string command)
        {
            var p = new Process();
            var psI = new ProcessStartInfo("cmd.exe")
                          {
                              Arguments = @"/C " + command,
                              UseShellExecute = false,
                              RedirectStandardInput = true,
                              RedirectStandardOutput = true,
                              RedirectStandardError = true,
                              CreateNoWindow = true
                          };

            p.StartInfo = psI;
            p.Start();
            StreamWriter sw = p.StandardInput;
            StreamReader sr = p.StandardOutput;
            sw.AutoFlush = true;
            sw.WriteLine(command != "" ? command : "dir \\");
            sw.Close();
            sendCommandButton.Enabled = true;
            Console.ReadLine();
            var output = sr.ReadToEnd();
            
            return output;
        }

        private void ClearOutputButtonClick(object sender, EventArgs e)
        {
            outputTextBox.Text = "";
            Refresh();
        }
        public void Launch()
        {
            var path = new FilePath(@"f:\PROJECT_MYNE\interfaces\Vault\Software\04_Tools\GOP_HAL\12_Devenv\shared\gdb\gdb.exe");
            var control = new GdbControl(null, null, path);
            control.StartDebugging();
        }

        private void KlausButtonClick(object sender, EventArgs e)
        {
            Launch();
            var hold = "";
        }

        #region commented

        ///// <summary>
        ///// Executes a shell command synchronously.
        ///// </summary>
        ///// <param name="command">string command</param>
        ///// <returns>string, as output of the command.</returns>
        //public void ExecuteCommandSync(object command)
        //{
        //    try
        //    {
        //        // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.
        //        // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
        //        //System.Diagnostics.ProcessStartInfo procStartInfo2 = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

        //        // The following commands are needed to redirect the standard output. 
        //        //This means that it will be redirected to the Process.StandardOutput StreamReader.
        //        StreamWriter sw;
        //        StreamReader sr;
        //        StreamReader err;
        //        var path = "f:\\PROJECT_MYNE\\interfaces\\Vault\\Software\\01_Development\\DF_EV2\\start_gdb_mi.cmd";
        //        if (File.Exists(path))
        //        {
        //            var proc = new Process
        //            {
        //                StartInfo =
        //                {
        //                    FileName = path,
        //                    RedirectStandardError = true,
        //                    RedirectStandardInput = true,
        //                    RedirectStandardOutput = true,
        //                    UseShellExecute = false,
        //                    CreateNoWindow = false,
        //                    Arguments = command.ToString()
        //                }

        //            };
        //            proc.Start();
        //            // Enter text
        //            Thread.Sleep(2000);

        //            sw = proc.StandardInput;
        //            sr = proc.StandardOutput;
        //            sw.AutoFlush = true;
        //            sw.WriteLine(command.ToString());
        //            //var result = sr.ReadToEnd();
        //            // result += err.ReadToEnd();

        //            proc.StandardInput.WriteLine(command.ToString());
        //            //proc.StandardInput.Flush();

        //            //proc.StandardInput.Close();

        //            string[] args = Environment.GetCommandLineArgs();

        //            foreach (string arg in args)
        //            {
        //                // do stuff
        //            }

        //            //Console.WriteLine(proc.StandardOutput.ReadToEnd());
        //            //proc.WaitForExit();
        //            // Get the output into a string
        //            //string result = proc.StandardOutput.ReadToEnd();

        //            // Display the command output.
        //            // Console.WriteLine(result);
        //        }
        //        //proc.Start();

        //        // Get the output into a string
        //        //string result = proc.StandardOutput.ReadToEnd();

        //        // Display the command output.
        //        // Console.WriteLine(result);
        //    }
        //    catch (Exception objException)
        //    {
        //        // Log the exception
        //    }
        //}

        //private void SendInputButtonClick(object sender, EventArgs e)
        //{
        //    StreamReader sr;
        //    var tr = new StreamReader(outputTextBox.Text);
        //    string input;

        //    while ((input = tr.ReadLine()) != null)
        //    {
        //        outputTextBox.Text = (input);

        //    }

        //    tr.Close();
        //    // attach to process and send command
        //}

        #endregion
    }
}
