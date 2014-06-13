using System;
using System.Collections.Generic;
using System.Linq;

namespace DiplomskiProjekt.Classes
{
    [Serializable]
    public abstract class Mutation
    {
        public bool ConstantMutationEnabled;
        public double MutationFactor;
        public abstract Jedinka Mutiraj(Jedinka jedinka);

        protected static void MutirajKonstantu(Cvor cvor)
        {
            if (cvor.Tip != TipCvora.Konstanta)
                return;
            cvor.Vrijednost += RandomGenerator.GetNormal(0, 5);
            //todo odrediti koliko je raspršenje i odrediti vjerojatnost i čitanje iz configa?
        }
    }

    [Serializable]
    public class PointMutation : Mutation
    {
        public override Jedinka Mutiraj(Jedinka jedinka)
        {
            MutirajCvor(jedinka.Korjen);
            jedinka.FiksirajKonstante();
            return jedinka;
        }

        void MutirajCvor(Cvor cvor)
        {
            if (RandomGenerator.GetUniform() < MutationFactor)
            {
                if (ConstantMutationEnabled && cvor.Tip == TipCvora.Konstanta && RandomGenerator.GetUniform() < 0.5d )
                    MutirajKonstantu(cvor);
                else
                    cvor.ZamjeniSaIstimTipom();
            }
            if (cvor.Djeca == null) 
                return;
            foreach (var c in cvor.Djeca)
                MutirajCvor(c);
        }
    }

    [Serializable()]
    public class HoistMutation : Mutation
    {
        public override Jedinka Mutiraj(Jedinka jedinka)
        {
            if (RandomGenerator.GetUniform() > MutationFactor) return jedinka;

            var indexCvora = RandomGenerator.GetIntRange(1, jedinka.BrojCvorova());
            var cvorovi = new List<Cvor> {jedinka.Korjen};
            Cvor noviKorjen = null;
            for (int i = 0; cvorovi.Count > 0; i++)
            {
                noviKorjen = cvorovi.ElementAt(0);
                if (i == indexCvora)
                    break;
                cvorovi.RemoveAt(0);
                if (noviKorjen.BrojDjece != 0)
                    cvorovi.AddRange(noviKorjen.Djeca);
            }
            jedinka.Korjen = noviKorjen;
            return jedinka;
        }
    }
}
