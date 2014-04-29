using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomskiProjekt.Classes
{
    public abstract class Evaluation
    {
        private Podaci _podaci;
        public int FoldNaKojemuSeUci {get { return _podaci.FoldForTesting; }}
        public int BrojVarijabli {get { return _podaci.PodaciZaUcenje.BrojVarijabli; }}

        public void UcitajDataSet(string filepath, bool crx)
        {
            _podaci = new Podaci(filepath, crx);
            // todo - mijenjati dataset ovisno o potrebama i provjeriti dal je dobro
        }

        public IList<double> IzračunajVrijednostiJedinke(Jedinka jedinka, DataSet dataSet)
        {
            var rezultatiJedinke = new List<double>(new double[dataSet.BrojZapisa]);

            Parallel.For(0, dataSet.Varijable.Count,
                i => rezultatiJedinke[i] = jedinka.Izracunaj(dataSet.Varijable[i]));
            return rezultatiJedinke;
        }

        public bool SlijedeciPodaciZaUcenje()
        {
            var gotovo = !_podaci.PromijeniFold();
            return gotovo;
        }

        public double IzracunajGresku(Jedinka jedinka)
        {
            return jedinka.GreskaJedinke = Greska(IzračunajVrijednostiJedinke(jedinka, _podaci.PodaciZaUcenje), _podaci.PodaciZaUcenje.Rezultati);
        }

        public double Validiraj(Jedinka jedinka)
        {
            return Greska(IzračunajVrijednostiJedinke(jedinka, _podaci.PodaciZaProvjeru), _podaci.PodaciZaProvjeru.Rezultati);
        }

        public double Evaluiraj(Jedinka jedinka)
        {
            return jedinka.GreskaJedinke = Greska(IzračunajVrijednostiJedinke(jedinka, _podaci.PodaciZaEvaluaciju), _podaci.PodaciZaEvaluaciju.Rezultati);
        }

        public abstract double Greska(IEnumerable<double> rezultatiJedinke, IEnumerable<double> rezultatiPodataka);
    }

    public class MseEvaluation : Evaluation
    {
        public override double Greska(IEnumerable<double> rezultatiJedinke, IEnumerable<double> rezultatiPodataka)
        {
            return rezultatiJedinke.Zip(rezultatiPodataka, (o, f) => Math.Pow(o - f, 2)).Average();
        }
    }

    public class MaeEvaluation : Evaluation
    {
        public override double Greska(IEnumerable<double> rezultatiJedinke, IEnumerable<double> rezultatiPodataka)
        {
            return rezultatiJedinke.Zip(rezultatiPodataka, (o, f) => Math.Abs(o - f)).Average();
        }
    }

    public class MapeEvaluation : Evaluation
    {
        public override double Greska(IEnumerable<double> rezultatiJedinke, IEnumerable<double> rezultatiPodataka)
        {
            return rezultatiJedinke.Zip(rezultatiPodataka, (o, f) => (1 - Math.Abs(o / f))).Average();
        }
    }
}
