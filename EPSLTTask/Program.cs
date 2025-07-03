using DiscountGeneratorService.Handlers;
using DiscountGeneratorService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DiscountGeneratorService
{
    class Program
    {
        static bool isRunning = false;
        static int Ticks_Per_Sec = 10;
        static int MS_Per_Tic = 100;
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

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            isRunning = true;

            mainThread.Start();
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFileStorageHandler, FileStorageHandler>();
            services.AddTransient<IDiscountGenerator, EPSDiscountGenerator>();

            services.AddTransient<App>();
        }

        static void MainThread()
        {
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {

                    _discounteredGenerator.Loop();
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
