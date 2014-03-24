using System;
using System.Collections.Generic;

namespace DiplomskiProjekt.Classes
{
    public abstract class Crossover
    {
        public abstract Tuple<Jedinka, Jedinka> Krizaj(Jedinka mamaJedinka, Jedinka tataJedinka);
    }

    public class SimpleCrossover : Crossover
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
            var i1 = par[0].Roditelj.IndexOf(par[0]);
            var i2 = par[1].Roditelj.IndexOf(par[1]);

            par[0].Roditelj.Djeca[i1] = par[1];
            par[1].Roditelj.Djeca[i2] = par[0];
            Cvor tmp = par[0].Roditelj;
            par[0].Roditelj = par[1].Roditelj;
            par[1].Roditelj = tmp;

            return Tuple.Create(dijete1, dijete2);
        }

        static void NadjiTockePrekida(Cvor mama, Cvor tata, List<List<Cvor>> cvorovi)
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

    //todo implementirati još jedan operator križanja
}
