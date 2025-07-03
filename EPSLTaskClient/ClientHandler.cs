
using TCPLibrary;

namespace DiscountGeneratorClient
{
    internal class ClientHandler
    {
        public static async Task HandshakeAsync(Packet packet)
        {
            var clientIdCheck = packet.ReadInt();
            Console.WriteLine($"Handshake complete. ID given {clientIdCheck}");
        }

        public static async Task GenerateAsync(Packet packet)
        {
        }

        public static async Task UseCodeAsync(Packet packet)
        {
        }

        public static async Task ErrorAsync(Packet packet)
        {
            var errorMsg = packet.ReadString();
            Console.WriteLine($"Error! {errorMsg}");
        }

        public static async Task SucessAsync(Packet packet)
        {
            Console.WriteLine($"Sucess! Operation Complete");
        }
    }

}
