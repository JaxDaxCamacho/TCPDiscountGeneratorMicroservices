
using DiscountGeneratorService.Interfaces;
using System.Text;
using System.Threading;
using TCPLibrary;

namespace DiscountGeneratorService.Handlers
{
    public class DiscountCodeHandler
    {

        readonly IFileStorageHandler _fileStorageHandler;
        readonly IDiscountGenerator _discountGenerator;

        public DiscountCodeHandler(IDiscountGenerator discountgenerator, IFileStorageHandler fileStorageHandler )
        {
            _fileStorageHandler = fileStorageHandler;
            _discountGenerator = discountgenerator;
        }

        public async Task HandleGenerateAsync(int _fromClient, short numberOfCodes, short lengthOfCodes, CancellationToken ct)
        {
            try
            {
                List<Task<string>> tasks = new List<Task<string>>();

                for (var i = 0; i < numberOfCodes; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    tasks.Add(GeneratePendingCode(lengthOfCodes));
                }

                var codesToInsert = await Task.WhenAll(tasks);

                _discountGenerator.CommitPendingCodes(codesToInsert);

                await _discountGenerator.SuccessAsync(_fromClient, ct);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                await _discountGenerator.ErrorAsync(_fromClient,"Operation Canceled", ct);
            }
        }

        async Task<string> GeneratePendingCode(int codeLength)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(codeLength);

            for (int i = 0; i < codeLength; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            var resultStringified = result.ToString();

            var storedCode = await _fileStorageHandler.GetCode(resultStringified);

            if (storedCode != null) 
            {
                return await GeneratePendingCode(codeLength);
            }
            else
            {
                return resultStringified;
            }
        }

        public async Task HandleActivateCodeAsync(int _fromClient, string codeToActivate, CancellationToken ct)
        {
            Console.WriteLine($"Activating code {codeToActivate}");
            var storedCode = await _fileStorageHandler.GetCode(codeToActivate);
            if (storedCode != null)
            {
                if (storedCode.IsActive)
                {
                    //Activation Code Logic
                    _discountGenerator.CommitActivation(storedCode.Code);
                    await _discountGenerator.SuccessAsync(_fromClient, ct);
                }
                else
                {
                    await _discountGenerator.ErrorAsync(_fromClient, "This Code was already used", ct);
                    throw new InvalidOperationException("This Code was already used");
                }
            }
            else
            {
                await _discountGenerator.ErrorAsync(_fromClient, "This Code doesn't exist or is pending activation", ct);
                throw new InvalidOperationException("This Code doesn't exist or is pending activation");
            }
        }
    }

}
