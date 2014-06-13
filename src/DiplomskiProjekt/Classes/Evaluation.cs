using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomskiProjekt.Classes
{
    public abstract class Evaluation
    {
        private Podaci _podaci;
        public int FoldNaKojemuSeUci {get { return _podaci.FoldForTesting; }}
        public int BrojVarijabli {get { return _podaci.PodaciZaUcenje.BrojVarijabli; }}
        public bool IsCrossValidation;
        private bool _rotateFolds;

        /// <summary>
        /// Ucitava podatke iz datoteke kada nema krosvalidacije 
        /// </summary>
        /// <param name="filepath">file path do datoteke sa podacima</param>
        public void UcitajDataSet(string filepath)
        {
            _podaci = new Podaci(filepath);
            // todo - mijenjati dataset ovisno o potrebama i provjeriti dal je dobro
        }

        /// <summary>
        /// Ucitava podatke iz datoteke kada postoji krosvalidacija
        /// </summary>
        /// <param name="filepath">file path do datoteke sa podacima</param>
        /// <param name="noOfFolds">broj foldova koji se koriste kod k-fold validacije</param>
        /// <param name="rotateFolds">da li ce se rotirati foldovi kao kod k-fold validacije</param>
        /// <param name="createEvaluationSet">da li ce se kreirati dodatni skup za evaluaciju (ne spada u k-fold)</param>
        public void UcitajDataSet(string filepath, int noOfFolds, bool rotateFolds, bool createEvaluationSet)
        {
            _podaci = new Podaci(filepath, noOfFolds, createEvaluationSet);
            _rotateFolds = rotateFolds;
        }

        private static IEnumerable<double> IzračunajVrijednostiJedinke(Jedinka jedinka, DataSet dataSet)
        {
            var rezultatiJedinke = new List<double>(new double[dataSet.BrojZapisa]);

            Parallel.For(0, dataSet.Varijable.Count,
                i => rezultatiJedinke[i] = jedinka.Izracunaj(dataSet.Varijable[i]));

            //for (int i = 0; i < dataSet.Varijable.Count; i++)
            //{
            //    rezultatiJedinke[i] = jedinka.Izracunaj((dataSet.Varijable[i]));
            //}
            return rezultatiJedinke;
        }

        public bool SlijedeciPodaciZaUcenje()
        {
            var gotovo = !(_podaci.PromijeniFold() && _rotateFolds);
            return gotovo;
        }

        public void ResetirajFoldove()
        {
            _podaci.FoldForTesting = -1;
            SlijedeciPodaciZaUcenje();
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

        protected abstract double Greska(IEnumerable<double> rezultatiJedinke, IEnumerable<double> rezultatiPodataka);
    }

    public class MseEvaluation : Evaluation
    {
        protected override double Greska(IEnumerable<double> rezultatiJedinke, IEnumerable<double> rezultatiPodataka)
        {
            return rezultatiJedinke.Zip(rezultatiPodataka, (o, f) => Math.Pow(o - f, 2)).Average();
        }
    }

    public class MaeEvaluation : Evaluation
    {
        protected override double Greska(IEnumerable<double> rezultatiJedinke, IEnumerable<double> rezultatiPodataka)
        {
            return rezultatiJedinke.Zip(rezultatiPodataka, (o, f) => Math.Abs(o - f)).Average();
        }
    }

    public class MapeEvaluation : Evaluation
    {
        protected override double Greska(IEnumerable<double> rezultatiJedinke, IEnumerable<double> rezultatiPodataka)
        {
            return rezultatiJedinke.Zip(rezultatiPodataka, (o, f) => (Math.Abs(1 - o / f))).Average() * 100;
        }
    }
}
