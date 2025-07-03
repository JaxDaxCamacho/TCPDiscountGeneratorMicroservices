using DiscountGeneratorService.Handlers;
using DiscountGeneratorService.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using TCPLibrary;

namespace DiscountGeneratorService
{
    public class EPSDiscountGenerator : IDiscountGenerator
    {
        int Port;

        public TcpListener TcpListener;

        public delegate void PacketHandler(int fromClient, Packet packet);

        public Dictionary<int, Client> Clients;

        public Dictionary<string, bool> Codes;

        readonly IFileStorageHandler _fileStorageHandler;

        int CallTimeout = 360;

        int clientConnectCounter = 0;

        public DiscountCodeHandler RequestHandler { get; }

        public EPSDiscountGenerator(IFileStorageHandler fileStorageHandler)
        {
            _fileStorageHandler = fileStorageHandler;
            Codes = new Dictionary<string, bool>();
            Clients = new Dictionary<int, Client>();
            RequestHandler = new DiscountCodeHandler(this, _fileStorageHandler);

            Console.WriteLine($"Service initialized...");
        }

        public void Start(int port)
        {
            Port = port;

            Console.WriteLine($"Starting EPSDiscountGenerator on {Directory.GetCurrentDirectory()}...");

            TcpListener = new TcpListener(IPAddress.Any, Port);
            TcpListener.Start();
            TcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

            Codes = _fileStorageHandler.ReadStoredCodesToMemory();

            Console.WriteLine($"EPSDiscountGenerator Service hosted on Port {Port}.");
        }

        public Dictionary<string, bool> GetCodesInMemory()
        {
            return Codes;
        }

        public void UpdateCodesInMemory(string code, bool active)
        {
            if(Codes.TryGetValue(code, out var currentActiveState))
            {
                Codes[code] = active;
            }
            else
            {
                Console.WriteLine($"the code {code} doesn't exist");
            }
        }

        public void AddCodeToMemory(string code)
        {
            Codes[code] = true;
        }

        public void Loop()
        {
            var addedCodes = _fileStorageHandler.InsertCodesIntoStorage();
            foreach(var code in addedCodes)
            {
                Codes[code] = true;
            }
            var usedCodes = _fileStorageHandler.ProcessCodeActivations();
            foreach (var code in usedCodes)
            {
                if (Codes.ContainsKey(code))
                {
                    Codes[code] = false;
                    Console.WriteLine($"Activated {code}.");
                }
            }
        }

        void TcpConnectCallback(IAsyncResult result)
        {
            TcpClient client = TcpListener.EndAcceptTcpClient(result);
            TcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);  
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            clientConnectCounter++;
            Clients.Add(clientConnectCounter, new Client(clientConnectCounter, this));

            var cts = new CancellationTokenSource();

            var task = Clients[clientConnectCounter]._tcp.ConnectAsync(client, clientConnectCounter, cts.Token);

            Task.Delay(CallTimeout).ContinueWith(_ => cts.Cancel());
        }

        async Task SendTCPData(int toClient, Packet packet, CancellationToken ct)
        {
            packet.WriteLength();
            await Clients[toClient]._tcp.SendDataAsync(packet, ct);
        }

        public async Task HandshakeAsync(int toClient, int clientId, CancellationToken ct)
        {
            using (Packet packet = new Packet((int)ResponsePacket.Handshake))
            {
                packet.Write(clientId);
                packet.Write(toClient);

                await SendTCPData(toClient, packet, ct);
            }
        }

        public async Task GenerateAsync(int toClient, string msg, CancellationToken ct)
        {
            using (Packet packet = new Packet((int)ResponsePacket.Generate))
            {
                packet.Write(toClient);

                await SendTCPData(toClient, packet, ct);
            }
        }

        public async Task UseCodeAsync(int toClient, short discountValue, CancellationToken ct)
        {
            using (Packet packet = new Packet((int)ResponsePacket.UseCode))
            {
                packet.Write(discountValue);
                packet.Write(toClient);

                await SendTCPData(toClient, packet, ct);
            }
        }

        public async Task SuccessAsync(int toClient, CancellationToken ct)
        {
            using (Packet packet = new Packet((int)ResponsePacket.Success))
            {
                packet.Write(toClient);

                await SendTCPData(toClient, packet, ct);
            }
        }

        public async Task ErrorAsync(int toClient, string errorMsg, CancellationToken ct)
        {
            using (Packet packet = new Packet((int)ResponsePacket.Error))
            {
                packet.Write(errorMsg);
                packet.Write(toClient);

                await SendTCPData(toClient, packet, ct);
            }
        }

    }
}
