
using DiscountGeneratorService.Interfaces;
using TCPLibrary;

namespace DiscountGeneratorService
{
    public  class Client
    {
        public static int dataBufferSize = 4096;

        public int? _clientId;
        public ServerTCP _tcp;


        public Client(int clientId, IDiscountGenerator discountGenerator)
        {
            _clientId = clientId;
            _tcp = new ServerTCP(clientId, dataBufferSize, discountGenerator);
        }
    }
}
