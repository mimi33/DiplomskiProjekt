using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace DiplomskiProjekt
{
    using Classes;
    using System.IO;

    static class Program
    {
        private static GP _genProg;

        static void Main(string[] args)
        {
            var configFileName = args.Length == 1 ? args[0] : "Config.xml";

            RandomGenerator.SetSeedFromSystemTime();

            for (var i = 0; i < 24; i++)
            {
                Console.WriteLine("=============================================");
                Console.WriteLine("Sat: " + i);
                _genProg = new GP(i, configFileName);
                //var x = new XmlSerializer(_genProg.GetType());
                //x.Serialize(Console.Out, _genProg);
                _genProg.Pokreni();
            }
        }
    }
}
