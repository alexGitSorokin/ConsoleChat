using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiClient
{
    class Program
    {
        #region Fields
        private static string _clientName = string.Empty;

        private static byte[] recBuf = new byte[256];

        private static readonly Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int PORT = 100;
        #endregion

        #region Methods
        static void Main()
        {
            Console.Title = "Client";
            ConnectToServer();
            RequestLoopAsync();
            Exit();
        }

        private static void ConnectedCallback()
        {
            ClientSocket.BeginReceive(recBuf, 0, recBuf.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), ClientSocket);
        }

        private static void ReceivedCallback(IAsyncResult iar)
        {
            Socket s = (Socket)iar.AsyncState;
            int rec = s.EndReceive(iar);
            byte[] dataBuf = new byte[rec];

            Buffer.BlockCopy(recBuf, 0, dataBuf, 0, rec);

            string q = Encoding.ASCII.GetString(dataBuf);

            Console.WriteLine(q);

            s.BeginReceive(recBuf, 0, recBuf.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), s);
        }


        private static void ConnectToServer()
        {
            int attempts = 0;

            Console.WriteLine("Enter your name:");
            _clientName = Console.ReadLine();

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt " + attempts);

                    ClientSocket.Connect(IPAddress.Loopback, PORT);

                    ConnectedCallback();
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }

        private static void RequestLoopAsync()
        {
            Console.WriteLine(@"<Type ""exit"" to properly disconnect client>");

            while (true)
            {
                SendRequest();
            }
        }


        private static void Exit()
        {
            SendString("exit");
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        private async static void SendRequest()
        {
            string userMessage = Console.ReadLine();

            string finishMessage = $"{_clientName}: {userMessage}";

            SendString(finishMessage);

            if (userMessage.ToLower() == "exit")
            {
                Exit();
            }
        }

        private static void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }
        #endregion
    }
}
