using System;

namespace DiplomskiProjekt
{
    using Classes;

    static class Program
    {
        private static GP _genProg;

        static void Main(string[] args)
        {
            var configFileName = args.Length >= 1 ? args[0] : "Config.xml";
            var sat = args.Length >= 2 ? Convert.ToInt16(args[1]) : -1;

            RandomGenerator.SetSeedFromSystemTime();

            if (sat != -1)
            {
                Console.WriteLine("=============================================");
                Console.WriteLine("Sat: " + sat);
                _genProg = new GP(sat, configFileName);
                _genProg.Pokreni();
            }
            else
            {
                for (var i = 0; i < 24; i++)
                {
                    Console.WriteLine("=============================================");
                    Console.WriteLine("Sat: " + i);
                    _genProg = new GP(i, configFileName);
                    _genProg.Pokreni();
                }
            }
        }
    }
}
