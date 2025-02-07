using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AccessGdb;

namespace InputTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SecondForm());
        }
    }
}
