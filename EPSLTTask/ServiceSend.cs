using TCPLibrary;

namespace DiscountGeneratorService
{
    internal class ServiceSend
    {
        static async Task SendTCPData(int toClient, Packet packet, CancellationToken ct)
        {
            packet.WriteLength();
            await EPSDiscountGenerator.clients[toClient]._tcp.SendDataAsync(packet, ct);
        }

        public static async Task HandshakeAsync(int toClient, int clientId, CancellationToken ct)
        {
            using (Packet packet = new Packet((int)ResponsePacket.Handshake))
            {
                packet.Write(clientId);
                packet.Write(toClient);

                await SendTCPData(toClient, packet, ct);
            }
        }

        public static async Task GenerateAsync(int toClient, string msg, CancellationToken ct)
        {
            using (Packet packet = new Packet((int)ResponsePacket.Generate))
            {
                packet.Write(toClient);

                await SendTCPData(toClient, packet, ct);
            }
        }

        public static async Task UseCodeAsync(int toClient, short discountValue, CancellationToken ct)
        {
            using (Packet packet = new Packet((int)ResponsePacket.UseCode))
            {
                packet.Write(discountValue);
                packet.Write(toClient);

                await SendTCPData(toClient, packet, ct);
            }
        }

        public static async Task SuccessAsync(int toClient, CancellationToken ct)
        {
            using (Packet packet = new Packet((int)ResponsePacket.Success))
            {
                packet.Write(toClient);

                await SendTCPData(toClient, packet, ct);
            }
        }

        public static async Task ErrorAsync(int toClient, string errorMsg, CancellationToken ct)
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
