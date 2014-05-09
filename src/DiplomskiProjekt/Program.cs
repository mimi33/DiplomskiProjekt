using System;

namespace DiplomskiProjekt
{
    using Classes;

    class Program
    {
        public static GP GenProg;

        static void Main(string[] args)
        {
            if (args.Length != 1)
                throw new ArgumentException();
            Console.Clear();
            Console.CancelKeyPress += myHandler;

            RandomGenerator.SetSeedFromSystemTime();

            for (var i = 0; i < 24; i++)
            {
                Console.WriteLine("=============================================");
                Console.WriteLine("Sat: " + i);
                GenProg = new GP(i, args[0]);
                GenProg.Pokreni();
            }
        }

        /// <summary>
        /// Služi za ručno postavljanje uvjeta zaustavljanja programa
        /// </summary>
        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            GP.Zavrsi = true;
            args.Cancel = true;
        }
    }
}
