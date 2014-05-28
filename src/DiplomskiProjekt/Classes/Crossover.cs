using System;
using System.Collections.Generic;

namespace DiplomskiProjekt.Classes
{
    public abstract class Crossover
    {
        public double CrossoverFactor;
        public abstract Tuple<Jedinka, Jedinka> Krizaj(Jedinka mamaJedinka, Jedinka tataJedinka);
        /// <summary>
        /// Nadje sve cvorove koji imaju roditelje sa istim brojem djece te se krizanje moze obaviti zamjenom dobivenih cvorova.
        /// </summary>
        protected static void NadjiTockePrekida(Cvor mama, Cvor tata, List<List<Cvor>> cvorovi)
        {
            if (mama.BrojDjece != tata.BrojDjece)
                return;

            for (var i = 0; i < mama.BrojDjece; i++)
            {
                cvorovi.Add(new List<Cvor> { mama.Djeca[i], tata.Djeca[i] });
                NadjiTockePrekida(mama.Djeca[i], tata.Djeca[i], cvorovi);
            }
        }
    }

    public class OnePointCrossover : Crossover
    {
        public override Tuple<Jedinka, Jedinka> Krizaj(Jedinka mamaJedinka, Jedinka tataJedinka)
        {
            var cvorovi = new List<List<Cvor>>();
            var dijete1 = mamaJedinka.Kopiraj();
            var dijete2 = tataJedinka.Kopiraj();

            NadjiTockePrekida(dijete1.Korjen, dijete2.Korjen, cvorovi);

            if (cvorovi.Count == 0)
                return null;

            var par = RandomGenerator.GetRandomElement(cvorovi);
            Cvor.ZamjeniRoditelje(par[0], par[1]);
            return Tuple.Create(dijete1, dijete2);
        }
    }

    public class UniformCrossover : Crossover
    {
        public override Tuple<Jedinka, Jedinka> Krizaj(Jedinka mamaJedinka, Jedinka tataJedinka)
        {
            var cvorovi = new List<List<Cvor>>();
            var dijete1 = mamaJedinka.Kopiraj();
            var dijete2 = tataJedinka.Kopiraj();

            NadjiTockePrekida(dijete1.Korjen, dijete2.Korjen, cvorovi);

            if (cvorovi.Count == 0)
                return null;

            foreach (var par in cvorovi)
            {
                if (RandomGenerator.GetUniform() < 0.5d)
                    continue;
                Cvor.ZamjeniRoditelje(par[0], par[1]);
            }

            return Tuple.Create(dijete1, dijete2);
        }
    }

}
