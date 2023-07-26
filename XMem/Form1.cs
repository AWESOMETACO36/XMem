using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JRPC_Client;
using XDevkit;

namespace XMem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        IXboxConsole xbox;
        private void button1_Click(object sender, EventArgs e)
        {
            xbox.Connect(out xbox);
            if (xbox.Connect(out xbox) == true)
            {
                xbox.XNotify("Tool Connected");
                MessageBox.Show("Tool Sucessfully Connected");
            }
            else
            {
                MessageBox.Show("Tool Failed to Connect!");
            }
        }
    }
}
