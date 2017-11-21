using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

using Reactivix.Thread;
using Reactivix.Quark;
using Reactivix.Network.Transports;

namespace Reactivix.Examples.WinForms
{
    public class ClientThread : IReactivixThread
    {
        public QuarkNetworkClient Client { get; set; }

        public ClientThread()
        {
            Client = new QuarkNetworkClient(new ReactivixNetworkTransportTCP(), "127.0.0.1", 8500);
        }

        public void ReactivixThreadStart(ReactivixThread context)
        {
            Client.OnConnect += _client_OnConnect;
            Client.OnClose += _client_OnClose;
            Client.OnError += _client_OnError;

            Client.Response<MessageWelcome>("/", ResponseMessageWelcome);
            Client.Event<MessageWelcome>("/", EventMessageWelcome);

            Client.Connect();
        }

        private void _client_OnConnect(QuarkNetworkClient client)
        {
            Program.Thread.External(() => {
                Program.Log("Program.Connect");
            });
        }

        public void ReactivixThreadPipe(ReactivixThread context)
        {
            Client.Pipe();
        }

        private void _client_OnClose(QuarkNetworkClient client)
        {
            Program.Thread.External(() => {
                Program.Log("Program.Close");
            });
        }

        private void _client_OnError(QuarkNetworkClient client, Exception e)
        {
            Program.Thread.External(() => {
                Program.Log("Program.Error: " + e.Message);
            });
        }

        public void ResponseMessageWelcome(QuarkNetworkPacket e)
        {
            Program.Thread.External(() => {
                MessageWelcome data = (MessageWelcome)e.Data;

                Program.Log("Program.response '" + data.message + "'");
            });
        }

        public void EventMessageWelcome(QuarkNetworkPacket e)
        {
            Program.Thread.External(() => {
                MessageWelcome data = (MessageWelcome)e.Data;

                Program.Log("Program.event '" + data.message + "'");
            });
        }
    }

    public class MessageWelcome : IQuarkNetworkPacketData
    {
        public int status { get; set; }
        public string message { get; set; }
    }

    static class Program
    {
        public static ReactivixThread Thread { get; set; }

        public static void Log(string message, string lvl = "info")
        {
            Debug.WriteLine("[" + lvl + "] [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId + "] " + message);
        }

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread = new ReactivixThread(new ClientThread());

            Thread.Start((IReactivixThread context) => {
                Log("Init.Thread");
            });
            /*
            bool run = true;
            while (run)
            {
                Thread.Pipe();
                System.Threading.Thread.Sleep(10);
            }
            */

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //ApplicationContext context = new ApplicationContext(new Form1());
            //context.
            //Application.Run();
            Application.Idle += Application_Idle;

            Application.Run(new Form1());

            //Application.RegisterMessageLoop()
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            Thread.Pipe();
        }
    }
}