using DiscountGeneratorService.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPLibrary;

namespace DiscountGeneratorService.Interfaces
{
    public interface IDiscountGenerator
    {
        public DiscountCodeHandler RequestHandler { get; }
        public void Start(int port);
        public Dictionary<string, bool> GetCodesInMemory();
        public void UpdateCodesInMemory(string code, bool active);
        public void AddCodeToMemory(string code);
        public void Loop();
        public Task HandshakeAsync(int toClient, int clientId, CancellationToken ct);
        public Task GenerateAsync(int toClient, string msg, CancellationToken ct);
        public Task UseCodeAsync(int toClient, short discountValue, CancellationToken ct);
        public Task SuccessAsync(int toClient, CancellationToken ct);
        public Task ErrorAsync(int toClient, string errorMsg, CancellationToken ct);

    }
}
