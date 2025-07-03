using DiscountGeneratorService.Handlers;
using DiscountGeneratorService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountGeneratorService
{
    public class App
    {
        private readonly IFileStorageHandler _fileStorageHandler;
        private readonly IDiscountGenerator _discountgenerator;

        public App(IDiscountGenerator discountgenerator, IFileStorageHandler fileStorageHandler)
        {
            _fileStorageHandler = fileStorageHandler;
            _discountgenerator = discountgenerator;
        }

        public void Run(int port)
        {
            _discountgenerator.Start(port);
        }
    }
}
