
namespace DiscountGeneratorService
{
    class Program
    {
        static bool isRunning = false;
        static int Ticks_Per_Sec = 10;
        static int MS_Per_Tic = 100;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Discount Code Generator Service");
            Console.Title = "DiscountCode Generator";
            EPSDiscountGenerator.Start(19888);
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            isRunning = true;

            mainThread.Start();
        }

        static void MainThread()
        {
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {

                    EPSDiscountGenerator.Loop();
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
