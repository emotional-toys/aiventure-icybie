using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace CybieCommunicator
{
    public partial class MainForm : Form
    {
        int decData = 0;
        int decAdd = 888; // 378h Selected Default

        public MainForm()
        {
            InitializeComponent();
        }

        private void testConnectionButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                Cybie.PortControl.Output(decAdd, decData); // Initialize
            }
            catch(System.DllNotFoundException)
            {
                MessageBox.Show("inpout32.dll Not Found !!!","Dll Error");
                System.Environment.Exit(-1);
            }
        }
    }
}
