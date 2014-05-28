using System.Collections.Generic;
using System.Linq;

namespace DiplomskiProjekt.Classes
{
    public abstract class Algorithm
    {
        public abstract void ResetirajPopulaciju();
        public abstract void OdradiGeneraciju();
        protected int VelicinaTurnira;
        protected Populacija Population;

        public Jedinka NajboljaJedinka
        {
            get { return Population.NajboljaJedinka; }
        }

        /// <summary>
        /// fitness based selection
        /// </summary>
        /// <returns>Index odabrane jedinke</returns>
        protected int NapraviTurnir()
        {
            var turnir = new List<int>();
            while (turnir.Count < VelicinaTurnira)
            {
                var jedinka = RandomGenerator.GetIntRange(0, Population.Count);
                if (!turnir.Contains(jedinka))
                    turnir.Add(jedinka);
            }
            var najboljaGreska = turnir.Min(j => Population[j].GreskaJedinke);
            return turnir.First(j => double.Equals(Population[j].GreskaJedinke, najboljaGreska));
        }
    }

    public class SteadyStateTournamentOneOperator : Algorithm
    {
        public SteadyStateTournamentOneOperator(int velicinaPopulacije, int velicinaTurnira)
        {
            Population = new Populacija(velicinaPopulacije);
            VelicinaTurnira = velicinaTurnira < 3 ? 3 : velicinaTurnira;
        }

        public override void ResetirajPopulaciju()
        {
            Population = new Populacija(Population.Count);
        }

        public override void OdradiGeneraciju()
        {
            for (int i = 0; i < Population.Count; i++)
            {
                var jedinke = new List<int>();
                var najlosijaJedinka = -1;
                var indexNajlosijeJedinke = -1;
                var najboljaJedinka = -1;

                while (jedinke.Count < VelicinaTurnira)
                {
                    var j = RandomGenerator.GetIntRange(0, Population.Count);
                    if (jedinke.Contains(j))
                        continue;

                    jedinke.Add(j);
                    if (najlosijaJedinka == -1)
                    {
                        najlosijaJedinka = j;
                        indexNajlosijeJedinke = 0;
                    }
                    else if (Population[j].GreskaJedinke > Population[najlosijaJedinka].GreskaJedinke)
                    {
                        najlosijaJedinka = j;
                        indexNajlosijeJedinke = jedinke.Count - 1;
                    }
                    if (najboljaJedinka == -1)
                    {
                        najboljaJedinka = j;
                    }
                    else if (Population[j].GreskaJedinke < Population[najboljaJedinka].GreskaJedinke)
                    {
                        najboljaJedinka = j;
                    }
                }
                jedinke.RemoveAt(indexNajlosijeJedinke);

                Jedinka dijete;
                if (RandomGenerator.GetUniform() < GP.CrossoverOp.CrossoverFactor)
                {
                    var djeca = GP.CrossoverOp.Krizaj(Population[jedinke[0]], Population[jedinke[1]]);
                    if (djeca == null)
                        continue;
                    dijete = djeca.Item1.GreskaJedinke < djeca.Item2.GreskaJedinke ? djeca.Item1 : djeca.Item2;
                }
                else
                {
                    dijete = GP.MutationOp.Mutiraj(Population[najboljaJedinka].Kopiraj());
                }

                GP.EvaluationOp.IzracunajGresku(dijete);
                Population[najlosijaJedinka] = dijete;
            }
        }
    }

    public class SteadyStateTournamentTwoOperators : Algorithm
    {
        public SteadyStateTournamentTwoOperators(int velicinaPopulacije, int velicinaTurnira)
        {
            Population = new Populacija(velicinaPopulacije);
            VelicinaTurnira = velicinaTurnira < 3 ? 3 : velicinaTurnira;
        }

        public override void ResetirajPopulaciju()
        {
            Population = new Populacija(Population.Count);
        }

