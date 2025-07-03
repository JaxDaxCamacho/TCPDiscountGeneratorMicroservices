using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using TCPLibrary;

namespace DiscountGeneratorClient
{
    static class Program
    {
        static bool isRunning = false;
        static int Ticks_Per_Sec = 10;
        static int MS_Per_Tic = 100;
        static public ClientTCP? tcp;
        static int dataBufferSize = 4096;
        public static Dictionary<int, Func<Packet, Task>> packetHandlers;

        static void Main(string[] args)
        {
            var commands = new Dictionary<string, Action>
            {
                { "generate", GenerateRequest },
                { "activate", ActivateRequest },
                { "exit", DisconnectGracefully }
            };
            Console.WriteLine("Starting Discount Code Generator Client");
            Console.Title = "DiscountCode Generator Client";

            InitializeClientPackets();

            Console.WriteLine("Handshake with Server!");

            tcp = new ClientTCP(0,dataBufferSize);
            tcp.Connect(IPAddress.Parse("127.0.0.1"), 19888);

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            isRunning = true;

            Console.WriteLine("Available commands:");
            foreach (var cmd in commands.Keys)
            {
                Console.WriteLine($"- {cmd}");
            }

            mainThread.Start();
        }

        static void GenerateRequest()
        {
            Console.Write("Enter the number of Discount Codes to Generate: ");
            if (!short.TryParse(Console.ReadLine(), out var a))
            {
                Console.WriteLine("Invalid number.");
                return;
            }

            Console.Write("Enter the length of the codes (7 or 8): ");
            if (!short.TryParse(Console.ReadLine(), out var b))
            {
                Console.WriteLine("Invalid number.");
                return;
            }

            if (b != 8 && b != 7)
            {
                Console.WriteLine("Needs to be either 7 or 8");
                return;
            }

            Console.WriteLine($"Generating {a} codes of {b} Length");
            using (Packet _packet = new Packet((int)RequestPacket.Generate))
            {
                _packet.Write(a);
                _packet.Write(b);
                _packet.WriteLength();
                tcp.SendData(_packet);
            }
        }

        static void ActivateRequest()
        {
            Console.Write("Enter your Code to activate: ");
            var code = Console.ReadLine();

            Console.WriteLine($"Activate Code {code}");
            using (Packet _packet = new Packet((int)RequestPacket.UseCode))
            {
                _packet.Write(code);
                _packet.WriteLength();
                tcp.SendData(_packet);
            }
        }

        static void DisconnectGracefully()
        {
            tcp.Disconnect();
            Console.Write("Disconnected From Service...");
        }

        static void InitializeClientPackets()
        {
            packetHandlers = new Dictionary<int, Func<Packet, Task>>
            {
                { (int)ResponsePacket.Handshake, ClientHandler.HandshakeAsync },
                { (int)ResponsePacket.Generate, ClientHandler.GenerateAsync },
                { (int)ResponsePacket.UseCode, ClientHandler.UseCodeAsync },
                { (int)ResponsePacket.Success, ClientHandler.SucessAsync },
                { (int)ResponsePacket.Error, ClientHandler.ErrorAsync }
            };

            Console.WriteLine($"Client packets initialized...");
        }

        static void MainThread()
        {
            DateTime nextLoop = DateTime.Now;

            var commands = new Dictionary<string, Action>
            {
                { "generate", GenerateRequest },
                { "activate", ActivateRequest },
                { "exit", DisconnectGracefully }
            };

            while (isRunning)
            {
                Console.Write("\nEnter a command: ");
                var input = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Please enter a command.");
                    continue;
                }

                if (commands.TryGetValue(input, out var action))
                {
                    action.Invoke();
                }
                else
                {
                    Console.WriteLine($"Unknown command: '{input}'");
                }
                while (nextLoop < DateTime.Now)
                {
                    nextLoop = nextLoop.AddMilliseconds(MS_Per_Tic);

                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
