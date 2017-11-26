using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Reactivix.Quark;
using Reactivix.Thread;

namespace Reactivix.Examples.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.Thread.Internal((IReactivixThread thread) => {
                ClientThread client = thread as ClientThread;

                Program.Log("Program.service.test");
                client.Client.Service("/test", new TestServiceDTO() {
                    message = textBox1.Text
                });
            });
        }
    }

    class TestServiceDTO : IQuarkNetworkPacketData
    {
        public string message { get; set; }
    }
}