        public override void OdradiGeneraciju()
        {
            for (int i = 0; i < Population.Count; i++)
            {
                var jedinke = new List<int>();
                var najlosijaJedinka = -1;
                var najboljaJedinka = -1;
                var drugaNajboljaJedinka = -1;

                while (jedinke.Count < VelicinaTurnira)
                {
                    var j = RandomGenerator.GetIntRange(0, Population.Count);
                    if (jedinke.Contains(j))
                        continue;

                    jedinke.Add(j);
                    if (najlosijaJedinka == -1)
                    {
                        najlosijaJedinka = j;
                    }
                    else if (Population[j].GreskaJedinke > Population[najlosijaJedinka].GreskaJedinke)
                    {
                        najlosijaJedinka = j;
                    }

                    if (najboljaJedinka == -1)
                    {
                        najboljaJedinka = j;
                    }
                    else if (Population[j].GreskaJedinke < Population[najboljaJedinka].GreskaJedinke)
                    {
                        drugaNajboljaJedinka = najboljaJedinka;
                        najboljaJedinka = j;
                    }
                    else if (drugaNajboljaJedinka == -1)
                    {
                        drugaNajboljaJedinka = j;
                    }
                    else if (Population[j].GreskaJedinke < Population[drugaNajboljaJedinka].GreskaJedinke)
                    {
                        drugaNajboljaJedinka = j;
                    }
                }

                var djeca = GP.CrossoverOp.Krizaj(Population[najboljaJedinka], Population[drugaNajboljaJedinka]);
                if (djeca == null)
                    continue;

                var dijete1 = GP.MutationOp.Mutiraj(djeca.Item1);
                var dijete2 = GP.MutationOp.Mutiraj(djeca.Item2);

                GP.EvaluationOp.IzracunajGresku(dijete1);
                GP.EvaluationOp.IzracunajGresku(dijete2);

                if (dijete1.GreskaJedinke < dijete2.GreskaJedinke)
                    Population[najlosijaJedinka] = dijete1;
                else
                    Population[najlosijaJedinka] = dijete2;
            }
        }
    }

    public class GenerationalTournamentOneOperator : Algorithm
    {
        public GenerationalTournamentOneOperator(int velicinaPopulacije, int velicinaTurnira)
        {
            VelicinaTurnira = velicinaTurnira;
            Population = new Populacija(velicinaPopulacije);
        }

        public override void OdradiGeneraciju()
        {
            var staraPopulacija = Population;
            var novaPopulacija = new Populacija {staraPopulacija.NajboljaJedinka.Kopiraj()};

            while (novaPopulacija.Count < staraPopulacija.Count)
            {
                var i = NapraviTurnir();
                var rand = RandomGenerator.GetUniform();
                if (rand < GP.CrossoverOp.CrossoverFactor) // krizaj
                {
                    var j = NapraviTurnir();
                    var djeca = GP.CrossoverOp.Krizaj(staraPopulacija[i], staraPopulacija[j]);
                    if (djeca == null)
                        continue;
                    novaPopulacija.Add(djeca.Item1);
                    if (novaPopulacija.Count < staraPopulacija.Count)
                        novaPopulacija.Add(djeca.Item2);
                }
                else // mutiraj
                {
                    novaPopulacija.Add(GP.MutationOp.Mutiraj(staraPopulacija[i].Kopiraj()));
                }
            }

            foreach (var jedinka in novaPopulacija)
            {
                GP.EvaluationOp.IzracunajGresku(jedinka);
            }

            Population = novaPopulacija;
            Population.IzracunajNajboljuJedinku();
        }

        public override void ResetirajPopulaciju()
        {
            Population = new Populacija(Population.Count);
        }
    }

    public class GenerationalTournamentTwoOperators : Algorithm
    {
        public GenerationalTournamentTwoOperators(int velicinaPopulacije, int velicinaTurnira)
        {
            VelicinaTurnira = velicinaTurnira;
            Population = new Populacija(velicinaPopulacije);
        }

        public override void OdradiGeneraciju()
        {
            var staraPopulacija = Population;
            var novaPopulacija = new Populacija();

            while (novaPopulacija.Count < staraPopulacija.Count - 1)
            {
                var i = NapraviTurnir();
                if (RandomGenerator.GetUniform() < GP.CrossoverOp.CrossoverFactor)
                {
                    var j = NapraviTurnir();

                    var djeca = GP.CrossoverOp.Krizaj(staraPopulacija[i], staraPopulacija[j]);
                    if (djeca == null)
                        continue;
                    novaPopulacija.Add(djeca.Item1);
                    if (novaPopulacija.Count < staraPopulacija.Count)
                        novaPopulacija.Add(djeca.Item2);
                }
                else
                {
                    novaPopulacija.Add(staraPopulacija[i].Kopiraj());
                }
            }

            foreach (var jedinka in novaPopulacija)
            {
                GP.MutationOp.Mutiraj(jedinka);
                GP.EvaluationOp.IzracunajGresku(jedinka);
            }
            // elitilizam
            var theBest = staraPopulacija.NajboljaJedinka.Kopiraj();
            GP.EvaluationOp.IzracunajGresku(theBest);
            novaPopulacija.Add(theBest);

            Population = novaPopulacija;
            Population.IzracunajNajboljuJedinku();
        }

        public override void ResetirajPopulaciju()
        {
            Population = new Populacija(Population.Count);
        }
    }
}
