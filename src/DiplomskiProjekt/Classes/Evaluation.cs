using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomskiProjekt.Classes
{
    public abstract class Evaluation
    {
        private Podaci _podaci;
        public DataSet DataSet;

        public void UcitajDataSet(string filepath)
        {
            Podaci.BrojPrethodnihMjerenja = 7;
            Podaci.SatniInterval = 24;

            _podaci = new Podaci(filepath);
            DataSet = _podaci.PodaciZaUcenje;
            // todo - maknuti hardcoded vrijednosti
            // todo - mijenjati dataset ovisno o potrebama
        }

        public IList<double> IzračunajVrijednostiJedinke(Jedinka jedinka)
        {
            var rezultatiJedinke = new List<double>(new double[_podaci.PodaciZaUcenje.BrojZapisa]);

            Parallel.For(0, _podaci.PodaciZaUcenje.Varijable.Count,
                i => rezultatiJedinke[i] = jedinka.Izracunaj(_podaci.PodaciZaUcenje.Varijable[i]));
            return rezultatiJedinke;
        }

        public bool SlijedeciPodaciZaUcenje()
        {
            var gotovo = !_podaci.PromijeniFold();
            DataSet = _podaci.PodaciZaUcenje;
            return gotovo;
        }

        public void VratiUPodatkeZaUcenje()
        {
            DataSet = _podaci.PodaciZaUcenje;
        }

        public void PromijeniUPodatkeZaProvjeru()
        {
            DataSet = _podaci.PodaciZaProvjeru;
        }

        public void PromijeniUPodatkeZaEvaluaciju()
        {
            DataSet = _podaci.PodaciZaEvaluaciju;
        }

        public int FoldNaKojemSeUci()
        {
            return _podaci.FoldForTesting;
        }

        public abstract double Evaluiraj(Jedinka jedinka);


    }

    public class MseEvaluation : Evaluation
    {
        public override double Evaluiraj(Jedinka jedinka)
        {
            // todo - koliko ima smisla spremati to u gresku jedinke ako imamo učenje,evaluaciju,testiranje...
            return jedinka.GreskaJedinke = IzračunajVrijednostiJedinke(jedinka).Zip(DataSet.Rezultati, (o, f) => Math.Pow(o - f, 2)).Average();
        }
    }

    public class MaeEvaluation : Evaluation
    {
        public override double Evaluiraj(Jedinka jedinka)
        {
            // todo - koliko ima smisla spremati to u gresku jedinke ako imamo učenje,evaluaciju,testiranje...
            return jedinka.GreskaJedinke = IzračunajVrijednostiJedinke(jedinka).Zip(DataSet.Rezultati, (o, f) => Math.Abs(o - f)).Average();
        }
    }

    public class MapeEvaluation : Evaluation
    {
        public override double Evaluiraj(Jedinka jedinka)
        {
            // todo - koliko ima smisla spremati to u gresku jedinke ako imamo učenje,evaluaciju,testiranje...
            return jedinka.GreskaJedinke = IzračunajVrijednostiJedinke(jedinka).Zip(DataSet.Rezultati, (o, f) => (1 - Math.Abs(o / f))).Average();
        }
    }
}
