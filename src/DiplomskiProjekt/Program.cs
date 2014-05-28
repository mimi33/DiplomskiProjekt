using System;

namespace DiplomskiProjekt
{
    using Classes;

    static class Program
    {
        private static GP _genProg;

        static void Main(string[] args)
        {
            var configFileName = args.Length == 1 ? args[0] : "Config.xml";
            Console.Clear();
            Console.CancelKeyPress += myHandler;

            RandomGenerator.SetSeedFromSystemTime();

            for (var i = 0; i < 24; i++)
            {
                Console.WriteLine("=============================================");
                Console.WriteLine("Sat: " + i);
                _genProg = new GP(i, configFileName);
                _genProg.Pokreni();
            }
        }

        /// <summary>
        /// Služi za ručno postavljanje uvjeta zaustavljanja programa
        /// </summary>
        static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            GP.Zavrsi = true;
            args.Cancel = true;
        }
    }
}
