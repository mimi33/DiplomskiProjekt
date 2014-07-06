using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DiplomskiProjekt.Classes;

namespace Evaluiraj
{
    static class Program
    {
        // ReSharper restore PossibleNullReferenceException
        // ReSharper disable PossibleNullReferenceException
        static void Main(string[] args)
        {
            var configFileName = args.Length >= 1 ? args[0] : "Config.xml";

            RandomGenerator.SetSeedFromSystemTime();

            var root = XDocument.Load(configFileName).Root;
            var eval = root.Element("Evaluation");
            Evaluation evalOp;
            var evalOpName = (string) eval.Element("TrainEvaluator");
            switch (evalOpName)
            {
                case "MAPE":
                    evalOp = new MapeEvaluation();
                    break;
                case "MAE":
                    evalOp = new MaeEvaluation();
                    break;
                case "MSE":
                    evalOp = new MseEvaluation();
                    break;
                default:
                    throw new Exception("nije eval");
            }
            evalOp.IsCrossValidation = false;
            var prevLoads = (int?) eval.Element("Data").Element("PreviousLoads") ?? DefaultValues.BrojPrijasnjihMjerenja;
            var evalData = (string) eval.Element("EvaluationData") ?? "Eval/";
            var logs = (string) root.Element("Log").Element("FileName");

            // ucitati sve jedinke
            var jedinkeList = new List<List<Jedinka>>();
            for (var i = 0; i < 24; i++)
            {
                jedinkeList.Add(null);
            }

            Parallel.For(
                0,
                24,
                i =>
                {
                    var log = logs.Replace("{ID}", i.ToString(CultureInfo.InvariantCulture));
                    var logRoot = XDocument.Load(log).Root;

                    var mjerenja = logRoot.Elements("Batch").ToList();

                    if (!mjerenja.Any()) //ako nije bilo batch, a mozda je bilo XV
                    {
                        mjerenja = logRoot.Elements("XValidation").ToList();
                        if (!mjerenja.Any())
                        {
                            mjerenja = null;
                        }
                    }
                    else // ako je bilo batch, mozda je bilo i XV
                    {
                        if (mjerenja.Elements("XValidation").Any())
                        {
                            mjerenja = mjerenja.Elements("XValidation").ToList();
                        }
                    }


                    var jedinkeString = mjerenja != null
                        ? mjerenja.Select(node => node.Elements().Last()).ToList()
                        : new List<XElement> {(XElement) logRoot.LastNode};

                    //var jedinke = new List<Jedinka>();
                    //Parallel.ForEach(jedinkeString, element => jedinke.Add(Jedinka.Deserijaliziraj(element.ToString())));

                    var jedinke = jedinkeString.Select(value => Jedinka.Deserijaliziraj(value.ToString())).ToList();

                    jedinkeList[i] = jedinke;
                }
                );


            var ukupnaStvarnaPotrosnja = new List<double>();
            var ukupnaPredvidenaPotrosnja = new List<double>();
            var ukupniRadniDani = new List<bool>();


            foreach (var filename in Directory.GetFiles(evalData))
            {
                // ucitati podatke za evaluaciju, evaluirati i zapisati u iste podatke
                var prijasnjaMjerenjaPoSatima = new List<List<double>>();
                for (var i = 0; i < 24; i++)
                {
                    prijasnjaMjerenjaPoSatima.Add(new List<double>());
                }
                var predvidjenaPotrosnja = new List<double>();
                var stvarnaPotrosnja = new List<double>();
                var radniDan = new List<bool>();

                // ucitavati liniju po liniju
                var input = new StreamReader(filename);
                var output = new StreamWriter(filename.Replace(".", "-evaluated."));
                string line;
                input.ReadLine(); // header
                while ((line = input.ReadLine()) != null)
                {
                    var podaci = line.Split(';').Select(x => Convert.ToDouble(x, CultureInfo.InvariantCulture)).ToList();
                    var sat = Convert.ToInt32(podaci.First());
                    var rezultat = podaci.Last();

                    var prijasnjaMjerenja = prijasnjaMjerenjaPoSatima[sat];
                    if (prijasnjaMjerenja.Count < prevLoads)
                    {
                        // ako nema dovoljno prijasnjih mjerenja, nece probati predvidjati, nece nikaj zapisat na izlaz
                        prijasnjaMjerenja.Add(rezultat);

                        continue;
                    }
                    if (!podaci.Any(double.IsNaN) && !prijasnjaMjerenja.Any(double.IsNaN))
                    {
                        // ako ima dovoljno prijasnjih mjerenja, onda ce ici predvidjeti
                        var ds = new DataSet
                        {
                            Varijable =
                                new List<List<double>>
                            {
                                podaci.Skip(1).Take(podaci.Count - 2).Concat(prijasnjaMjerenja).ToList()
                            },
                            Rezultati = new List<double> { rezultat }
                        };

                        var srednjaGreska = // asambl!! :)
                            jedinkeList[sat].Select(j => Evaluation.IzračunajVrijednostiJedinke(j, ds).First()).Average();

                        if (double.IsNaN(srednjaGreska))
                            srednjaGreska = 0;

                        var r = string.Join(";", podaci.ConvertAll(x => x.ToString(CultureInfo.InvariantCulture))) + ";" + srednjaGreska.ToString(CultureInfo.InvariantCulture);
                        output.WriteLine(r);
                        stvarnaPotrosnja.Add(rezultat);
                        predvidjenaPotrosnja.Add(srednjaGreska);
                        radniDan.Add(Convert.ToBoolean(podaci.First(), CultureInfo.InvariantCulture));
                    }
                    prijasnjaMjerenja.RemoveAt(0);
                    prijasnjaMjerenja.Add(rezultat);
                }
                input.Close();
                output.Close();
                output = new StreamWriter(filename.Replace(".csv", "-error.txt"));
                output.WriteLine(evalOpName + ": " + evalOp.Greska(predvidjenaPotrosnja, stvarnaPotrosnja));

                output.WriteLine("R: " + (IzracunajR(stvarnaPotrosnja, predvidjenaPotrosnja)).ToString(CultureInfo.InvariantCulture));

                var y_radniDan = new List<double>();
                var f_radniDan = new List<double>();
                var y_neradni = new List<double>();
                var f_neradni = new List<double>();

                for (int i = 0; i < stvarnaPotrosnja.Count; i++)
                {
                    if (radniDan[i])
                    {
                        y_radniDan.Add(stvarnaPotrosnja[i]);
                        f_radniDan.Add(predvidjenaPotrosnja[i]);
                    }
                    else
                    {
                        y_neradni.Add(stvarnaPotrosnja[i]);
                        f_neradni.Add(predvidjenaPotrosnja[i]);
                    }
                }

                output.WriteLine("radni_R: " + (IzracunajR(y_radniDan, f_radniDan)).ToString(CultureInfo.InvariantCulture));

                output.WriteLine("neradni_R: " + (IzracunajR(y_neradni, f_neradni)).ToString(CultureInfo.InvariantCulture));

                ukupnaStvarnaPotrosnja = ukupnaStvarnaPotrosnja.Concat(stvarnaPotrosnja).ToList();
                ukupnaPredvidenaPotrosnja = ukupnaPredvidenaPotrosnja.Concat(predvidjenaPotrosnja).ToList();
                ukupniRadniDani = ukupniRadniDani.Concat(radniDan).ToList();

                output.Close();
            }
            var zadnjaDat = new StreamWriter(evalData + "ukupni-error.txt");
            zadnjaDat.WriteLine(evalOpName + ": " + evalOp.Greska(ukupnaPredvidenaPotrosnja, ukupnaStvarnaPotrosnja));

            var radniY = new List<double>();
            var radniF = new List<double>();
            var neradniY = new List<double>();
            var neradniF = new List<double>();

            for (int i = 0; i < ukupnaStvarnaPotrosnja.Count; i++)
            {
                if (ukupniRadniDani[i])
                {
                    radniY.Add(ukupnaStvarnaPotrosnja[i]);
                    radniF.Add(ukupnaPredvidenaPotrosnja[i]);
                }
                else
                {
                    neradniY.Add(ukupnaStvarnaPotrosnja[i]);
                    neradniF.Add(ukupnaPredvidenaPotrosnja[i]);
                }
            }

            zadnjaDat.WriteLine("radni_R: " + (IzracunajR(radniY, radniF)).ToString(CultureInfo.InvariantCulture));
            zadnjaDat.WriteLine("neradni_R: " + (IzracunajR(neradniY, neradniF)).ToString(CultureInfo.InvariantCulture));
            zadnjaDat.Close();
        }



        private static double IzracunajR(List<double> stvarnaPotrosnja, List<double> predviđenjaPotrosnja)
        {
            var stvarnaProsjek = stvarnaPotrosnja.Average();
            var predvidjenaProsjek  = predviđenjaPotrosnja.Average();
            var brojnik =
                predviđenjaPotrosnja
                    .Zip(stvarnaPotrosnja,
                        (f, y) => (f - predvidjenaProsjek)*(y - stvarnaProsjek))
                    .Sum();
            var nazivnik = Math.Sqrt(stvarnaPotrosnja.Select(x => Math.Pow(x - stvarnaProsjek, 2)).Sum())
                       *Math.Sqrt(predviđenjaPotrosnja.Select(x => Math.Pow(x - predvidjenaProsjek, 2)).Sum());
            return brojnik/nazivnik;
        }
    }
}
