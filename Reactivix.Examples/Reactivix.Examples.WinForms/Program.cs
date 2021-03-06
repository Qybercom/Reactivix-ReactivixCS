﻿using System;
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

            Client.Response<TestResponseDTO>("/test", TestResponseCallback);
            Client.Event<TestEventDTO>("/test", TestEventCallback);

            Client.Connect();
        }

        public void ReactivixThreadStop(ReactivixThread context)
        {
            //throw new NotImplementedException();
        }

        private void _client_OnConnect(QuarkNetworkClient client)
        {
            Program.Thread.External((object context) => {
                Program.Log("Program.Connect");
            });
        }

        public void ReactivixThreadPipe(ReactivixThread context)
        {
            Client.Pipe();
        }

        private void _client_OnClose(QuarkNetworkClient client)
        {
            Program.Thread.External((object context) => {
                Program.Log("Program.Close");
            });
        }

        private void _client_OnError(QuarkNetworkClient client, Exception e)
        {
            Program.Thread.External((object context) => {
                Program.Log("Program.Error: " + e.Message);
            });
        }

        public void ResponseMessageWelcome(QuarkNetworkClient client, QuarkNetworkPacket packet)
        {
            Program.Thread.External((object context) => {
                MessageWelcome data = (MessageWelcome)packet.Data;

                Program.Log("Program.response '" + data.message + "'");
            });
        }

        public void EventMessageWelcome(QuarkNetworkClient client, QuarkNetworkPacket packet)
        {
            Program.Thread.External((object context) => {
                MessageWelcome data = (MessageWelcome)packet.Data;

                Program.Log("Program.event '" + data.message + "'");
            });
        }

        public void TestResponseCallback(QuarkNetworkClient client, QuarkNetworkPacket packet)
        {
            Program.Thread.External((object context) => {
                TestResponseDTO data = (TestResponseDTO)packet.Data;

                Program.Log("Program.response.test '" + data.status + "'");
            });
        }

        public void TestEventCallback(QuarkNetworkClient client, QuarkNetworkPacket packet)
        {
            Program.Thread.External((object context) => {
                TestEventDTO data = (TestEventDTO)packet.Data;
                Form1 form = context as Form1;

                form.label1.Text = data.message;

                Program.Log("Program.event.test '" + data.message + "'");
            });
        }
    }

    public class MessageWelcome : IQuarkNetworkPacketData
    {
        public int status { get; set; }
        public string message { get; set; }
    }

    public class TestResponseDTO : IQuarkNetworkPacketData
    {
        public int status { get; set; }
    }

    public class TestEventDTO : IQuarkNetworkPacketData
    {
        public string message{ get; set; }
    }

    static class Program
    {
        public static ReactivixThread Thread { get; set; }

        public static void Log(string message, string lvl = "info")
        {
            Debug.WriteLine("[" + lvl + "] [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId + "] " + message);
        }

        public static void TestAttr()
        {
            System.Diagnostics.Debug.WriteLine("Hook");
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
            //Application.Idle += Application_Idle;

            //Application.Run(new Form1());
            Form1 main = new Form1();
            main.Show();

            var run = true;
            while (run)
            {
                Application.DoEvents();
                Thread.Pipe(main);
                System.Threading.Thread.Sleep(10);
            }

            //Application.RegisterMessageLoop()
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            Thread.Pipe();
        }
    }
}