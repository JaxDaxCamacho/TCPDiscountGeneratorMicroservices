using DiscountGeneratorService.Handlers;
using DiscountGeneratorService.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Threading.Tasks;
using TCPLibrary;

namespace DiscountGeneratorService
{
    public class EPSDiscountGenerator : IDiscountGenerator
    {
        int Port;

        public TcpListener TcpListener;

        public delegate void PacketHandler(int fromClient, Packet packet);

        public Dictionary<int, Client> Clients;

        readonly IFileStorageHandler _fileStorageHandler;

        int CallTimeout = 360;

        int PageSize = 300;

        int ClientConnectCounter = 0;

        public List<string> PendingCodes = new List<string>();
        public Dictionary<string,int> PendingActivations = new Dictionary<string, int>();

        public DiscountCodeHandler RequestHandler { get; }

        public EPSDiscountGenerator(IFileStorageHandler fileStorageHandler)
        {
            _fileStorageHandler = fileStorageHandler;
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


            Console.WriteLine($"EPSDiscountGenerator Service hosted on Port {Port}.");
        }

        public void FileStorageLoop()
        {
            var cts = new CancellationTokenSource();
            if (PendingCodes.Count > 0)
            {
                var pageSize = PendingCodes.Count > PageSize ? PageSize : PendingCodes.Count;
                var codesToAddPage = PendingCodes.TakeLast(pageSize);
                _fileStorageHandler.SaveCodes(codesToAddPage.ToArray());
                PendingCodes = PendingCodes.Except(codesToAddPage).ToList();
            }

            if (PendingActivations.Count > 0)
            {
                var pageSize = PendingActivations.Count > PageSize ? PageSize : PendingActivations.Count;
                var codesToActivatePage = PendingActivations.TakeLast(pageSize);
                foreach (var codeClientPair in codesToActivatePage)
                {
                    var success = _fileStorageHandler.ActivateCode(codeClientPair.Key);
                    if (success) SuccessAsync(codeClientPair.Value, cts.Token).GetAwaiter().GetResult();
                    else ErrorAsync(codeClientPair.Value, "Code is no longer valid", cts.Token).GetAwaiter().GetResult();
                }
                PendingActivations = PendingActivations.Except(codesToActivatePage).ToDictionary();
            }
        }

        public void CommitPendingCode(string code)
        {
            Console.WriteLine($"pending code {code}");
            PendingCodes.Add(code);
        }

        public void CommitActivation(string code, int clientId)
        {
            Console.WriteLine($"atempting activation on {code} by client {clientId}");
            PendingActivations.Add(code, clientId);
        }

        public void CommitPendingCodes(string[] codes)
        {
            Console.WriteLine($"inserted code batch: {string.Join("\n",codes)}");
            PendingCodes.AddRange(codes);
        }

        void TcpConnectCallback(IAsyncResult result)
        {
            TcpClient client = TcpListener.EndAcceptTcpClient(result);
            TcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);  
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            ClientConnectCounter++;
            Clients.Add(ClientConnectCounter, new Client(ClientConnectCounter, this));

            var cts = new CancellationTokenSource();

            var task = Clients[ClientConnectCounter]._tcp.ConnectAsync(client, ClientConnectCounter, cts.Token);

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
