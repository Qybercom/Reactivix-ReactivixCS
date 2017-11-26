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

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.Thread.Stop((IReactivixThread thread) => {
                ClientThread client = thread as ClientThread;

                Program.Log("Program.close");

                client.Client.Close();
            });
        }
    }

    class TestServiceDTO : IQuarkNetworkPacketData
    {
        public string message { get; set; }
    }
}
