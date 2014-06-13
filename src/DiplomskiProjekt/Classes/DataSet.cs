using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;

namespace DiplomskiProjekt.Classes
{
    public class Podaci
    {
        public static int BrojPrethodnihMjerenja;
        //public static List<string> ImenaVarijabli;

        public int FoldForTesting;
        private readonly List<DataSet> _folds;

        public DataSet PodaciZaUcenje;
        public DataSet PodaciZaProvjeru;
        public readonly DataSet PodaciZaEvaluaciju; 

        public Podaci(string filename)
        {
            List<List<double>> listaVarijabli;
            List<double> rezultati;
            NapraviPodatke(filename, out listaVarijabli, out rezultati);

            FoldForTesting = -1;
            PodaciZaUcenje = new DataSet {Rezultati = rezultati, Varijable = listaVarijabli};
        }

        public Podaci(string filename, int noOfFolds, bool createEvalutaionSet)
        {
            List<List<double>> listaVarijabli;
            List<double> rezultati;
            NapraviPodatke(filename, out listaVarijabli, out rezultati);

            var brojPodatakaPoFoldu = listaVarijabli.Count / (noOfFolds + (createEvalutaionSet ? 1 : 0));

            _folds = new List<DataSet>();
            for (int i = 0; i < noOfFolds; i++)
            {
                _folds.Add(new DataSet
                {
                    Rezultati = rezultati.GetRange(i * brojPodatakaPoFoldu, brojPodatakaPoFoldu),
                    Varijable = listaVarijabli.GetRange(i * brojPodatakaPoFoldu, brojPodatakaPoFoldu)
                });
            }

            if (createEvalutaionSet)
            {
                PodaciZaEvaluaciju = new DataSet
                {
                    Varijable = listaVarijabli.GetRange(brojPodatakaPoFoldu * noOfFolds, brojPodatakaPoFoldu),
                    Rezultati = rezultati.GetRange(brojPodatakaPoFoldu * noOfFolds, brojPodatakaPoFoldu)
                };
            }

            FoldForTesting = 0;
            PodaciZaUcenje = new DataSet();
            foreach (var fold in _folds.Where((ds, i) => i != FoldForTesting))
            {
                PodaciZaUcenje.Varijable = PodaciZaUcenje.Varijable.Concat(fold.Varijable).ToList();
                PodaciZaUcenje.Rezultati = PodaciZaUcenje.Rezultati.Concat(fold.Rezultati).ToList();
            }
            PodaciZaProvjeru = _folds[FoldForTesting];
        }

        private static void NapraviPodatke(string filename, out List<List<double>> listaVarijabli, out List<double> rezultati)
        {
            var lines = File.ReadAllLines(filename).ToList();
            //ImenaVarijabli = lines[0].Split(';').ToList();
            lines.RemoveAt(0); //ovo treba ako ima header
            var vrijednostiPoSatima = lines.Select(line => line.Split(';').Select(x =>
                double.Parse(x, CultureInfo.InvariantCulture.NumberFormat)).ToList()).ToList();

            listaVarijabli = new List<List<double>>();
            rezultati = new List<double>();

            for (var i = BrojPrethodnihMjerenja; i < vrijednostiPoSatima.Count - 1; i++)
            {
                var primjer = new List<double>();
                for (var j = 0; j < BrojPrethodnihMjerenja; j++)
                    primjer.Add(vrijednostiPoSatima[i - BrojPrethodnihMjerenja + j].Last());
                        //zadnja vrijednost mora biti potrosnja

                primjer.AddRange(vrijednostiPoSatima[i].Take(vrijednostiPoSatima[i].Count - 1));
                    //ne smije biti trenutna potrosnja

                if (primjer.Any(double.IsNaN) || double.IsNaN(vrijednostiPoSatima[i].Last()))
                    continue;

                listaVarijabli.Add(primjer);
                rezultati.Add(vrijednostiPoSatima[i].Last());
            }

            // shuffle podataka da ne utjece podatak kao sto je mjesec - trebalo bi prosiriti skup za ucenje
            var n = rezultati.Count;
            while (n > 1)
            {
                n--;
                var k = RandomGenerator.GetIntRange(0, n + 1);
                var value = listaVarijabli[k];
                listaVarijabli[k] = listaVarijabli[n];
                listaVarijabli[n] = value;
                var value2 = rezultati[k];
                rezultati[k] = rezultati[n];
                rezultati[n] = value2;
            }
        }

        public bool PromijeniFold()
        {
            FoldForTesting++;
            var nijeGotovo = true;
            if (FoldForTesting >= _folds.Count)
            {
                nijeGotovo = false;
                FoldForTesting = 0;
            }
            PodaciZaUcenje = new DataSet();
            foreach (var fold in _folds.Where((ds, i) => i != FoldForTesting))
            {
                PodaciZaUcenje.Varijable = PodaciZaUcenje.Varijable.Concat(fold.Varijable).ToList();
                PodaciZaUcenje.Rezultati = PodaciZaUcenje.Rezultati.Concat(fold.Rezultati).ToList();
            }
            PodaciZaProvjeru = _folds[FoldForTesting];
            return nijeGotovo;
        }
    }

    public class DataSet
    {
        public List<List<double>> Varijable;
        public List<double> Rezultati;

        public int BrojZapisa { get { return Varijable.Count; } }
        public int BrojVarijabli { get { return Varijable[0].Count; } }

        public DataSet()
        {
            Varijable = new List<List<double>>();
            Rezultati = new List<double>();
        }
    }
}
