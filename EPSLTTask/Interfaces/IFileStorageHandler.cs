using TCPLibrary;

namespace DiscountGeneratorService.Interfaces
{
    public interface IFileStorageHandler
    {
        const int readCap = 300;

        public Task AddCode(string code);
        public Task<CodeDTO?> GetCode(string code);
        public Task SaveCodes(string[] pendingCodes);
        public bool ActivateCode(string CodeToActivate);
    }
}
