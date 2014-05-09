using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;

namespace DiplomskiProjekt.Classes
{
    public class Podaci
    {
        public static int BrojPrethodnihMjerenja;
        public static int BrojPodatakaPoFoldu;
        //public static List<string> ImenaVarijabli;

        public int FoldForTesting { get; private set; }
        private readonly List<DataSet> _folds;

        public DataSet PodaciZaUcenje;
        public DataSet PodaciZaProvjeru;
        public DataSet PodaciZaEvaluaciju;

        public Podaci(string filename, bool crx)
        {
            var lines = File.ReadAllLines(filename).ToList();
            //ImenaVarijabli = lines[0].Split(';').ToList();
            lines.RemoveAt(0); //ovo treba ako ima header
            var vrijednostiPoSatima = lines.Select(line => line.Split(';').Select(x => 
                double.Parse(x, CultureInfo.InvariantCulture.NumberFormat)).ToList()).ToList();

            var listaVarijabli = new List<List<double>>();
            var rezultati = new List<double>();

            for (var i = BrojPrethodnihMjerenja; i < vrijednostiPoSatima.Count - 1; i++)
            {
                var primjer = new List<double>();
                for (var j = 0; j < BrojPrethodnihMjerenja; j++)
                    primjer.Add(vrijednostiPoSatima[i - BrojPrethodnihMjerenja + j].Last()); //zadnja vrijednost mora biti potrosnja

                primjer.AddRange(vrijednostiPoSatima[i].Take(vrijednostiPoSatima[i].Count - 1)); //ne smije biti trenutna potrosnja

                if (primjer.Any(double.IsNaN) || double.IsNaN(vrijednostiPoSatima[i].Last())) 
                    continue;

                listaVarijabli.Add(primjer);
                rezultati.Add(vrijednostiPoSatima[i].Last());
            }

            // shuffle podataka da ne utjece podatak kao sto je mjesec
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

            
            if (!crx)
            {
                FoldForTesting = -1;
                PodaciZaUcenje = new DataSet {Rezultati = rezultati, Varijable = listaVarijabli};
            }
            else
            {
                // posložiti te vrijednosti u datasetove
                if (BrojPodatakaPoFoldu * 6 > listaVarijabli.Count)
                {
                    BrojPodatakaPoFoldu = (int)Math.Floor(listaVarijabli.Count / (double)6);
                    Console.WriteLine("Broj podataka po foldu je prevelik. Novi broj je " + BrojPodatakaPoFoldu);
                }
                _folds = new List<DataSet>
                {
                    new DataSet
                    {
                        Rezultati = rezultati.GetRange(0, BrojPodatakaPoFoldu),
                        Varijable = listaVarijabli.GetRange(0, BrojPodatakaPoFoldu)
                    },
                    new DataSet
                    {
                        Rezultati = rezultati.GetRange(BrojPodatakaPoFoldu, BrojPodatakaPoFoldu),
                        Varijable = listaVarijabli.GetRange(BrojPodatakaPoFoldu, BrojPodatakaPoFoldu)
                    },
                    new DataSet
                    {
                        Rezultati = rezultati.GetRange(BrojPodatakaPoFoldu*2, BrojPodatakaPoFoldu),
                        Varijable = listaVarijabli.GetRange(BrojPodatakaPoFoldu*2, BrojPodatakaPoFoldu)
                    },
                    new DataSet
                    {
                        Rezultati = rezultati.GetRange(BrojPodatakaPoFoldu*3, BrojPodatakaPoFoldu),
                        Varijable = listaVarijabli.GetRange(BrojPodatakaPoFoldu*3, BrojPodatakaPoFoldu)
                    },
                    new DataSet
                    {
                        Rezultati = rezultati.GetRange(BrojPodatakaPoFoldu*4, BrojPodatakaPoFoldu),
                        Varijable = listaVarijabli.GetRange(BrojPodatakaPoFoldu*4, BrojPodatakaPoFoldu)
                    }
                };
                var evaluation = new DataSet
                {
                    Varijable = listaVarijabli.GetRange(BrojPodatakaPoFoldu*5, BrojPodatakaPoFoldu),
                    Rezultati = rezultati.GetRange(BrojPodatakaPoFoldu*5, BrojPodatakaPoFoldu)
                };

                FoldForTesting = 0;
                PodaciZaUcenje = new DataSet();
                foreach (var fold in _folds.Where((ds, i) => i != FoldForTesting))
                {
                    PodaciZaUcenje.Varijable = PodaciZaUcenje.Varijable.Concat(fold.Varijable).ToList();
                    PodaciZaUcenje.Rezultati = PodaciZaUcenje.Rezultati.Concat(fold.Rezultati).ToList();
                }
                PodaciZaProvjeru = _folds[FoldForTesting];
                PodaciZaEvaluaciju = evaluation;
            }
        }

        public bool PromijeniFold()
        {
            FoldForTesting++;
            var nijeGotovo = true;
            if (FoldForTesting >= 5)
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
