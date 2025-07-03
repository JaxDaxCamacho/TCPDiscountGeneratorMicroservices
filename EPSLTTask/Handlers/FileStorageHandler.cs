using DiscountGeneratorService.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DiscountGeneratorService.Handlers
{
    public class FileStorageHandler : IFileStorageHandler
    {
        string _path;
        const string pendingCodesDir = "CodesToBeInserted";
        const string pendingActivationsDir = "CodesToBeActivated";
        const string activeCodesText = "ActiveCodes.txt";
        const string usedCodesText = "UsedCodes.txt";
        
        public FileStorageHandler() 
        {
            _path = Directory.GetCurrentDirectory();
            var pendingCodesPath = $"{_path}/{pendingCodesDir}";
            if (!Directory.Exists(pendingCodesPath)) Directory.CreateDirectory(pendingCodesPath);

            var pendingActivationsPath = $"{_path}/{pendingActivationsDir}";
            if (!Directory.Exists(pendingActivationsPath)) Directory.CreateDirectory(pendingActivationsPath);
        }

        public async Task AddCodeFileToBeInserted(string code)
        {
            string fileName = Guid.NewGuid().ToString() + ".txt"; // or any other extension
            string fullPath = Path.Combine(pendingCodesDir, fileName);

            // Write the content to the file
            await File.WriteAllTextAsync(fullPath, code, Encoding.UTF8);

            Console.WriteLine($"Code pending: {code}");
        }

        public async Task AddCodeFileToBeActivated(string code)
        {
            string fileName = Guid.NewGuid().ToString() + ".txt"; // or any other extension
            string fullPath = Path.Combine(pendingActivationsDir, fileName);

            // Write the content to the file
            await File.WriteAllTextAsync(fullPath, code, Encoding.UTF8);

            Console.WriteLine($"Code pending activation: {code}");
        }

        public Dictionary<string, bool> ReadStoredCodesToMemory()
        {
            var readCodes = new Dictionary<string, bool>();
            if (!File.Exists(activeCodesText))
            {
                Console.WriteLine("File not found.");
                return readCodes;
            }

            var pendingcodes = Directory.GetFiles(pendingCodesDir);

            if (pendingcodes.Length < 0)
            {
                InsertCodesIntoStorage(pendingcodes.Length);
            }

            var codes = File.ReadAllLines(activeCodesText);

            foreach (var code in codes)
            {
                readCodes[code] = true;
            }

            var pendingActivations = Directory.GetFiles(pendingActivationsDir);

            if (pendingActivations.Length < 0)
            {
                ProcessCodeActivations(pendingActivations.Length);
            }

            var usedcodes = File.ReadAllLines(usedCodesText);

            foreach (var code in usedcodes)
            {
                if (readCodes.ContainsKey(code))
                {
                    readCodes[code] = false;
                }
            }

            return readCodes;
        }

        public List<string> InsertCodesIntoStorage(int forceCap)
        {
            var codes = Directory.GetFiles(pendingCodesDir);
                        
            if (codes.Length == 0)
            {
                return new List<string>();
            }

            var codesToProcess = new List<string>();

            if (codes.Length < forceCap)
            {
                codesToProcess = codes.Take(codes.Length).ToList();
            }
            else
            {
                codesToProcess = codes.Take(forceCap).ToList();
            }

            using var outputStream = new StreamWriter(activeCodesText, append: true, encoding: Encoding.UTF8);

            foreach (var code in codesToProcess)
            {
                string content = File.ReadAllText(code);

                outputStream.WriteLine(content);
            }

            outputStream.Flush();

            foreach (var file in codesToProcess)
            {
                File.Delete(file);
            }

            Console.WriteLine($"Processed {codesToProcess.Count} codes into {activeCodesText}");
            return codesToProcess;
        }

        public List<string> ProcessCodeActivations(int forceCap)
        {
            var files = Directory.GetFiles(pendingActivationsDir);

            if (files.Length == 0)
            {
                return new List<string>(); ;
            }

            var codesToActivate = new List<string>();

            if (files.Length < forceCap)
            {
                codesToActivate = files.Take(files.Length).ToList();
            }
            else
            {
                codesToActivate = files.Take(forceCap).ToList();
            }

            using var outputStream = new StreamWriter(usedCodesText, append: true, encoding: Encoding.UTF8);

            foreach (var file in files)
            {
                string content = File.ReadAllText(file);

                outputStream.WriteLine(content);
            }

            outputStream.Flush();

            foreach (var file in files)
            {
                File.Delete(file);
            }

            Console.WriteLine($"Processed {codesToActivate.Count} code activations");
            return codesToActivate;
        }

        public void ClearAllData()
        {
            DirectoryInfo di = new DirectoryInfo($"{_path}/{pendingCodesDir}");
            foreach (FileInfo file in di.GetFiles())
            { file.Delete(); }

            DirectoryInfo di2 = new DirectoryInfo($"{_path}/{pendingActivationsDir}");
            foreach (FileInfo file in di2.GetFiles())
            { file.Delete(); }

            File.Delete(activeCodesText);
            File.Delete(usedCodesText);
        }

        public void ForcePath(string path)
        {
            _path = path;
            var pendingCodesPath = $"{_path}/{pendingCodesDir}";
            if (!Directory.Exists(pendingCodesPath)) Directory.CreateDirectory(pendingCodesPath);

            var pendingActivationsPath = $"{_path}/{pendingActivationsDir}";
            if (!Directory.Exists(pendingActivationsPath)) Directory.CreateDirectory(pendingActivationsPath);
        }
    }
}
