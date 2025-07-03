using DiscountGeneratorService.Interfaces;
using System.Net.Sockets;
using TCPLibrary;

namespace DiscountGeneratorService
{
    public class ServerTCP
    {
        public TcpClient socket;

        readonly int _id;

        NetworkStream _stream;
        byte[] _receiveBuffer;

        Packet _receivedData;

        int _dataBufferSize;

        const int MaxCodes = 2000;


        readonly IDiscountGenerator _discountGenerator;

        public ServerTCP(int id, int dataBufferSize, IDiscountGenerator discountGenerator)
        {
            _id = id;
            _dataBufferSize = dataBufferSize;
            _discountGenerator = discountGenerator;
        }

        public async Task ConnectAsync(TcpClient _socket,int clientId, CancellationToken ct)
        {
            socket = _socket;
            socket.ReceiveBufferSize = _dataBufferSize;
            socket.SendBufferSize = _dataBufferSize;

            _stream = socket.GetStream();

            _receivedData = new Packet();
            _receiveBuffer = new byte[_dataBufferSize];

            _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);

            await _discountGenerator.HandshakeAsync(_id, clientId, ct);
        }

        public void Disconnect()
        {
            socket.Close();
            _stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            socket = null;
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Sending data to client {_id} via TCP: {ex.Message}");
            }
        }

        public async Task SendDataAsync(Packet packet, CancellationToken ct)
        {
            try
            {
                if (socket != null)
                {
                    await _stream.WriteAsync(packet.ToArray(), 0, packet.Length(), ct);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Sending data to client {_id} via TCP: {ex.Message}");
            }
        }

        void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = _stream.EndRead(_result);

                if (_byteLength <= 0)
                {
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(_receiveBuffer, _data, _byteLength);

                _receivedData.Reset(HandleData(_data));
                _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex}, from user {_id}");
            }
        }

        bool HandleData(byte[] data)
        {
            var cts = new CancellationTokenSource();
            int packetLength = 0;

            _receivedData.SetBytes(data);

            if (_receivedData.UnreadLength() >= 4)
            {
                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
            {
                byte[] packetBytes = _receivedData.ReadBytes(packetLength);
                
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    switch (packetId)
                    {
                        case (int)RequestPacket.Generate:
                            var numberOfCodes = packet.ReadShort();
                            if (numberOfCodes > MaxCodes)
                            {
                                _discountGenerator.ErrorAsync(_id, "Max codes to be generated is 2000", cts.Token);
                                break;
                            }

                            var lengthOfCodes = packet.ReadShort();
                            if (lengthOfCodes != 7 && lengthOfCodes != 8)
                            {
                                _discountGenerator.ErrorAsync(_id, "Length of codes can only be 7 or 8", cts.Token);
                                break;
                            }
                            _discountGenerator.RequestHandler.HandleGenerateAsync(_id, numberOfCodes, lengthOfCodes, cts.Token);
                            break;

                        case (int)RequestPacket.UseCode:
                            var codeToActivate = packet.ReadString();
                            if (string.IsNullOrEmpty(codeToActivate) || codeToActivate.Length > 8 || codeToActivate.Length < 7)
                            {
                                _discountGenerator.ErrorAsync(_id, $"{codeToActivate} is not a valid code", cts.Token);
                                break;
                            }
                            _discountGenerator.RequestHandler.HandleUseCodeAsync(_id, codeToActivate, cts.Token);
                            break;
                    }
                }

                packetLength = 0;
                if (_receivedData.UnreadLength() >= 4)
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }
    }
}
