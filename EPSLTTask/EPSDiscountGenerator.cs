using DiscountGeneratorService.Handlers;
using System.Net;
using System.Net.Sockets;
using TCPLibrary;

namespace DiscountGeneratorService
{
    static public class EPSDiscountGenerator
    {
        public static int Port { get; set; }
        public static TcpListener TcpListener { get; set; }

        public delegate void PacketHandler(int fromClient, Packet packet);

        public static Dictionary<int, Func<int, Packet, CancellationToken, Task>> packetHandlers;

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public static string path = Directory.GetCurrentDirectory();

        public static Dictionary<string, bool> Codes;

        public static FileStorageHandler fileStorageHandler;

        public static int CallTimeout = 360;

        public static int clientConnectCounter = 0;

        public static void Start(int port)
        {
            Port = port;

            Console.WriteLine($"Starting EPSDiscountGenerator on {Directory.GetCurrentDirectory()}...");

            InitializePacketMapping();

            ClearTempData();

            fileStorageHandler = new FileStorageHandler(path);
            clients = new Dictionary<int, Client>();
            TcpListener = new TcpListener(IPAddress.Any, Port);
            TcpListener.Start();
            TcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

            Codes = fileStorageHandler.ReadStoredCodesToMemory();

            Console.WriteLine($"EPSDiscountGenerator Service hosted on Port {Port}.");
        }

        public static void Loop()
        {
            var addedCodes = fileStorageHandler.InsertCodesIntoStorage();
            foreach(var code in addedCodes)
            {
                Codes[code] = true;
            }
            var usedCodes = fileStorageHandler.ProcessCodeActivations();
            foreach (var code in usedCodes)
            {
                if (Codes.ContainsKey(code))
                {
                    Codes[code] = false;
                    Console.WriteLine($"Activated {code}.");
                }
            }
        }

        static void TcpConnectCallback(IAsyncResult result)
        {
            TcpClient client = TcpListener.EndAcceptTcpClient(result);
            TcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);  
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            clientConnectCounter++;
            clients.Add(clientConnectCounter, new Client(clientConnectCounter));

            var cts = new CancellationTokenSource();

            var task = clients[clientConnectCounter]._tcp.ConnectAsync(client, clientConnectCounter, cts.Token);

            Task.Delay(CallTimeout).ContinueWith(_ => cts.Cancel());
        }

        private static void ClearTempData()
        {
            var TempPath = $"{path}/Temp";
            if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);

            DirectoryInfo di = new DirectoryInfo(TempPath);


            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private static void InitializePacketMapping()
        {
            packetHandlers = new Dictionary<int, Func<int, Packet, CancellationToken, Task>>
            {
                { (int)RequestPacket.Generate, DiscountCodeHandler.GenerateAsync },
                { (int)RequestPacket.UseCode, DiscountCodeHandler.UseCodeAsync }
            };

            Console.WriteLine($"Service packets initialized...");
        }
    }
}
