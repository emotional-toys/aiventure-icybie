using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CybieCommunicator.Cybie
{
    /// <summary>
    /// Imports a Windows dll.
    /// </summary>
    public class PortControl 
    {
        [DllImport("inpout32.dll")]
        public static extern short Inp32(int address);
        [DllImport("inpout32.dll", EntryPoint = "Out32")]
        public static extern void Output(int address, int value); // decimal
    }
}
