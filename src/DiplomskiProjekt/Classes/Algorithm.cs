using System;
using System.Collections.Generic;

namespace DiplomskiProjekt.Classes
{
    abstract public class Algorithm
    {
        public abstract void ReInicijaliziraj();
        public abstract void OdradiGeneraciju();

        protected Populacija Populacija;
        public bool Zavrsi;

        public Jedinka NajboljaJedinka
        {
            get { return Populacija.NajboljaJedinka; }
        }
    }

    public class SteadyStateTournament : Algorithm
    {
        private readonly int _tournamentSize;
        public SteadyStateTournament(int velicinaPopulacije, int velicinaTurnira)
        {
            Populacija = new Populacija(velicinaPopulacije);
            _tournamentSize = velicinaTurnira;
        }

        public override void ReInicijaliziraj()
        {
            Populacija = new Populacija(Populacija.BrojJedinki);
        }

        public override void OdradiGeneraciju()
        {
            for (int i = 0; i < Populacija.BrojJedinki; i++)
            {
                var jedinke = new List<int>();
                var najlosijaJedinka = -1;
                var indexNajlosijeJedinke = -1;

                while (jedinke.Count < _tournamentSize)
                {
                    var j = RandomGenerator.GetIntRange(0, Populacija.BrojJedinki);
                    if (jedinke.Contains(j))
                        continue;

                    jedinke.Add(j);
                    if (najlosijaJedinka == -1)
                    {
                        najlosijaJedinka = j;
                        indexNajlosijeJedinke = 0;
                    }
                    else if (Populacija[j].GreskaJedinke > Populacija[najlosijaJedinka].GreskaJedinke)
                    {
                        najlosijaJedinka = j;
                        indexNajlosijeJedinke = jedinke.Count - 1;
                    }
                }
                jedinke.RemoveAt(indexNajlosijeJedinke);

                var djeca = GP.CrossoverOp.Krizaj(Populacija[jedinke[0]], Populacija[jedinke[1]]);
                if (djeca == null)
                    continue;

                var dijete1 = GP.MutationOp.Mutiraj(djeca.Item1);
                var dijete2 = GP.MutationOp.Mutiraj(djeca.Item2);

                GP.EvaluationOp.Evaluiraj(dijete1);
                GP.EvaluationOp.Evaluiraj(dijete2);

                if (dijete1.GreskaJedinke < dijete2.GreskaJedinke)
                    Populacija[najlosijaJedinka] = dijete1;
                else
                    Populacija[najlosijaJedinka] = dijete2;
                Populacija.NajboljaJedinka = Populacija[najlosijaJedinka];
            }
        }
    }
}
