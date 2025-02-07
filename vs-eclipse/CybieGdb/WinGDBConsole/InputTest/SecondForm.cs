using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using Consonya.Tools.IO;
using GdbApi;

namespace AccessGdb
{
    public partial class SecondForm : Form
    {
        private static readonly FilePath GdbPath = new FilePath(@"c:\MinMe\bin\gdb.exe");
        private static readonly SubdirPath WorkingDirectory =
            new SubdirPath(@"c:\aeonWorkingDirectory\bph\cybie\vs\CybieGdb\conf\");
        private static readonly StringDictionary Environment = new StringDictionary
                                                                   {
                                                                       {"Path", "F:/PROJECT_MYNE/interfaces/Vault/Software/04_Tools/GOP_HAL/12_Devenv/shared/nxprdlib"},
                                                                       {"DEVENV", "F:/PROJECT_MYNE/interfaces/Vault/Software/04_Tools/GOP_HAL/12_Devenv"}
                                                                   };
        public GdbControl GdbControl { get; set; }
        public bool DebuggingStarted;
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondForm"/> class.
        /// </summary>
        public SecondForm()
        {
            InitializeComponent();
            GdbControl = new GdbControl(null, WorkingDirectory, GdbPath);
            GdbControl.ConsoleStreamOutput += message => Invoke(new Action(() => outputTextBox.AppendText(message)));
            GdbControl.ResultRecord += log => Invoke(new Action(() => logOutputTextBox.AppendText(log + System.Environment.NewLine)));
            GdbControl.AsyncNotification += notify => Invoke(new Action(() => asyncNotifyTextBox.AppendText(notify + System.Environment.NewLine)));
            GdbControl.AsyncExecOutput += execute => Invoke(new Action(() => asyncExecTextBox.AppendText(execute + System.Environment.NewLine)));

            #region Tooltips
            //var tooltip = new ToolTip();
            //tooltip.SetToolTip(startDebuggingButton, "Click button to start active GDB with ISS connection.");
            //tooltip.SetToolTip(stopDebuggingButton, "Click button to stop the active GDB with ISS connection.");
            //tooltip.SetToolTip(customCommandButton, "Send a command from the text box.");
            //tooltip.SetToolTip(setBreakpointButton, "Set a breakpoint in the programmed being debugged.");
            //tooltip.SetToolTip(addSourceButton, "Add the sources based on the configuration script, particularly .gdbinit.");
            //tooltip.SetToolTip(addPathButton, "Add paths where the sources can be found.");
            ////tooltip.SetToolTip(inputTextBox, "Input a command to send to GDB.");
            ////tooltip.SetToolTip(outputTextBox, "The output from the GDB console.");
            //tooltip.SetToolTip(clearButton, "Clears the output text box in the form.");
            #endregion

            AcceptButton = customCommandButton;
            loadParameterListButton.Enabled = false;
        }
        private void LoadParameterListButtonClick(object sender, EventArgs e)
        {
            var parameters = LoadParameters();
            SendCommands(parameters);
        }
        private List<string> LoadParameters()
        {
            var tempParams = new List<string>();
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TextReader reader = new StreamReader(openFileDialog.FileName);
                
                try
                {
                    string str;
                    while ((str = reader.ReadLine()) != null)
                    {
                        tempParams.Add(str);
                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("Failed reading the file.", "File error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (true)
                        reader.Close();
                }
                
            }
            return tempParams;
        }

        public void SendCommands(List<string> commands)
        {
            if (DebuggingStarted)
            {
                foreach (var element in commands)
                {
                    GdbControl.SendCustomCommand(element);
                }
            }
            else if (DebuggingStarted == false)
            {
                GdbControl.StartDebugging(Environment);
                DebuggingStarted = true;
                startDebuggingButton.Enabled = false;
            }
        }
        /// <summary>
        /// Captures the event of the form closing to dispose the GDB connection.
        /// </summary>
        private void SecondFormFormClosing(object sender, FormClosingEventArgs e)
        {
            GdbControl.Dispose();
        }
        /// <summary>
        /// Set a breakpoint in the programmed being debugged.
        /// </summary>
        private void SetBreakpointButtonClick(object sender, EventArgs e)
        {
            GdbControl.SetBreakpoint(inputTextBox.Text);
        }
        /// <summary>
        /// Click button to stop the active GDB with ISS connection.
        /// </summary>
        private void StopDebuggingButtonClick(object sender, EventArgs e)
        {
            GdbControl.StopDebugging();
            DebuggingStarted = false;
            startDebuggingButton.Enabled = true;
        }
        /// <summary>
        /// Click button to start active GDB with ISS connection.
        /// </summary>
        private void StartDebuggingButtonClick(object sender, EventArgs e)
        {
            GdbControl.StartDebugging(Environment);
            DebuggingStarted = true;
            startDebuggingButton.Enabled = false;
            loadParameterListButton.Enabled = true;
        }
        /// <summary>
        /// Send a command from the text box.
        /// </summary>
        private void CustomCommandButtonClick(object sender, EventArgs e)
        {
            if (DebuggingStarted)
            {
                GdbControl.SendCustomCommand(inputTextBox.Text);
                inputTextBox.Text = "";
            }
            else if (DebuggingStarted == false)
            {
                GdbControl.StartDebugging(Environment);
                startDebuggingButton.Enabled = false;
                DebuggingStarted = true;
            }
        }
        /// <summary>
        /// Add the sources based on the configuration script, particularly .gdbinit.
        /// </summary>
        private void AddSourceButtonClick(object sender, EventArgs e)
        {
            GdbControl.SendCustomCommand("source .gdbinit");
        }
        /// <summary>
        /// Add paths where the sources can be found.
        /// </summary>
        private void AddPathButtonClick(object sender, EventArgs e)
        {
            GdbControl.SendCustomCommand("path " + inputTextBox.Text);
        }
        /// <summary>
        /// Clears the output text box in the form.
        /// </summary>
        private void ClearButtonClick(object sender, EventArgs e)
        {
            outputTextBox.Text = "";
            logOutputTextBox.Text = "";
            asyncNotifyTextBox.Text = "";
            asyncExecTextBox.Text = "";
            Refresh();
        }

    }
}
