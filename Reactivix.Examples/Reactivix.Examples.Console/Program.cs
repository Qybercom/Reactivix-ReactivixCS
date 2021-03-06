﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactivix.Thread;
using Reactivix.Quark;
using Reactivix.Network;
using Reactivix.Network.Transports;
using Reactivix.IOProcessors;
using Reactivix.OAuth;

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

        public void ReactivixThreadPipe(ReactivixThread context)
        {
            _client.Pipe();
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
    }

    public partial class Program
    {
        public static ReactivixThread Thread { get; set; }

        public static void Log(string message, string lvl = "info")
        {
            System.Console.WriteLine("[" + lvl + "] [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId + "] " + message);
        }

        /// <summary>
        /// Define your own Main() in a partial class in ProgramLocal.cs
        /// </summary>
        /// <param name="args"></param>
        static void MainFallback(string[] args)
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

    public class ResponseAPIIndex : IQuarkNetworkPacketData
    {
        public int status { get; set; }
        public ResponseAPIIndexApp app { get; set; }
    }

    public class ResponseAPIIndexApp
    {
        public string name { get; set; }
        public string logo { get; set; }
    }
}