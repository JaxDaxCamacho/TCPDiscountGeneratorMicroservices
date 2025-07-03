using DiscountGeneratorClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCPLibrary
{
    public class ClientTCP
    {
        public TcpClient socket;

        public int _id;

        NetworkStream _stream;
        byte[] _receiveBuffer;

        Packet _receivedData;

        int _dataBufferSize;

        public ClientTCP(int id, int dataBufferSize)
        {
            _id = id;
            _dataBufferSize = dataBufferSize;
        }

        public void UpdateId(int id)
        {
            _id = id;
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

        public void Connect(IPAddress ip, int port)
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = _dataBufferSize,
                SendBufferSize = _dataBufferSize
            };

            _receiveBuffer = new byte[_dataBufferSize];
            socket.BeginConnect(ip, port, ConnectCallback, socket);
        }

        public void Disconnect()
        {
            socket.Close();
        }

        void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                return;
            }

            _stream = socket.GetStream();

            _receivedData = new Packet();

            _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);
        }

        void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = _stream.EndRead(_result);

                byte[] _data = new byte[_byteLength];
                Array.Copy(_receiveBuffer, _data, _byteLength);

                _receivedData.Reset(HandleData(_data));
                _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                //Handle it
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            _receivedData.SetBytes(_data);

            if (_receivedData.UnreadLength() >= 4)
            {
                _packetLength = _receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true; 
                }
            }

            while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength())
            {
                byte[] _packetBytes = _receivedData.ReadBytes(_packetLength);
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Program.packetHandlers[_packetId](_packet); 
                }

                _packetLength = 0; 
                if (_receivedData.UnreadLength() >= 4)
                {
                    _packetLength = _receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true; 
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }
    }
}
