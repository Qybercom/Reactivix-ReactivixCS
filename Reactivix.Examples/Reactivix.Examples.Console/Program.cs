using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactivix.Thread;
using Reactivix.Quark;
using Reactivix.Network.Transports;

namespace Reactivix.Examples.Console
{
    public class ClientThread : IReactivixThread
    {
        private QuarkNetworkClient _client { get; set; }

        public ClientThread()
        {
            _client = new QuarkNetworkClient(new ReactivixNetworkTransportTCP(), "127.0.0.1", 8500);
        }

        public void ReactivixThreadStart(ReactivixThread context)
        {
            _client.OnConnect += _client_OnConnect;
            _client.OnClose += _client_OnClose;
            _client.OnError += _client_OnError;

            _client.Response<MessageWelcome>("/", ResponseMessageWelcome);
            _client.Event<MessageWelcome>("/", EventMessageWelcome);

            _client.Connect();
        }

        private void _client_OnConnect(QuarkNetworkClient client)
        {
            Program.Thread.External(() => {
                Program.Log("Program.Connect");
            });
        }

        public void ReactivixThreadPipe(ReactivixThread context)
        {
            _client.Pipe();
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

    class Program
    {
        public static ReactivixThread Thread { get; set; }

        public static void Log(string message, string lvl = "info")
        {
            System.Console.WriteLine("[" + lvl + "] [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId + "] " + message);
        }

        static void Main(string[] args)
        {
            Log("Init.Main");

            Thread = new ReactivixThread(new ClientThread());

            Thread.Start((IReactivixThread context) => {
                Log("Init.Thread");
            });

            bool run = true;
            while (run)
            {
                Thread.Pipe();
                System.Threading.Thread.Sleep(10);
            }
        }
    }

    public class MessageWelcome : IQuarkNetworkPacketData
    {
        public int status { get; set; }
        public string message { get; set; }
    }
}