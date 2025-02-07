using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Jaspreets_Parallel_Port_Control
{
    public partial class ParallelPort : UserControl
    {
        #region "Imported Functions"

        [DllImport("ParallelPort.dll")]
        private static extern void Out32(short PortAddress, short data);

        [DllImport("ParallelPort.dll")]
        private static extern short Inp32(short PortAddress);

        [DllImport("ParallelPort.dll")]
        private static extern Int16 ReadMemShort(short Address);

        #endregion

        bool _IsBusy = false;
        string ExceptionOccured;

        private int _PortAddress = 0;

        /// <summary>
        /// Constructor.
        /// It detects the parallel port presence on the computer.
        /// If detected than _PortAddress is set to the address of LPT1.
        /// If not detected then the whole user control gets disabled.
        /// </summary>
        public ParallelPort()
        {
            InitializeComponent();

            int[] PortAddresses = Detect_LPT_Ports();
            if (PortAddresses.Length != 0)
            {
                _PortAddress = PortAddresses[0]; 
            }
            else
            {
                _PortAddress = 0;
                MessageBox.Show("No Parallel Port Detected On This Computer.");
                ParallelPortTabControl.Enabled = false;
            }
            
            try
            {
                UpdateAllDataBusPictureBox();
                UpdateAllStatusBusPictureBox();
                UpdateAllControlBusPictureBox();

                #region "Change shape of All Picture Boxes to ellipse"
                GraphicsPath myPath = new GraphicsPath();
                myPath.AddEllipse(0, 0, PD7.Width, PD7.Height);
                PD7.Region = new Region(myPath);
                PD6.Region = new Region(myPath);
                PD5.Region = new Region(myPath);
                PD4.Region = new Region(myPath);
                PD3.Region = new Region(myPath);
                PD2.Region = new Region(myPath);
                PD1.Region = new Region(myPath);
                PD0.Region = new Region(myPath);
                PC3.Region = new Region(myPath);
                PC2.Region = new Region(myPath);
                PC1.Region = new Region(myPath);
                PC0.Region = new Region(myPath);
                PS7.Region = new Region(myPath);
                PS6.Region = new Region(myPath);
                PS5.Region = new Region(myPath);
                PS4.Region = new Region(myPath);
                PS3.Region = new Region(myPath);
                LPD7.Region = new Region(myPath);
                LPD6.Region = new Region(myPath);
                LPD5.Region = new Region(myPath);
                LPD4.Region = new Region(myPath);
                LPD3.Region = new Region(myPath);
                LPD2.Region = new Region(myPath);
                LPD1.Region = new Region(myPath);
                LPD0.Region = new Region(myPath);
                LPC3.Region = new Region(myPath);
                LPC2.Region = new Region(myPath);
                LPC1.Region = new Region(myPath);
                LPC0.Region = new Region(myPath);
                CPD7.Region = new Region(myPath);
                CPD6.Region = new Region(myPath);
                CPD5.Region = new Region(myPath);
                CPD4.Region = new Region(myPath);
                CPD3.Region = new Region(myPath);
                CPD2.Region = new Region(myPath);
                CPD1.Region = new Region(myPath);
                CPD0.Region = new Region(myPath);
                CPC3.Region = new Region(myPath);
                CPC2.Region = new Region(myPath);
                CPC1.Region = new Region(myPath);
                CPC0.Region = new Region(myPath);

                #endregion

                StartLoopFromComboBox.SelectedIndex = 0;
                EndLoopAtComboBox.SelectedIndex = EndLoopAtComboBox.Items.Count - 1;
                CounterBitsCombBox.SelectedIndex = 0;
                CounterTypeComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ParallelPort(). ERROR occured is ---> " + ex.Message;
            }
        }

        private void ParallelPortTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            try
            {
                if (e.TabPage.Text.Equals("Loops"))
                {
                    StopLoopButton.Enabled = false;
                }
                else
                {
                    if (IsLooping)
                    {
                        MessageBox.Show("Stop Loop First", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                    }
                }

                if (e.TabPage.Text.Equals("Counters"))
                {
                    StopCounterButton.Enabled = false;
                }
                else
                {
                    if (IsCounting)
                    {
                        MessageBox.Show("Stop Counter First", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ParallelPortTabControl_Selecting(object sender, TabControlCancelEventArgs e). ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Detecs the presence of LPT ports.
        /// </summary>
        /// <returns>Returns an integer array of all detected LPT addresses.</returns>
        private int[] Detect_LPT_Ports()
        {
            int Number_Of_LPT_Ports = 0;
            short[] portAddresses = new short[3];
            for (int i = 0; i < 3; i++)
            {
                portAddresses[i] = ReadMemShort(Convert.ToInt16(1032 + i * 2));
                if (portAddresses[i] != 0)
                {
                    Number_Of_LPT_Ports++;
                }
            }
            int[] PortAddresses = new int[Number_Of_LPT_Ports];
            for (int i = 0, j = 0; i < 3; i++)
            {
                if (portAddresses[i] != 0)
                {
                    PortAddresses[j] = Convert.ToInt32(portAddresses[i]);
                    j++;
                }
            }
            return PortAddresses;
        }

        #region "Direct Methods To Access Parallel Port"
        
        /// <summary>
        /// Gets or Sets the address of the Parallel Port.
        /// </summary>
        public short PortAddress
        {
            get
            {
                return Convert.ToInt16(_PortAddress);
            }
            set
            {
                try
                {
                    _PortAddress = value;
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "PortAddress assigned ---> " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Returns true if Parallel Port is busy implementing LOOP or COUNTER. 
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _IsBusy;
            }           
        }

        /// <summary>
        /// Resets all Data and Control Pins. All Data & Control Pins become LOW.
        /// Stops all running LOOPs and COUNTERs.
        /// </summary>
        public void ResetAll()
        {
            try
            {
                EndLoop();
                EndCounter();
                ResetControlBus();
                ResetDataBus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ResetAll() called. ERROR occured is ---> " + ex.Message;
            }
        }

        #region "Data Bus"

        /// <summary>
        /// Gets the address of the Parallel Port's Data Bus.
        /// </summary>
        private short DataBusAddress
        {
            get
            {
                return Convert.ToInt16(_PortAddress);
            }
        }

        /// <summary>
        /// Read current status of the Data Bus. 
        /// </summary>
        /// <returns>
        /// Current status of all the Data Pins as an integer 
        /// whose bits give the current status of each Data Pin (D0 to D7) 
        /// with D0 being the LSB and D7 being the MSB.
        /// </returns>
        public int ReadFromDataBus()
        {
            return Convert.ToInt32(Inp32(DataBusAddress));
        }

        /// <summary>
        /// Writes data to the Data Bus.   
        /// </summary>
        /// <param name="Data">Data to be written</param>
        public void WriteToDataBus(int Data)
        {
            try
            {
                Out32(DataBusAddress, Convert.ToInt16(Data));
                UpdateAllDataBusPictureBox();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "WriteToDataBus(int Data) called with Data = " + Data.ToString() + ". ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Make data at all the Data Pins HIGH.
        /// </summary>
        public void SetDataBus()
        {
            try
            {
                D0 = true;
                D1 = true;
                D2 = true;
                D3 = true;
                D4 = true;
                D5 = true;
                D6 = true;
                D7 = true;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "SetDataBus() called. ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        ///  Make data at all the Data Pins LOW.
        /// </summary>
        public void ResetDataBus()
        {
            try
            {
                D0 = false;
                D1 = false;
                D2 = false;
                D3 = false;
                D4 = false;
                D5 = false;
                D6 = false;
                D7 = false;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ResetDataBus() called. ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin D0 (Pin no. 2).
        /// </summary>
        public bool D0
        {
            get
            {
                return Convert.ToBoolean(Inp32(DataBusAddress) & 1);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) | 1));
                    }
                    else
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) & (~1)));
                    }
                    UpdateDataBusPictureBox(0);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "D0 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin D1 (Pin no. 3).
        /// </summary>
        public bool D1
        {
            get
            {
                return Convert.ToBoolean(Inp32(DataBusAddress) & 2);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) | 2));
                    }
                    else
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) & (~2)));
                    }
                    UpdateDataBusPictureBox(1);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "D1 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin D2 (Pin no. 4).
        /// </summary>
        public bool D2
        {
            get
            {
                return Convert.ToBoolean(Inp32(DataBusAddress) & 4);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) | 4));
                    }
                    else
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) & (~4)));
                    }
                    UpdateDataBusPictureBox(2);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "D2 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin D3 (Pin no. 5).
        /// </summary>
        public bool D3
        {
            get
            {
                return Convert.ToBoolean(Inp32(DataBusAddress) & 8);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) | 8));
                    }
                    else
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) & (~8)));
                    }
                    UpdateDataBusPictureBox(3);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "D3 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin D4 (Pin no. 6).
        /// </summary>
        public bool D4
        {
            get
            {
                return Convert.ToBoolean(Inp32(DataBusAddress) & 16);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) | 16));
                    }
                    else
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) & (~16)));
                    }
                    UpdateDataBusPictureBox(4);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "D4 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin D5 (Pin no. 7).
        /// </summary>
        public bool D5
        {
            get
            {
                return Convert.ToBoolean(Inp32(DataBusAddress) & 32);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) | 32));
                    }
                    else
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) & (~32)));
                    }
                    UpdateDataBusPictureBox(5);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "D5 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin D6 (Pin no. 8).
        /// </summary>
        public bool D6
        {
            get
            {
                return Convert.ToBoolean(Inp32(DataBusAddress) & 64);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) | 64));
                    }
                    else
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) & (~64)));
                    }
                    UpdateDataBusPictureBox(6);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "D6 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin D7 (Pin no. 9).
        /// </summary>
        public bool D7
        {
            get
            {
                return Convert.ToBoolean(Inp32(DataBusAddress) & 128);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) | 128));
                    }
                    else
                    {
                        Out32(DataBusAddress, Convert.ToInt16(Inp32(DataBusAddress) & (~128)));
                    }
                    UpdateDataBusPictureBox(7);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "D7 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        #endregion

        #region "Status Bus"

        /// <summary>
        /// Gets the address of the Parallel Port's Status Bus.
        /// </summary>
        private short StatusBusAddress
        {
            get
            {
                return Convert.ToInt16(_PortAddress + 1);
            }
        }

        /// <summary>
        /// Checks the current status of the Status Pins.
        /// Also assigns value to S3,S4,S5,S6,S7 accordingly.
        /// </summary>
        public void UpdateStatusBus()
        {
            try
            {
                UpdateAllStatusBusPictureBox();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "UpdateStatusBus() called. ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Read current status of the Status Bus. 
        /// </summary>
        /// <returns>
        /// Current status of all the Status Pins(including the HIDDEN ones also) as
        /// an integer whose bits give the current status of each Status Pin (S0 to S7) 
        /// with S0 being the LSB and S7 being the MSB.
        /// </returns>
        public int ReadFromStatusBus()
        {
            return Convert.ToInt32(Inp32(StatusBusAddress));
        }

        /// <summary>
        /// Gets the data at Pin S3 (Pin no. 15).
        /// </summary>
        public bool S3
        {
            get
            {
                return Convert.ToBoolean(Inp32(StatusBusAddress) & 8);
            }
        }

        /// <summary>
        /// Gets the data at Pin S4 (Pin no. 13).
        /// </summary>
        public bool S4
        {
            get
            {
                return Convert.ToBoolean(Inp32(StatusBusAddress) & 16);
            }
        }

        /// <summary>
        /// Gets the data at Pin S5 (Pin no. 12).
        /// </summary>
        public bool S5
        {
            get
            {
                return Convert.ToBoolean(Inp32(StatusBusAddress) & 32);
            }
        }

        /// <summary>
        /// Gets the data at Pin S6 (Pin no. 10).
        /// </summary>
        public bool S6
        {
            get
            {
                return Convert.ToBoolean(Inp32(StatusBusAddress) & 64);
            }
        }

        /// <summary>
        /// Gets the data at Pin S7 (Pin no. 11).
        /// </summary>
        public bool S7
        {
            get
            {
                return !Convert.ToBoolean(Inp32(StatusBusAddress) & 128);
            }
        }

        #endregion

        #region "Control Bus"

        /// <summary>
        /// Gets the address of the Parallel Port's Control Bus.
        /// </summary>
        private short ControlBusAddress
        {
            get
            {
                return Convert.ToInt16(_PortAddress + 2);
            }
        }

        /// <summary>
        /// Reads the current status of the Control Bus.
        /// </summary>
        /// <returns>
        /// Current status of all the Control Pins (including the HIDDEN ones also) as an integer 
        /// whose bits give the current status of each Control Pin (C0 to C7) 
        /// with C0 being the LSB and C7 being the MSB.
        /// </returns>
        public int ReadFromControlBus()
        {
            return Convert.ToInt32(Inp32(ControlBusAddress));
        }

        /// <summary>
        /// Writes data to the Control Pins.   
        /// </summary>
        /// <param name="Data">Data to be written.</param>
        public void WriteToControlBus(int Data)
        {
            try
            {
                Out32(ControlBusAddress, Convert.ToInt16(Data));
                UpdateAllControlBusPictureBox();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "WriteToControlBus(int Data) called with Data = " + Data.ToString() + ". ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Make data at all the Control Pins HIGH.
        /// </summary>
        public void SetControlBus()
        {
            try
            {
                C0 = true;
                C1 = true;
                C2 = true;
                C3 = true;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "SetControlBus() called. ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Make data at all the Control Pins LOW.
        /// </summary>
        public void ResetControlBus()
        {
            try
            {
                C0 = false;
                C1 = false;
                C2 = false;
                C3 = false;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ResetControlBus() called. ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin C0 (Pin no. 1).
        /// </summary>
        public bool C0
        {
            get
            {
                return !Convert.ToBoolean(Inp32(ControlBusAddress) & 1);
            }
            set
            {
                try
                {
                    if (!value)
                    {
                        Out32(ControlBusAddress, Convert.ToInt16(Inp32(ControlBusAddress) | 1));
                    }
                    else
                    {
                        Out32(ControlBusAddress, Convert.ToInt16(Inp32(ControlBusAddress) & (~1)));
                    }
                    UpdateControlBusPictureBox(0);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "C0 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin C1 (Pin no. 14).
        /// </summary>
        public bool C1
        {
            get
            {
                return !Convert.ToBoolean(Inp32(ControlBusAddress) & 2);
            }
            set
            {
                try
                {
                    if (!value)
                    {
                        Out32(ControlBusAddress, Convert.ToInt16(Inp32(ControlBusAddress) | 2));
                    }
                    else
                    {
                        Out32(ControlBusAddress, Convert.ToInt16(Inp32(ControlBusAddress) & (~2)));
                    }
                    UpdateControlBusPictureBox(1);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "C1 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin C2 (Pin no. 16).
        /// </summary>
        public bool C2
        {
            get
            {
                return Convert.ToBoolean(Inp32(ControlBusAddress) & 4);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        Out32(ControlBusAddress, Convert.ToInt16(Inp32(ControlBusAddress) | 4));
                    }
                    else
                    {
                        Out32(ControlBusAddress, Convert.ToInt16(Inp32(ControlBusAddress) & (~4)));
                    }
                    UpdateControlBusPictureBox(2);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "C2 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the data at Pin C3 (Pin no. 17).
        /// </summary>
        public bool C3
        {
            get
            {
                return !Convert.ToBoolean(Inp32(ControlBusAddress) & 8);
            }
            set
            {
                try
                {
                    if (!value)
                    {
                        Out32(ControlBusAddress, Convert.ToInt16(Inp32(ControlBusAddress) | 8));
                    }
                    else
                    {
                        Out32(ControlBusAddress, Convert.ToInt16(Inp32(ControlBusAddress) & (~8)));
                    }
                    UpdateControlBusPictureBox(3);
                }
                catch (Exception ex)
                {
                    ExceptionOccured = "C3 assigned = " + value.ToString() + ". ERROR occured is ---> " + ex.Message;
                }
            }
        }

        #endregion

        #region "Loops"

        /// <summary>
        /// Loops from Starting Pin to Ending Pin.
        /// </summary>
        /// <param name="StartingPin">Starting Pin of Loop.</param>
        /// <param name="EndingPin">Ending Pin of Loop.</param>
        /// <param name="PulseDuration">Delay in milliseconds after each iteration.</param>
        /// <param name="ShouldOnlySinglePinBeHigh">True if data at current pin in loop should be HIGH and data at rest of the pins should be LOW,vice versa.</param>
        public void StartSynchronousLoop(Pin StartingPin, Pin EndingPin, int PulseDuration, bool ShouldOnlySinglePinBeHigh)
        {
            try
            {
                if (_IsBusy)
                {
                    ResetAll();
                }
                ParallelPortTabControl.SelectedIndex = 1;
                GenerateLoopSequence(Convert.ToInt16(StartingPin), Convert.ToInt16(EndingPin));
                EnableDisableLoopControls(false);
                IsLoopPositive = ShouldOnlySinglePinBeHigh;
                LoopTimerTicker = 0;
                LoopTimer.Interval = PulseDuration;
                LoopTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "StartSynchronousLoop(Pin StartingPin, Pin EndingPin, int PulseDuration,bool ShouldOnlySinglePinBeHigh) called with StartingPin = " + StartingPin.ToString() + " , EndingPin = " + EndingPin.ToString() + " , PulseDuration = " + PulseDuration.ToString() + " , ShouldOnlySinglePinBeHigh = " + ShouldOnlySinglePinBeHigh.ToString() + ". ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Loops through desired sequence of Pins.
        /// </summary>
        /// <param name="Pins">Collection of Pins to be iterated.</param>
        /// <param name="PulseDuration">Delay in milliseconds after each iteration.</param>
        /// <param name="ShouldOnlySinglePinBeHigh">True if data at current pin in loop should be HIGH and data at rest of the pins should be LOW,vice versa.</param>
        public void StartAsynchronousLoop(Pin[] Pins, int PulseDuration, bool ShouldOnlySinglePinBeHigh)
        {
            try
            {
                if (_IsBusy)
                {
                    ResetAll();
                }
                ParallelPortTabControl.SelectedIndex = 1;
                GeneratedLoopSequence = new string[Pins.Length];
                for (int i = 0; i <= Pins.GetUpperBound(0); i++)
                {
                    GeneratedLoopSequence[i] = Pins[i].ToString();
                }
                EnableDisableLoopControls(false);
                IsLoopPositive = ShouldOnlySinglePinBeHigh;
                LoopTimerTicker = 0;
                LoopTimer.Interval = PulseDuration;
                LoopTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "StartAsynchronousLoop(Pin[] Pins, int PulseDuration, bool ShouldOnlySinglePinBeHigh) called with PulseDuration = " + PulseDuration.ToString() + " , ShouldOnlySinglePinBeHigh = " + ShouldOnlySinglePinBeHigh.ToString() + ". ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Stops all running Loops.
        /// </summary>
        public void EndLoop()
        {
            try
            {
                LoopTimer.Enabled = false;
                EnableDisableLoopControls(true);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "EndAllLoops() called. ERROR occured is ---> " + ex.Message;
            }
        }

        #endregion

        #region "Counters"

        /// <summary>
        /// Counts desired sequence. 
        /// </summary>
        /// <param name="NoOfBits">Number of bits to be iterated.</param>
        /// <param name="__CounterType">Type of Counter to be implemented.</param>
        /// <param name="PulseDuration">Delay in milliseconds after each iteration.</param>
        public void StartCounter(CountBits NoOfBits, CounterType __CounterType, int PulseDuration)
        {
            try
            {
                if (_IsBusy)
                {
                    ResetAll();
                }
                ParallelPortTabControl.SelectedIndex = 2;
                MaximumCount = -(Convert.ToInt32(NoOfBits) - 12);
                if (__CounterType == CounterType.UPCounter)
                {
                    _CounterType = "UP Counter";
                    CountTimerTicker = 0;
                }
                else
                {
                    _CounterType = "DOWN Counter";
                    CountTimerTicker = Convert.ToInt32(Math.Pow(2, MaximumCount)) - 1;
                }
                EnableDisableCounterControls(true);
                CounterTimer.Interval = PulseDuration;
                CounterTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "StartCounter(CountBits NoOfBits, CounterType __CounterType, int PulseDuration) called with NoOfBits = " + NoOfBits.ToString() + " , __CounterType = " + __CounterType.ToString() + " , PulseDuration = " + PulseDuration.ToString() + ". ERROR occured is ---> " + ex.Message;
            }
        }

        /// <summary>
        /// Stops all running Counters.
        /// </summary>
        public void EndCounter()
        {
            try
            {
                CounterTimer.Enabled = false;
                EnableDisableCounterControls(false);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "EndCounter() called. ERROR occured is ---> " + ex.Message;
            }
        }

        #endregion

        #endregion

        #region "Methods which check current status of port and update all Picture Boxes"

        private void UpdateDataBusPictureBox(int PictureBoxNo)
        {
            Color OnColor = Color.Coral;
            Color OffColor = Color.Lavender;
            try
            {
                if (PictureBoxNo == 7)
                {
                    if (D7)
                    {
                        PD7.BackColor = OnColor;
                        LPD7.BackColor = OnColor;
                        CPD7.BackColor = OnColor;
                    }
                    else
                    {
                        PD7.BackColor = OffColor;
                        LPD7.BackColor = OffColor;
                        CPD7.BackColor = OffColor;
                    }
                }

                else if (PictureBoxNo == 6)
                {
                    if (D6)
                    {
                        PD6.BackColor = OnColor;
                        LPD6.BackColor = OnColor;
                        CPD6.BackColor = OnColor;
                    }
                    else
                    {
                        PD6.BackColor = OffColor;
                        LPD6.BackColor = OffColor;
                        CPD6.BackColor = OffColor;
                    }
                }
                else if (PictureBoxNo == 5)
                {
                    if (D5)
                    {
                        PD5.BackColor = OnColor;
                        LPD5.BackColor = OnColor;
                        CPD5.BackColor = OnColor;
                    }
                    else
                    {
                        PD5.BackColor = OffColor;
                        LPD5.BackColor = OffColor;
                        CPD5.BackColor = OffColor;
                    }
                }

                else if (PictureBoxNo == 4)
                {
                    if (D4)
                    {
                        PD4.BackColor = OnColor;
                        LPD4.BackColor = OnColor;
                        CPD4.BackColor = OnColor;
                    }
                    else
                    {
                        PD4.BackColor = OffColor;
                        LPD4.BackColor = OffColor;
                        CPD4.BackColor = OffColor;
                    }
                }
                else if (PictureBoxNo == 3)
                {
                    if (D3)
                    {
                        PD3.BackColor = OnColor;
                        LPD3.BackColor = OnColor;
                        CPD3.BackColor = OnColor;
                    }
                    else
                    {
                        PD3.BackColor = OffColor;
                        LPD3.BackColor = OffColor;
                        CPD3.BackColor = OffColor;
                    }
                }
                else if (PictureBoxNo == 2)
                {
                    if (D2)
                    {
                        PD2.BackColor = OnColor;
                        LPD2.BackColor = OnColor;
                        CPD2.BackColor = OnColor;
                    }
                    else
                    {
                        PD2.BackColor = OffColor;
                        LPD2.BackColor = OffColor;
                        CPD2.BackColor = OffColor;
                    }
                }
                else if (PictureBoxNo == 1)
                {
                    if (D1)
                    {
                        PD1.BackColor = OnColor;
                        LPD1.BackColor = OnColor;
                        CPD1.BackColor = OnColor;
                    }
                    else
                    {
                        PD1.BackColor = OffColor;
                        LPD1.BackColor = OffColor;
                        CPD1.BackColor = OffColor;
                    }
                }
                else
                {
                    if (D0)
                    {
                        PD0.BackColor = OnColor;
                        LPD0.BackColor = OnColor;
                        CPD0.BackColor = OnColor;
                    }
                    else
                    {
                        PD0.BackColor = OffColor;
                        LPD0.BackColor = OffColor;
                        CPD0.BackColor = OffColor;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "UpdateDataBusPictureBox(int PictureBoxNo) called with PictureBoxNo = " + PictureBoxNo.ToString() + ". ERROR occured is ---> " + ex.Message;
            }
        }

        private void UpdateStatusBusPictureBox(int PictureBoxNo)
        {
            Color OnColor = Color.Coral;
            Color OffColor = Color.Lavender;
            try
            {
                if (PictureBoxNo == 7)
                {
                    if (S7)
                    {
                        PS7.BackColor = OnColor;
                    }
                    else
                    {
                        PS7.BackColor = OffColor;
                    }
                }
                else if (PictureBoxNo == 6)
                {
                    if (S6)
                    {
                        PS6.BackColor = OnColor;
                    }
                    else
                    {
                        PS6.BackColor = OffColor;
                    }
                }
                else if (PictureBoxNo == 5)
                {
                    if (S5)
                    {
                        PS5.BackColor = OnColor;
                    }
                    else
                    {
                        PS5.BackColor = OffColor;
                    }
                }
                else if (PictureBoxNo == 4)
                {
                    if (S4)
                    {
                        PS4.BackColor = OnColor;
                    }
                    else
                    {
                        PS4.BackColor = OffColor;
                    }
                }
                else
                {
                    if (S3)
                    {
                        PS3.BackColor = OnColor;
                    }
                    else
                    {
                        PS3.BackColor = OffColor;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "UpdateStatusBusPictureBox(int PictureBoxNo) called with PictureBoxNo = " + PictureBoxNo.ToString() + ". ERROR occured is ---> " + ex.Message;
            }
        }

        private void UpdateControlBusPictureBox(int PictureBoxNo)
        {
            Color OnColor = Color.Coral;
            Color OffColor = Color.Lavender;
            try
            {
                if (PictureBoxNo == 3)
                {
                    if (C3)
                    {
                        PC3.BackColor = OnColor;
                        LPC3.BackColor = OnColor;
                        CPC3.BackColor = OnColor;
                    }
                    else
                    {
                        PC3.BackColor = OffColor;
                        LPC3.BackColor = OffColor;
                        CPC3.BackColor = OffColor;
                    }
                }
                else if (PictureBoxNo == 2)
                {
                    if (C2)
                    {
                        PC2.BackColor = OnColor;
                        LPC2.BackColor = OnColor;
                        CPC2.BackColor = OnColor;
                    }
                    else
                    {
                        PC2.BackColor = OffColor;
                        LPC2.BackColor = OffColor;
                        CPC2.BackColor = OffColor;
                    }
                }
                else if (PictureBoxNo == 1)
                {
                    if (C1)
                    {
                        PC1.BackColor = OnColor;
                        LPC1.BackColor = OnColor;
                        CPC1.BackColor = OnColor;
                    }
                    else
                    {
                        PC1.BackColor = OffColor;
                        LPC1.BackColor = OffColor;
                        CPC1.BackColor = OffColor;
                    }
                }
                else
                {
                    if (C0)
                    {
                        PC0.BackColor = OnColor;
                        LPC0.BackColor = OnColor;
                        CPC0.BackColor = OnColor;
                    }
                    else
                    {
                        PC0.BackColor = OffColor;
                        LPC0.BackColor = OffColor;
                        CPC0.BackColor = OffColor;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "UpdateControlBusPictureBox(int PictureBoxNo) called with PictureBoxNo = " + PictureBoxNo.ToString() + ". ERROR occured is ---> " + ex.Message;
            }
        }

        private void UpdateAllDataBusPictureBox()
        {
            try
            {
                for (int i = 0; i <= 7; i++)
                {
                    UpdateDataBusPictureBox(i);
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "UpdateAllDataBusPictureBox() called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void UpdateAllStatusBusPictureBox()
        {
            try
            {
                for (int i = 3; i <= 7; i++)
                {
                    UpdateStatusBusPictureBox(i);
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "UpdateAllStatusBusPictureBox() called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void UpdateAllControlBusPictureBox()
        {
            try
            {
                for (int i = 0; i <= 3; i++)
                {
                    UpdateControlBusPictureBox(i);
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "UpdateAllControlBusPictureBox() called. ERROR occured is ---> " + ex.Message;
            }
        }

        #endregion

        #region "Home Page"

        #region "All Picture Boxes Click Event"

        private void PD7_Click(object sender, EventArgs e)
        {
            try
            {
                D7 = !D7;
                UpdateDataBusPictureBox(7);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PD7_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PD6_Click(object sender, EventArgs e)
        {
            try
            {
                D6 = !D6;
                UpdateDataBusPictureBox(6);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PD6_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PD5_Click(object sender, EventArgs e)
        {
            try
            {
                D5 = !D5;
                UpdateDataBusPictureBox(5);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PD5_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PD4_Click(object sender, EventArgs e)
        {
            try
            {
                D4 = !D4;
                UpdateDataBusPictureBox(4);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PD4_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PD3_Click(object sender, EventArgs e)
        {
            try
            {
                D3 = !D3;
                UpdateDataBusPictureBox(3);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PD3_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PD2_Click(object sender, EventArgs e)
        {
            try
            {
                D2 = !D2;
                UpdateDataBusPictureBox(2);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PD2_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PD1_Click(object sender, EventArgs e)
        {
            try
            {
                D1 = !D1;
                UpdateDataBusPictureBox(1);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PD1_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PD0_Click(object sender, EventArgs e)
        {
            try
            {
                D0 = !D0;
                UpdateDataBusPictureBox(0);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PD0_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }


        private void PS7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Status Pins are unidirectional. We can only read data from these pins. We cannot send data to these pins through software but we can change their status through externally generated signals(High or Low).", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PS6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Status Pins are unidirectional. We can only read data from these pins. We cannot send data to these pins through software but we can change their status through externally generated signals(High or Low).", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PS5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Status Pins are unidirectional. We can only read data from these pins. We cannot send data to these pins through software but we can change their status through externally generated signals(High or Low).", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PS4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Status Pins are unidirectional. We can only read data from these pins. We cannot send data to these pins through software but we can change their status through externally generated signals(High or Low).", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PS3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Status Pins are unidirectional. We can only read data from these pins. We cannot send data to these pins through software but we can change their status through externally generated signals(High or Low).", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void PC3_Click(object sender, EventArgs e)
        {
            try
            {
                C3 = !C3;
                UpdateControlBusPictureBox(3);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PC3_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PC2_Click(object sender, EventArgs e)
        {
            try
            {
                C2 = !C2;
                UpdateControlBusPictureBox(2);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PC2_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PC1_Click(object sender, EventArgs e)
        {
            try
            {
                C1 = !C1;
                UpdateControlBusPictureBox(1);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PC1_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void PC0_Click(object sender, EventArgs e)
        {
            try
            {
                C0 = !C0;
                UpdateControlBusPictureBox(0);
            }
            catch (Exception ex)
            {
                ExceptionOccured = "PC0_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        #endregion

        private void SetDataBusButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetDataBus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "SetDataBusButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ResetDataBusButton_Click(object sender, EventArgs e)
        {
            try
            {
                ResetDataBus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ResetDataBusButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void SetControlBusButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetControlBus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "SetControlBusButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ResetControlBusButton_Click(object sender, EventArgs e)
        {
            try
            {
                ResetControlBus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ResetControlBusButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ReCheckStatusBusButton_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateStatusBus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ReCheckStatusBusButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void WriteToDataBusButton_Click(object sender, EventArgs e)
        {
            try
            {
                WriteToDataBus(Convert.ToInt32(DataBusNumericUpDown.Value));
                UpdateAllDataBusPictureBox();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "WriteToDataBusButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void WriteToControlBusButton_Click(object sender, EventArgs e)
        {
            try
            {
                WriteToControlBus(Convert.ToInt32(ControlBusNumericUpDown.Value));
                //UpdateAllControlBusPictureBox();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "WriteToControlBusButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        #endregion

        #region "Loops"

        bool IsLooping = false;
        bool IsLoopPositive = false;
        int LoopTimerTicker = 0;
        string[] OriginalLoopSequence = new string[] { "C3", "C2", "C1", "C0", "D7", "D6", "D5", "D4", "D3", "D2", "D1", "D0" };
        string[] GeneratedLoopSequence;
        public enum Pin { C3, C2, C1, C0, D7, D6, D5, D4, D3, D2, D1, D0 };

        #region "Synchronous Loops"

        private void SynchronousDataPulseTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (!(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar)))
                {
                    e.Handled = true;
                }
                if (SynchronousDataPulseTextBox.Text.Length == 9)
                {
                    MessageBox.Show("Maximun Allowed Pulse Duration is 999999999. You can enter only upto 9 digit long numbers.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SynchronousDataPulseTextBox.Text = SynchronousDataPulseTextBox.Text.Remove(SynchronousDataPulseTextBox.Text.Length - 1, 1);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "SynchronousDataPulseTextBox_KeyPress(object sender, KeyPressEventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void SynchronousDataPulseTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(SynchronousDataPulseTextBox.Text) == 0)
                {
                    MessageBox.Show("Pulse Duration cannot be zero. It should be atleast 1 ms.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SynchronousDataPulseTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "SynchronousDataPulseTextBox_Leave(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void SynchronousLoopsButton_Click(object sender, EventArgs e)
        {
            try
            {
                AsynchronousLoopsGroupBox.Visible = false;
                SynchronousLoopsGroupBox.Visible = true;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "SynchronousLoopsButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        #endregion

        #region "Asynchronous Loops"

        private void AsynchronousDataPulseTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (!(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar)))
                {
                    e.Handled = true;
                }
                if (AsynchronousDataPulseTextBox.Text.Length == 9)
                {
                    MessageBox.Show("Maximun Allowed Pulse Duration is 999999999. You can enter only upto 9 digit long numbers.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    AsynchronousDataPulseTextBox.Text = AsynchronousDataPulseTextBox.Text.Remove(AsynchronousDataPulseTextBox.Text.Length - 1, 1);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "AsynchronousDataPulseTextBox_KeyPress(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void AsynchronousDataPulseTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(AsynchronousDataPulseTextBox.Text) == 0)
                {
                    MessageBox.Show("Pulse Duration cannot be zero. It should be atleast 1 ms.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    AsynchronousDataPulseTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "AsynchronousDataPulseTextBox_Leave(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void AsynchronousLoopsButton_Click(object sender, EventArgs e)
        {
            try
            {
                AsynchronousLoopsGroupBox.Visible = true;
                SynchronousLoopsGroupBox.Visible = false;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "AsynchronousLoopsButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonC3_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("C3");
                EnableDisableLoopListButtons();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonC3_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonC2_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("C2");
                EnableDisableLoopListButtons();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonC2_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonC1_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("C1");
                EnableDisableLoopListButtons();
            }

            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonC1_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonC0_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("C0");
                EnableDisableLoopListButtons();
            }

            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonC0_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonD7_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("D7");
                EnableDisableLoopListButtons();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonD7_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonD6_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("D6");
                EnableDisableLoopListButtons();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonD6_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonD5_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("D5");
                EnableDisableLoopListButtons();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonD5_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonD4_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("D4");
                EnableDisableLoopListButtons();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonD4_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonD3_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("D3");
                EnableDisableLoopListButtons();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonD3_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonD2_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("D2");
                EnableDisableLoopListButtons();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonD2_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonD1_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.Items.Add("D1");
                EnableDisableLoopListButtons();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonD1_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALButtonD0_Click(object sender, EventArgs e)
        {
            try
            { LoopListListBox.Items.Add("D0"); }
            catch (Exception ex)
            {
                ExceptionOccured = "ALButtonD0_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALoopListMoveUpButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.SelectedIndex = LoopListListBox.SelectedIndex - 1;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALoopListMoveUpButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALoopListMoveDownButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoopListListBox.SelectedIndex = LoopListListBox.SelectedIndex + 1;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALoopListMoveDownButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ALoopListDeleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (LoopListListBox.SelectedIndex != -1)
                {
                    int index = LoopListListBox.SelectedIndex;
                    LoopListListBox.Items.RemoveAt(LoopListListBox.SelectedIndex);
                    if (index < LoopListListBox.Items.Count)
                    {
                        LoopListListBox.SelectedIndex = index;
                    }
                    else
                    {
                        LoopListListBox.SelectedIndex = index - 1;
                    }
                }
                if (LoopListListBox.Items.Count == 0)
                {
                    ALoopListDeleteButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ALoopListDeleteButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void EnableDisableLoopListButtons()
        {
            try
            {
                if (LoopListListBox.Items.Count > 1)
                {
                    ALoopListMoveUpButton.Enabled = true;
                    ALoopListMoveDownButton.Enabled = true;
                }
                else
                {
                    ALoopListMoveUpButton.Enabled = false;
                    ALoopListMoveDownButton.Enabled = false;
                }
                ALoopListDeleteButton.Enabled = true;
                LoopListListBox.SelectedIndex = LoopListListBox.Items.Count - 1;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "EnableDisableLoopListButtons() called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void LoopListListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (LoopListListBox.Items.Count > 1)
                {
                    if (LoopListListBox.SelectedIndex == 0)
                    {
                        ALoopListMoveUpButton.Enabled = false;
                        ALoopListMoveDownButton.Enabled = true;
                    }
                    else if (LoopListListBox.SelectedIndex == LoopListListBox.Items.Count - 1)
                    {
                        ALoopListMoveDownButton.Enabled = false;
                        ALoopListMoveUpButton.Enabled = true;
                    }
                    else
                    {
                        ALoopListMoveUpButton.Enabled = true;
                        ALoopListMoveDownButton.Enabled = true;
                    }
                }
                else
                {
                    ALoopListMoveUpButton.Enabled = false;
                    ALoopListMoveDownButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "LoopListListBox_SelectedIndexChanged(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        #endregion

        private void ResetLoopPinsButton_Click(object sender, EventArgs e)
        {
            try
            {
                ResetControlBus();
                ResetDataBus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ResetLoopPinsButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void EnableDisableLoopControls(bool enable)
        {
            try
            {
                StartLoopButton.Enabled = enable;
                StopLoopButton.Enabled = !enable;
                ResetLoopPinsButton.Enabled = enable;
                SynchronousLoopsGroupBox.Enabled = enable;
                SynchronousLoopsButton.Enabled = enable;
                AsynchronousLoopsButton.Enabled = enable;
                AsynchronousLoopsGroupBox.Enabled = enable;
                IsLooping = !enable;
                _IsBusy = !enable;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "EnableDisableLoopControls(bool enable) called with enable = " + enable.ToString() + " . ERROR occured is ---> " + ex.Message;
            }
        }

        private void StartLoopButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_IsBusy)
                {
                    ResetAll();
                }
                EnableDisableLoopControls(false);
                if (SynchronousLoopsGroupBox.Visible)
                {
                    if (SynchronousLoopHighRadioButton.Checked)
                    {
                        IsLoopPositive = true;
                    }
                    else
                    {
                        IsLoopPositive = false;
                    }
                    GenerateLoopSequence(StartLoopFromComboBox.SelectedIndex, EndLoopAtComboBox.SelectedIndex);
                    LoopTimerTicker = 0;
                    LoopTimer.Interval = Convert.ToInt32(SynchronousDataPulseTextBox.Text);
                    LoopTimer.Enabled = true;
                    StopLoopButton.Focus();
                }
                else
                {
                    if (LoopListListBox.Items.Count == 0)
                    {
                        MessageBox.Show("Loop List is empty.Fill it first", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        EnableDisableLoopControls(true);
                        goto end;
                    }
                    if (AsynchronousLoopHighRadioButton.Checked)
                    {
                        IsLoopPositive = true;
                    }
                    else
                    {
                        IsLoopPositive = false;
                    }
                    GeneratedLoopSequence = new string[LoopListListBox.Items.Count];
                    for (int i = 0; i < LoopListListBox.Items.Count; i++)
                    {
                        GeneratedLoopSequence[i] = LoopListListBox.Items[i].ToString();
                    }
                    LoopTimerTicker = 0;
                    LoopTimer.Interval = Convert.ToInt32(AsynchronousDataPulseTextBox.Text);
                    LoopTimer.Enabled = true;
                    StopLoopButton.Focus();
                end: ;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "StartLoopButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void StopLoopButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoopTimer.Enabled = false;
                EnableDisableLoopControls(true);
                ResetLoopPinsButton.Focus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "StopLoopButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void LoopTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (IsLoopPositive)
                {
                    ResetDataBus();
                    ResetControlBus();
                    SetPinByName(GeneratedLoopSequence[LoopTimerTicker++], true);
                }
                else
                {
                    SetDataBus();
                    SetControlBus();
                    SetPinByName(GeneratedLoopSequence[LoopTimerTicker++], false);
                }
                if (LoopTimerTicker > GeneratedLoopSequence.GetUpperBound(0))
                {
                    LoopTimerTicker = 0;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "LoopTimer_Tick(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void GenerateLoopSequence(int StartIndex, int EndIndex)
        {
            try
            {
                int Diffrence = EndIndex - StartIndex;
                bool IsReverseLoop = false;
                if (Diffrence < 0)
                {
                    Diffrence = -Diffrence;
                    IsReverseLoop = true;
                }
                GeneratedLoopSequence = new string[Diffrence + 1];
                for (int i = 0; i <= Diffrence; i++)
                {
                    if (IsReverseLoop)
                    {
                        GeneratedLoopSequence[i] = OriginalLoopSequence[EndIndex++];
                    }
                    else
                    {
                        GeneratedLoopSequence[i] = OriginalLoopSequence[StartIndex++];
                    }
                }
                if (IsReverseLoop)
                {
                    Array.Reverse(GeneratedLoopSequence);
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "GenerateLoopSequence(int StartIndex, int EndIndex) called with StartIndex = " + StartIndex.ToString() + " , EndIndex = " + EndIndex.ToString() + " . ERROR occured is ---> " + ex.Message;
            }
        }

        private void SetPinByName(string PinName, bool Set)
        {
            try
            {
                if (PinName.Equals("D7"))
                {
                    D7 = Set;
                }
                else if (PinName.Equals("D6"))
                {
                    D6 = Set;
                }
                else if (PinName.Equals("D5"))
                {
                    D5 = Set;
                }
                else if (PinName.Equals("D4"))
                {
                    D4 = Set;
                }
                else if (PinName.Equals("D3"))
                {
                    D3 = Set;
                }
                else if (PinName.Equals("D2"))
                {
                    D2 = Set;
                }
                else if (PinName.Equals("D1"))
                {
                    D1 = Set;
                }
                else if (PinName.Equals("D0"))
                {
                    D0 = Set;
                }
                else if (PinName.Equals("C3"))
                {
                    C3 = Set;
                }
                else if (PinName.Equals("C2"))
                {
                    C2 = Set;
                }
                else if (PinName.Equals("C1"))
                {
                    C1 = Set;
                }
                else if (PinName.Equals("C0"))
                {
                    C0 = Set;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "SetPinByName(string PinName, bool Set) called with PinName = " + PinName.ToString() + " , Set = " + Set.ToString() + " . ERROR occured is ---> " + ex.Message;
            }
        }

        #endregion

        #region "Counters"

        bool IsCounting = false;
        string _CounterType;
        int CountTimerTicker = 0;
        int MaximumCount;
        public enum CounterType { UPCounter, DOWNCounter };
        public enum CountBits { _12, _11, _10, _9, _8, _7, _6, _5, _4, _3, _2, _1 };

        private void CounterPulseTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (!(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar)))
                {
                    e.Handled = true;
                }
                if (CounterPulseTextBox.Text.Length == 9)
                {
                    MessageBox.Show("Maximun Allowed Pulse Duration is 999999999. You can enter only upto 9 digit long numbers.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "CounterPulseTextBox_KeyPress(object sender, KeyPressEventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void CounterPulseTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(CounterPulseTextBox.Text) == 0)
                {
                    MessageBox.Show("Pulse Duration cannot be zero. It should be atleast 1 ms.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "CounterPulseTextBox_Leave(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void ResetCounterPinsButton_Click(object sender, EventArgs e)
        {
            try
            {
                ResetControlBus();
                ResetDataBus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "ResetCounterPinsButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void EnableDisableCounterControls(bool enable)
        {
            try
            {
                StartCounterButton.Enabled = !enable;
                StopCounterButton.Enabled = enable;
                ResetCounterPinsButton.Enabled = !enable;
                CounterBitsCombBox.Enabled = !enable;
                CounterTypeComboBox.Enabled = !enable;
                CounterPulseTextBox.Enabled = !enable;
                CountersCurrentStatusGroupBox.Visible = enable;
                IsCounting = enable;
                _IsBusy = enable;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "EnableDisableCounterControls(bool enable) called with enable = " + enable.ToString() + " . ERROR occured is ---> " + ex.Message;
            }
        }

        private void StartCounterButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_IsBusy)
                {
                    ResetAll();
                }
                EnableDisableCounterControls(true);
                _CounterType = CounterTypeComboBox.SelectedItem.ToString();
                MaximumCount = -(CounterBitsCombBox.SelectedIndex - 12);
                if (_CounterType.Equals("DOWN Counter"))
                {
                    CountTimerTicker = Convert.ToInt32(Math.Pow(2, MaximumCount)) - 1;
                }
                else
                {
                    CountTimerTicker = 0;
                    if (_CounterType.Equals("UP-DOWN Counter"))
                    {
                        UpDownCounterSelectionGroupBox.Visible = true;
                    }
                }
                CounterTimer.Interval = Convert.ToInt32(CounterPulseTextBox.Text);
                CounterTimer.Enabled = true;
                StopCounterButton.Focus();
            }
            catch (Exception ex)
            {
                ExceptionOccured = "StartCounterButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void StopCounterButton_Click(object sender, EventArgs e)
        {
            try
            {
                EnableDisableCounterControls(false);
                UpDownCounterSelectionGroupBox.Visible = false;
                ResetCounterPinsButton.Focus();
                CounterTimer.Enabled = false;
            }
            catch (Exception ex)
            {
                ExceptionOccured = "StopCounterButton_Click(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void CounterTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_CounterType.Equals("UP Counter"))
                {
                    CounterLabel.Text = CountTimerTicker.ToString();
                    SetPinsByBinaryConversion(CountTimerTicker++);
                    if (CountTimerTicker >= Math.Pow(2, MaximumCount))
                    {
                        CountTimerTicker = 0;
                    }
                }
                else if (_CounterType.Equals("DOWN Counter"))
                {
                    CounterLabel.Text = CountTimerTicker.ToString();
                    SetPinsByBinaryConversion(CountTimerTicker--);
                    if (CountTimerTicker < 0)
                    {
                        CountTimerTicker = Convert.ToInt32(Math.Pow(2, MaximumCount)) - 1;
                    }
                }
                else
                {
                    if (UpCounterRadioButton.Checked)
                    {
                        CounterLabel.Text = CountTimerTicker.ToString();
                        SetPinsByBinaryConversion(CountTimerTicker++);
                        if (CountTimerTicker >= Math.Pow(2, MaximumCount))
                        {
                            CountTimerTicker = 0;
                        }
                    }
                    else
                    {
                        CounterLabel.Text = CountTimerTicker.ToString();
                        SetPinsByBinaryConversion(CountTimerTicker--);
                        if (CountTimerTicker < 0)
                        {
                            CountTimerTicker = Convert.ToInt32(Math.Pow(2, MaximumCount)) - 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "CounterTimer_Tick(object sender, EventArgs e) called. ERROR occured is ---> " + ex.Message;
            }
        }

        private void SetPinsByBinaryConversion(int DataInDecimal)
        {
            try
            {
                bool[] SetReset = new bool[12];
                for (int i = 0; i < 12; i++)
                {
                    SetReset[i] = Convert.ToBoolean(DataInDecimal & Convert.ToInt32(Math.Pow(2, i)));
                }

                if (SetReset[0])
                {
                    D0 = true;
                }
                else
                {
                    D0 = false;
                }

                if (SetReset[1])
                {
                    D1 = true;
                }
                else
                {
                    D1 = false;
                }

                if (SetReset[2])
                {
                    D2 = true;
                }
                else
                {
                    D2 = false;
                }

                if (SetReset[3])
                {
                    D3 = true;
                }
                else
                {
                    D3 = false;
                }

                if (SetReset[4])
                {
                    D4 = true;
                }
                else
                {
                    D4 = false;
                }

                if (SetReset[5])
                {
                    D5 = true;
                }
                else
                {
                    D5 = false;
                }

                if (SetReset[6])
                {
                    D6 = true;
                }
                else
                {
                    D6 = false;
                }

                if (SetReset[7])
                {
                    D7 = true;
                }
                else
                {
                    D7 = false;
                }

                if (SetReset[8])
                {
                    C0 = true;
                }
                else
                {
                    C0 = false;
                }

                if (SetReset[9])
                {
                    C1 = true;
                }
                else
                {
                    C1 = false;
                }

                if (SetReset[10])
                {
                    C2 = true;
                }
                else
                {
                    C2 = false;
                }

                if (SetReset[11])
                {
                    C3 = true;
                }
                else
                {
                    C3 = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured = "SetPinsByBinaryConversion(int DataInDecimal) called with DataInDecimal = " + DataInDecimal.ToString() + " . ERROR occured is ---> " + ex.Message;
            }
        }

        #endregion

    }
}