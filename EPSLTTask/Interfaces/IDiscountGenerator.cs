using DiscountGeneratorService.Handlers;

namespace DiscountGeneratorService.Interfaces
{
    public interface IDiscountGenerator
    {
        public DiscountCodeHandler RequestHandler { get; }
        public void Start(int port);
        public void FileStorageLoop();
        public void CommitActivation(string code, int clientId);
        public void CommitPendingCodes(string[] codes);
        public void CommitPendingCode(string code);
        public Task HandshakeAsync(int toClient, int clientId, CancellationToken ct);
        public Task SuccessAsync(int toClient, CancellationToken ct);
        public Task ErrorAsync(int toClient, string errorMsg, CancellationToken ct);

    }
}
