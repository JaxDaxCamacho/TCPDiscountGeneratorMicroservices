
using DiscountGeneratorService.Interfaces;
using System.Text;
using System.Threading;
using TCPLibrary;

namespace DiscountGeneratorService.Handlers
{
    public class DiscountCodeHandler
    {
        const int MaxCodes = 2000;

        readonly IFileStorageHandler _fileStorageHandler;
        readonly IDiscountGenerator _discountGenerator;

        public DiscountCodeHandler(IDiscountGenerator discountgenerator, IFileStorageHandler fileStorageHandler )
        {
            _fileStorageHandler = fileStorageHandler;
            _discountGenerator = discountgenerator;
        }

        public async Task GenerateAsync(int _fromClient, Packet packet, CancellationToken ct)
        {
            try
            {
                var numberOfCodes = packet.ReadShort();
                if (numberOfCodes > MaxCodes)
                {
                    //Send Error Message
                    return;
                }

                var lengthOfCodes = packet.ReadShort();
                if (lengthOfCodes != 7 && lengthOfCodes != 8)
                {
                    //Send Error Message
                    return;
                }

                List<Task<string>> tasks = new List<Task<string>>();

                for (var i = 0; i < numberOfCodes; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    tasks.Add(GenerateCode(lengthOfCodes));
                }

                await Task.WhenAll(tasks);

                await _discountGenerator.SuccessAsync(_fromClient, ct);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                await _discountGenerator.ErrorAsync(_fromClient,"Operation Canceled", ct);
            }
        }

        async Task<string> GenerateCode(int codeLength)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(codeLength);

            for (int i = 0; i < codeLength; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            var resultStringified = result.ToString();

            if(_discountGenerator.GetCodesInMemory().TryGetValue(resultStringified, out var active))
            {
               return await GenerateCode(codeLength); 
            }
            else
            {
                await _fileStorageHandler.AddCodeFileToBeInserted(resultStringified);
                _discountGenerator.AddCodeToMemory(resultStringified);

                return resultStringified;
            }
        }

        public async Task UseCodeAsync(int _fromClient, Packet packet, CancellationToken ct)
        {
            var codeToActivate = packet.ReadString();
            Console.WriteLine($"Activating code {codeToActivate}");
            if (_discountGenerator.GetCodesInMemory().TryGetValue(codeToActivate, out var isActive))
            {
                if(isActive)
                {
                    //Activation Code Logic
                    await _fileStorageHandler.AddCodeFileToBeActivated(codeToActivate);
                    _discountGenerator.UpdateCodesInMemory(codeToActivate, true);
                    await _discountGenerator.SuccessAsync(_fromClient, ct);
                }
                else
                {
                    await _discountGenerator.ErrorAsync(_fromClient, "This Code was already used", ct);
                }
            }
            else
            {
                await _discountGenerator.ErrorAsync(_fromClient, "This Code doesn't exist or is pending activation", ct);
            }
        }
    }

}
