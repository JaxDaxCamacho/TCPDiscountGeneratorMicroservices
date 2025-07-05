using DiscountGeneratorService.Handlers;
using DiscountGeneratorService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DiscountGeneratorService
{
    class Program
    {
        static bool isRunning = false;
        static int MS_Per_Tic = 250;
        static int Port = 19888;
        static IDiscountGenerator _discounteredGenerator;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Discount Code Generator Service");
            Console.Title = "DiscountCode Generator";

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var app = serviceProvider.GetRequiredService<App>();
            app.Run(Port);

            _discounteredGenerator = serviceProvider.GetRequiredService<IDiscountGenerator>();

            Thread storageThread = new Thread(new ThreadStart(StorageThread));
            isRunning = true;

            storageThread.Start();
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFileStorageHandler, FileStorageHandler>();
            services.AddSingleton<IDiscountGenerator, EPSDiscountGenerator>();

            services.AddTransient<App>();
        }

        static void StorageThread()
        {
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {

                    _discounteredGenerator.FileStorageLoop();
                    nextLoop = nextLoop.AddMilliseconds(MS_Per_Tic);

                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
