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
    public interface IFileStorageHandler
    {
        const int readCap = 300;

        public Task AddCodeFileToBeInserted(string code);
        public Task AddCodeFileToBeActivated(string code);
        public Dictionary<string, bool> ReadStoredCodesToMemory();
        public List<string> InsertCodesIntoStorage(int forceCap = readCap);
        public List<string> ProcessCodeActivations(int forceCap = readCap);
    }
}
