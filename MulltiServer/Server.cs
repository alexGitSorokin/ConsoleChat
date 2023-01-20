using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MulltiServer
{
    public class Server
    {
        #region Fields

        private static readonly byte[] _buffer = new byte[BUFFER_SIZE];

        private const int BUFFER_SIZE = 256;
        private const int PORT = 100;

        private static readonly Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> _clientSockets = new List<Socket>();

        #endregion

        #region Methods

        public void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            _serverSocket.Listen(2);
            _serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = _serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            _clientSockets.Add(socket);
            socket.BeginReceive(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected...");
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult asyncResult)
        {
            Socket current = (Socket)asyncResult.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(asyncResult);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                current.Close();
                _clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine(text);
            byte[] data = Encoding.ASCII.GetBytes(text);

            foreach (var socket in _clientSockets)
            {
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.BeginReceive(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            }
        }

        private static void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }

        public void CloseAllSockets()
        {
            foreach (Socket socket in _clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            _serverSocket.Close();
        }
        #endregion
    }
}
