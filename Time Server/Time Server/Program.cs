using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Time_Server {
    class Program {

        private static byte[] _buffer = new byte[1024];

        private static List<Socket> _clientSockets = new List<Socket>();

        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args) {

            Console.Title = "Server";
            SetupServer();
            Console.ReadLine();
        }

        private static void SetupServer() {
            Console.WriteLine("Setting up Server ...");
            
            // Binding to port: 100
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));

            // Setting up a backlog
            _serverSocket.Listen(5);

            // Start listening for connections
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            
        }

        private static void AcceptCallback(IAsyncResult AR) {
            Socket socket = _serverSocket.EndAccept(AR);
            _clientSockets.Add(socket);

            Console.WriteLine("Client Connected");

            // Begin receiving data for each client socket
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);

            // Start listening for connections again
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

        }

        private static void ReceiveCallback(IAsyncResult AR) {
            Socket socket = (Socket)AR.AsyncState;
            int received = socket.EndReceive(AR);
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);

            string text = Encoding.ASCII.GetString(dataBuf);
            Console.WriteLine("Text received: "+text);

            string response = string.Empty;

            if(text.ToLower() != "Get Time") {
                response = "Invalid Request";
            } else {
                response = DateTime.Now.ToLongTimeString();
            }

            byte[] data = Encoding.ASCII.GetBytes(response);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);

            // Start listening for connections again
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void SendCallback(IAsyncResult AR) {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
    }
}
