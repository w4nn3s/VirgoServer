using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace VirgoServer
{
    class VirgoServer
    {
        public static Socket serverSocket;
        public static byte[] _buffer;

        public static ManualResetEvent allDone = new ManualResetEvent(false);


        private static void StartServer()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 4444));
            serverSocket.Listen(0);
            Console.WriteLine("-  Starting server");
            Console.WriteLine("-  Waiting for incoming connections...\n");
            while (true)
            {
                allDone.Reset();
                
                serverSocket.BeginAccept(new AsyncCallback(AcceptConnCallback), serverSocket);
                allDone.WaitOne();
            }
        }

        private static void AcceptConnCallback(IAsyncResult AR)
        {
            try
            {
                Socket s_clientSocket = serverSocket.EndAccept(AR);
                _buffer = new byte[1024];
                allDone.Set();
                Console.WriteLine("-  Accepted connection from {0}" ,s_clientSocket.RemoteEndPoint);
                s_clientSocket.BeginReceive(_buffer, 0, _buffer.Length,SocketFlags.None, new AsyncCallback(ReceivedBufferCallback), s_clientSocket);
                serverSocket.BeginAccept(new AsyncCallback(AcceptConnCallback), serverSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ReceivedBufferCallback(IAsyncResult AR)
        {
            try
            {
                Socket s_clientSocket = AR.AsyncState as Socket;
                int packetSize = s_clientSocket.EndReceive(AR);

                byte[] packet = new byte[packetSize];
                Array.Copy(_buffer, packet,packetSize);

                string text = Encoding.ASCII.GetString(packet);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nd. Read {0} bytes in packet. from client {1}", packet.Length,s_clientSocket.RemoteEndPoint);
                Console.WriteLine("This is happening in thread:{0}");

                Console.ResetColor();
                ProcessReceivedBuffer(text);

                //restart for new conn
                _buffer = new byte[1024];
                s_clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedBufferCallback), s_clientSocket);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static void ProcessReceivedBuffer(string text)
        {
                 Console.WriteLine("" + text);
        }

        public static int Main(string[] args)
        {
            StartServer();
            return 0;
        }
    }
}
