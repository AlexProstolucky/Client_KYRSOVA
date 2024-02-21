using Client.Services.Audio;
using Client.Services.FileSend;
using Client.Services.Network.Utilits;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

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
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string NickName { get; set; }

        public List<Guid> Friends { get; set; } = new List<Guid>();
        public string IP { get; set; }
        #endregion
        public string ServerAddress { get; set; }
        public string ServerName { get; set; }

        public bool IsConnected { get; set; }

        private string _file_path_transfer = "E:\\Курсова\\Client\\Client\\Client\\Services\\FileSend\\FileBuff";
        private int file_port = 63001;
        private VoiceCallHandler voiceCallHandler;
        private int Aviable_server_port = 0;
        #endregion

        #region Consructor
        public ChatClient(int port, string serverAddress, string email, string password, string IP)
        {
            try
            {
                server = new TcpClient(serverAddress, port);
                IsConnected = true;
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
            Data data = new(Command.Auth, "Client", "Server", IP, email + " " + CreateMD5(password));
            SendComamnd(data);
        }

        public ChatClient(int port, string serverAddress, string IP, string login, string password, string nickName, string email, DateTime? birthday)
        {
            try
            {
                server = new TcpClient(serverAddress, port);
                IsConnected = true;
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
            Data data = new(Command.Reg, "Client", "Server", IP, login + " " + CreateMD5(password) + " " + nickName + " " + email + " " + birthday.ToString());
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
            IP = "127.0.0.1";
            //IP = $"{endPoint.Address}:{endPoint.Port}";
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
                ParseMessage(Data.GetBytes(state.Buffer));

                server.Client.BeginReceive(state.Buffer, 0, ChatHelper.StateObject.BUFFER_SIZE, 0, OnReceive, state);
            }
            catch (SocketException)
            {
                IsConnected = false;
                server.Client.Disconnect(true);
            }
        }

        private void ParseMessage(Data data)
        {
            if (data.Command == Command.Good_Auth)
            {
                Console.WriteLine(data.Command.ToString());
            }
            else if (data.Command == Command.Bad_Auth)
            {
                Console.WriteLine(data.Command.ToString());
            }
            else if (data.Command == Command.Good_Reg)
            {
                Console.WriteLine(data.Command.ToString());
            }
            else if (data.Command == Command.Bad_Reg)
            {
                Console.WriteLine(data.Command.ToString());
            }
            else if (data.Command == Command.Accept_Port)
            {
                Aviable_server_port = int.Parse(data.Message);
            }
            else if (data.Command == Command.Accept_File)
            {
                //TODO зберігати ін меморі або хз файл зробити хуй зна (назва файлу)
            }
            else if (data.Command == Command.Cancel_Call)
            {
                voiceCallHandler.StopReceive();
                voiceCallHandler.StopSend();
                Console.WriteLine(data.Command.ToString());
            }
            else if (data.Command == Command.Request_Call)
            {

                //чи юзер прийняв дзвінок

                //yes
                IPEndPoint ipEndPointReceive = new(IPAddress.Parse(""), 60202);// 60202
                IPEndPoint ipEndPointSend = new(IPAddress.Parse(data.ClientAddress), 60201);// 60201
                SendComamnd(new(Command.Accept_Call, data.To, data.From, IP, ""));
                voiceCallHandler = new(ipEndPointReceive, ipEndPointSend);
                voiceCallHandler.Receive();
                voiceCallHandler.Send();

            }
            else if (data.Command == Command.Accept_Call)
            {

                IPEndPoint ipEndPointReceive = new(IPAddress.Parse(""), 60201);// 60201
                IPEndPoint ipEndPointSend = new(IPAddress.Parse(data.ClientAddress), 60202);// 60202
                voiceCallHandler = new(ipEndPointReceive, ipEndPointSend);
                voiceCallHandler.Receive();
                voiceCallHandler.Send();

            }
        }
        public void TryCall(Guid friendGuid)
        {

            SendComamnd(new Data(Command.Request_Call, Id.ToString(), friendGuid.ToString(), IP, ""));

        }
        public void EndCall(Guid friendGuid)
        {

            SendComamnd(new Data(Command.Cancel_Call, Id.ToString(), friendGuid.ToString(), IP, ""));
            voiceCallHandler.Receive();
            voiceCallHandler.Send();
        }
        private void SendComamnd(Data data)
        {
            server.Client.Send(data.ToBytes());
        }
        public bool GetFile(string filename)
        {
            if (Aviable_server_port == 0)
            {
                SendComamnd(new Data(Command.Accept_Port, Id.ToString(), "Server", IP, ""));
                return false;
            }
            else
            {
                SendComamnd(new Data(Command.Send_File, Id.ToString(), "Server", IP, filename + " " + Aviable_server_port.ToString()));
                Thread.Sleep(1500);
                FileTool.ReceiverFile(ServerAddress, Aviable_server_port, filename);
                Aviable_server_port = 0;
                return true;
            }
        }
        public void SendFile(string filename, int friend_ID)
        {
            Data data = new(Command.Accept_File, Id.ToString(), friend_ID.ToString(), IP, filename);
            SendComamnd(data);
            FileTool.SendFile(_file_path_transfer, file_port);
        }
        public void CloseConnection()
        {
            IsConnected = false;

            var data = new Data(Command.Disconnect, Id.ToString(), "Server", IP, "");
            if (server.Client.Connected)
                server.Client.Send(data.ToBytes());
        }
        #endregion

        static string CreateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);

                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }
    }
}
