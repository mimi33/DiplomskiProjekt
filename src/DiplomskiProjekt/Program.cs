using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            for (var i = 0; i < 24; i++)
            {
                var gp = new GP(i);
                gp.Pokreni();
            }
            Console.ReadKey();
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
