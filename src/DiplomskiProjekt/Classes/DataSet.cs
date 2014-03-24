using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;

namespace DiplomskiProjekt.Classes
{
    public class Podaci
    {
        public static int SatniInterval;
        public static int BrojPrethodnihMjerenja;
        public static int BrojPodatakaPoSkupu;

        public int FoldForTesting { get; private set; }
        private readonly List<DataSet> _folds;

        public DataSet PodaciZaUcenje;
        public DataSet PodaciZaProvjeru;
        public DataSet PodaciZaEvaluaciju;

        public Podaci(string filename)
        {
            var lines = File.ReadAllLines(filename).ToList();
            //var names = lines[0].Split(';');
            lines.RemoveAt(0);
            var vrijednostiPoSatima = lines.Select(line => line.Split(';').Select(x => 
                double.Parse(x, CultureInfo.InvariantCulture.NumberFormat)).ToList()).ToList();

            var varijable = new List<List<double>>();
            var rezultati = new List<double>();

            for (var i = SatniInterval * BrojPrethodnihMjerenja; i < vrijednostiPoSatima.Count - 1; i++)
            {
                // todo maknuti 4, staviti oznaku indexiranja pravog stupca, ili nek uvijek bude 0
                var primjer = new List<double>();
                for (var j = 1; j <= BrojPrethodnihMjerenja; j++)
                    primjer.Add(vrijednostiPoSatima[i - SatniInterval * j][4]);

                //primjer.AddRange(vrijednostiPoSatima[i]); -> ne smije biti trenutna potrosnja

                if (primjer.Any(double.IsNaN) || double.IsNaN(vrijednostiPoSatima[i][4])) 
                    continue;

                varijable.Add(primjer);
                rezultati.Add(vrijednostiPoSatima[i][4]);
            }

            // posložiti te vrijednosti u datasetove
            if (BrojPodatakaPoSkupu * 6 > varijable.Count)
                throw new Exception();

            _folds = new List<DataSet>
            {
                new DataSet
                {
                    Rezultati = rezultati.GetRange(0, BrojPodatakaPoSkupu),
                    Varijable = varijable.GetRange(0, BrojPodatakaPoSkupu)
                },
                new DataSet
                {
                    Rezultati = rezultati.GetRange(BrojPodatakaPoSkupu, BrojPodatakaPoSkupu),
                    Varijable = varijable.GetRange(BrojPodatakaPoSkupu, BrojPodatakaPoSkupu)
                },
                new DataSet
                {
                    Rezultati = rezultati.GetRange(BrojPodatakaPoSkupu*2, BrojPodatakaPoSkupu),
                    Varijable = varijable.GetRange(BrojPodatakaPoSkupu*2, BrojPodatakaPoSkupu)
                },
                new DataSet
                {
                    Rezultati = rezultati.GetRange(BrojPodatakaPoSkupu*3, BrojPodatakaPoSkupu),
                    Varijable = varijable.GetRange(BrojPodatakaPoSkupu*3, BrojPodatakaPoSkupu)
                },
                new DataSet
                {
                    Rezultati = rezultati.GetRange(BrojPodatakaPoSkupu*4, BrojPodatakaPoSkupu),
                    Varijable = varijable.GetRange(BrojPodatakaPoSkupu*4, BrojPodatakaPoSkupu)
                }
            };
            var evaluation = new DataSet
            {
                Varijable = varijable.GetRange(BrojPodatakaPoSkupu*5, BrojPodatakaPoSkupu),
                Rezultati = rezultati.GetRange(BrojPodatakaPoSkupu*5, BrojPodatakaPoSkupu)
            };

            FoldForTesting = 0;
            PodaciZaUcenje = new DataSet();
            foreach (var fold in _folds.Where((ds,i)=>i != FoldForTesting))
            {
                PodaciZaUcenje.Varijable = PodaciZaUcenje.Varijable.Concat(fold.Varijable).ToList();
                PodaciZaUcenje.Rezultati = PodaciZaUcenje.Rezultati.Concat(fold.Rezultati).ToList();
            }
            PodaciZaProvjeru = _folds[FoldForTesting];
            PodaciZaEvaluaciju = evaluation;
        }

        public bool PromijeniFold()
        {
            FoldForTesting++;
            if (FoldForTesting >= 5)
            {
                return false;
            }
            PodaciZaUcenje = new DataSet();
            foreach (var fold in _folds.Where((ds, i) => i != FoldForTesting))
            {
                PodaciZaUcenje.Varijable = PodaciZaUcenje.Varijable.Concat(fold.Varijable).ToList();
                PodaciZaUcenje.Rezultati = PodaciZaUcenje.Rezultati.Concat(fold.Rezultati).ToList();
            }
            PodaciZaProvjeru = _folds[FoldForTesting];
            return true;
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
