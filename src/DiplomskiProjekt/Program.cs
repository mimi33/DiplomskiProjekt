using System;

namespace DiplomskiProjekt
{
    using Classes;

    class Program
    {
        public static GP GenetskoProgramiranje;

        static void Main()
        {
            Console.Clear();
            Console.CancelKeyPress += myHandler;

            RandomGenerator.SetSeedFromSystemTime();

            GenetskoProgramiranje = new GP();

            GenetskoProgramiranje.Pokreni();

        }

        /// <summary>
        /// Služi za ručno postavljanje uvjeta zaustavljanja programa
        /// </summary>
        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            //GenetskoProgramiranje.Populacija.Zavrsi = true;
            args.Cancel = true;
        }
    }
}
