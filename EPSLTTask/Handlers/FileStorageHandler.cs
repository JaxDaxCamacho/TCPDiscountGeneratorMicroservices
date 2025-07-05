using DiscountGeneratorService.Interfaces;
using System.Text;
using TCPLibrary;
using System.Threading.Tasks;

namespace DiscountGeneratorService.Handlers
{
    public class FileStorageHandler : IFileStorageHandler
    {
        string _path;
        const string ActiveCodesDir = "CodesToBeInserted";
        Dictionary<string, bool> _cachedCodes;

        public FileStorageHandler()
        {
            _path = Directory.GetCurrentDirectory();
            var activeCodesPath = $"{_path}/{ActiveCodesDir}";
            if (!Directory.Exists(activeCodesPath)) Directory.CreateDirectory(activeCodesPath);
            _cachedCodes = new Dictionary<string, bool>();
        }

        public async Task AddCode(string code)
        {
            string fileName = Guid.NewGuid().ToString() + ".txt"; // or any other extension
            string fullPath = Path.Combine(ActiveCodesDir, fileName);

            // Write the content to the file
            await File.WriteAllTextAsync(fullPath, code, Encoding.UTF8);

            Console.WriteLine($"Code activated: {code}");
        }

        public async Task<CodeDTO?> GetCode(string code)
        {
            if (_cachedCodes.TryGetValue(code, out var active)) return new CodeDTO(code, active);

            string[] files = Directory.GetFiles(ActiveCodesDir);
            var _filepath = Path.Combine(ActiveCodesDir, code);
            if (files.Contains(_filepath))
            {
                try
                {
                    string fileContent = await File.ReadAllTextAsync(_filepath);
                    if (fileContent != "true")
                    {
                        _cachedCodes.Add(code, false);
                        return new CodeDTO(code,false);
                    }
                    else
                    {
                        _cachedCodes.Add(code, true);
                        return new CodeDTO(code, true);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine($"Could not read {code} due To IOException");
                }
            }
            return null;
        }

        public async Task SaveCodes(string[] pendingCodes)
        {
            foreach (var code in pendingCodes)
            {
                var _filepath = Path.Combine(ActiveCodesDir, code);
                await File.WriteAllTextAsync(_filepath, "true");
            }
            if(pendingCodes.Length > 0) Console.WriteLine($"Processed {pendingCodes.Count()} codes into {ActiveCodesDir}");
        }

        public bool ActivateCode(string codeToActivate)
        {
            if (_cachedCodes.TryGetValue(codeToActivate, out var active)) _cachedCodes[codeToActivate] = false;

            var files = Directory.GetFiles(ActiveCodesDir);

            var _filepath = Path.Combine(ActiveCodesDir, codeToActivate);

            var fileToActivate = files.FirstOrDefault(_filepath);

            if (fileToActivate != null)
            {
                try
                {
                    string contents = File.ReadAllText(fileToActivate);
                    if (contents != "true")
                    {
                        Console.WriteLine($"{codeToActivate} has already been activated");
                        return false;
                    }
                    else
                    {
                        File.WriteAllText(fileToActivate, "false");
                        return true;
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine($"Could not read {fileToActivate} due To IOException");
                }
            }
            return false;
        }

        public void ClearAllData()
        {
            var files = Directory.GetFiles(ActiveCodesDir);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        public void ForcePath(string path)
        {
            _path = path;
            var pendingCodesPath = $"{_path}/{ActiveCodesDir}";
            if (!Directory.Exists(pendingCodesPath)) Directory.CreateDirectory(pendingCodesPath);
        }
    }
}
