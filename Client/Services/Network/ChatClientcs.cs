using Client.Services.FileSend;
using Client.Services.Network.Utilits;
using System.Net;
using System.Net.Sockets;

namespace Client.Services.Network
{
    public class ChatClient
    {
        #region Fields

        private readonly TcpClient server;
        private readonly Socket clientSocket;
        private readonly IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;
        private Thread tcpRecieveThread;
        #endregion

        #region Events
        public event EventHandler UserListReceived;
        public event EventHandler MessageReceived;
        public event EventHandler CallRecieved;
        public event EventHandler CallRequestResponded;
        public event EventHandler FileRecieved;
        #endregion

        #region Properties

        #region Profile Info
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string IP { get; set; }
        #endregion
        public string ServerAddress { get; set; }
        public string ServerName { get; set; }

        public bool IsConnected { get; set; }

        private string _file_path_transfer { get; set; }
        private int file_port = 63001;
        #endregion

        #region Consructor
        /// <summary>
        /// Connect to server
        /// </summary>
        /// <param name="port"></param>
        /// <param name="serverAddress"></param>
        /// <param name="userName"></param>
        public ChatClient(int port, string serverAddress, string email, string password, string IP)
        {
            try
            {
                server = new TcpClient(serverAddress, port);
                IsConnected = true;
                // Creating new udp client socket
                clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);
                localEndPoint = GetHostEndPoint();
                ServerAddress = serverAddress;
            }
            catch (SocketException)
            {
                Console.WriteLine("Socket Exception");
                return;
            }
            Data data = new(Command.Auth, "Client", "Server", IP, email + " " + password);
            SendComamnd(data);
        }

        public ChatClient(int port, string serverAddress, string IP, string login, string password, string nickName, string email, DateTime? birthday)
        {
            try
            {
                server = new TcpClient(serverAddress, port);
                IsConnected = true;
                // Creating new udp client socket
                clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);
                localEndPoint = GetHostEndPoint();
                ServerAddress = serverAddress;
            }
            catch (SocketException)
            {
                Console.WriteLine("Socket Exception");
                return;
            }
            Data data = new(Command.Auth, "Client", "Server", IP, login + " " + password + " " + nickName + " " + email + " " + birthday.ToString());
            SendComamnd(data);
        }
        #endregion

        #region Methods

        private void BindSocket()
        {
            if (!clientSocket.IsBound)
                clientSocket.Bind(localEndPoint);
        }

        private IPEndPoint GetHostEndPoint()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            if (ipAddress == null)
                return null;
            var random = new Random();
            var endPoint = new IPEndPoint(ipAddress, random.Next(65000, 65536));
            IP = $"{endPoint.Address}:{endPoint.Port}";
            return endPoint;
        }


        public void Init()
        {
            tcpRecieveThread = new Thread(RecieveFromServer) { Priority = ThreadPriority.Normal };
            tcpRecieveThread.Start();
        }

        private void RecieveFromServer()
        {
            var state = new ChatHelper.StateObject
            {
                WorkSocket = server.Client
            };
            while (IsConnected)
            {
                if (IsReceivingData)
                    continue;
                IsReceivingData = true;
                server.Client.BeginReceive(state.Buffer, 0, ChatHelper.StateObject.BUFFER_SIZE, 0,
                    OnReceive, state);
            }
        }

        public bool IsReceivingData { get; set; }

        public void OnReceive(IAsyncResult ar)
        {
            var state = ar.AsyncState as ChatHelper.StateObject;
            if (state == null)
                return;
            var handler = state.WorkSocket;
            if (!handler.Connected)
                return;
            try
            {
                var bytesRead = handler.EndReceive(ar);
                if (bytesRead <= 0)
                    return;
                //ParseMessage(Data.GetBytes(state.Buffer));

                server.Client.BeginReceive(state.Buffer, 0, ChatHelper.StateObject.BUFFER_SIZE, 0, OnReceive, state);
            }
            catch (SocketException)
            {
                IsConnected = false;
                server.Client.Disconnect(true);
            }
        }


        public void OnUdpRecieve(IAsyncResult ar)
        {
        }

        //public void ParseMessage(Data data)
        //{
        //    switch (data.Command)
        //    {

        //    }
        //}
        private string ParseFileExtension(string fileName)
        {
            return System.IO.Path.GetExtension(fileName);
        }

        private void ParseResponse(string user, Command response, string address)
        {
            switch (response)
            {

            }
        }
        private void ReceiveUdpData()
        {


        }

        private void SendComamnd(Data data)
        {
            server.Client.Send(data.ToBytes());
        }
        public void SendFile(string filename, int friend_ID)
        {
            Data data = new(Command.Accept_File, Id.ToString(), friend_ID.ToString(), IP, filename);
            FileTool.SendFile(_file_path_transfer, file_port);
        }
        public void CloseConnection()
        {
            IsConnected = false;

            //var data = new Data { Command = Command.Disconnect };
            //if (server.Client.Connected)
            //    server.Client.Send(data.ToByte());
        }
        #endregion
    }
}
