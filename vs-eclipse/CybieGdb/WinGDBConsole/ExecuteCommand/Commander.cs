using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ExecuteCommands
{
    public class Commander
    {
        #region ExecuteCommand Sync and Async
        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        public void ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
                //System.Diagnostics.ProcessStartInfo procStartInfo2 = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output. 
                //This means that it will be redirected to the Process.StandardOutput StreamReader.
                //procStartInfo.RedirectStandardOutput = true;
               // procStartInfo2.RedirectStandardOutput = true;
                //procStartInfo.UseShellExecute = false;
                //procStartInfo2.UseShellExecute = false;
                // Do not create the black window.
                //procStartInfo.CreateNoWindow = true;
               // procStartInfo2.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                //var proc = new System.Diagnostics.Process {StartInfo = procStartInfo2};
                StreamWriter sw;
                StreamReader sr;
                StreamReader err;
                var path = "f:\\PROJECT_MYNE\\interfaces\\Vault\\Software\\01_Development\\DF_EV2\\start_gdb_mi.cmd";
                if (File.Exists(path))
                {
                    var proc = new Process
                                   {
                                       StartInfo =
                                           {
                                               FileName = path,
                                               RedirectStandardError = true,
                                               RedirectStandardInput = true,
                                               RedirectStandardOutput = true,
                                               UseShellExecute = false,
                                               CreateNoWindow = false,
                                               Arguments = command.ToString()
                                           }
                                           
                                   };
                    proc.Start();
                    // Enter text
                    Thread.Sleep(2000);

                    sw = proc.StandardInput;
                    sr = proc.StandardOutput;
                    sw.AutoFlush = true;
                    sw.WriteLine(command.ToString());
                    //var result = sr.ReadToEnd();
                   // result += err.ReadToEnd();
                    
                    proc.StandardInput.WriteLine(command.ToString());
                    //proc.StandardInput.Flush();
                    
                    //proc.StandardInput.Close();

                    string[] args = Environment.GetCommandLineArgs();

                    foreach (string arg in args)
                    {
                        // do stuff
                    }
                    
                    //Console.WriteLine(proc.StandardOutput.ReadToEnd());
                    //proc.WaitForExit();
                    // Get the output into a string
                    //string result = proc.StandardOutput.ReadToEnd();

                    // Display the command output.
                    // Console.WriteLine(result);
                }
                //proc.Start();

                // Get the output into a string
                //string result = proc.StandardOutput.ReadToEnd();

                // Display the command output.
               // Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }

        public string ExecuteCmd(string command)
        {
            Process p = new Process();
            StreamWriter sw;
            StreamReader sr;
            StreamReader err;
            ProcessStartInfo psI = new ProcessStartInfo("cmd.exe");
            psI.Arguments = @"/C ping www.google.com";
            psI.UseShellExecute = false;
            psI.RedirectStandardInput = true;
            psI.RedirectStandardOutput = true;
            psI.RedirectStandardError = true;
            psI.CreateNoWindow = true;
            p.StartInfo = psI;
            p.Start();
            sw = p.StandardInput;
            sr = p.StandardOutput;
            sw.AutoFlush = true;
            if (command != "")
                sw.WriteLine(command);
            else
                //execute default command
                sw.WriteLine("dir \\");
            sw.Close();
            Console.ReadLine();
            var output = sr.ReadToEnd();
            return output;
        }

        /// <summary>
        /// Execute the command Asynchronously.
        /// </summary>
        /// <param name="command">string command.</param>
        public void ExecuteCommandAsync(string command)
        {
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(command);
            }
            catch (ThreadStartException objException)
            {
                // Log the exception
            }
            catch (ThreadAbortException objException)
            {
                // Log the exception
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }
        #endregion
    }
}
