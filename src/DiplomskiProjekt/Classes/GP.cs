using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DiplomskiProjekt.Classes
{
    public class GP
    {
        private const string XmlPath = "Config.xml";
        private string _logPath = "log.txt";
        private const string PotrosnjaPath = "potrosnja.csv";

        public int Iteracija;
        private bool _crossvalidation;

        public static bool Zavrsi;
        public static Mutation MutationOp;
        public static Crossover CrossoverOp;
        public static Evaluation EvaluationOp;
        public static Algorithm Algorithm;
        public static List<TerminationCondition> TerminationConditions;

        public static int GenerationFrequencyLogging;
        
        public  GP()
        {
            // O.o -> ovo bi trebalo cistiti datoteku na pocetku ako postoju.
            new StreamWriter(_logPath, false).Close();

            XDocument configDocument = XDocument.Load(XmlPath);
            foreach (string node in Enum.GetNames(typeof(NodeType)))
            {
                // uvijek vraća samo prvi node sa zadanim nazivom, ako ga ima
                XElement xe = configDocument.Descendants(node).FirstOrDefault();
                if (xe != null)
                {
                    InicijalizirajKomponentu(xe);
                }
                else // defaultne postavke
                {
                    InicijalizirajKomponentu(node);
                }
            }
        }

        public void Pokreni()
        {
            var populacijaNajboljih = new Populacija();
            bool zavrsenoUcenje;
            
            do
            {
                if (_crossvalidation)
                {
                    Algorithm.ReInicijaliziraj();
                    Zavrsi = false;
                    WriteToLog(EvaluationOp.FoldNaKojemSeUci() + ". fold");
                }
                for (Iteracija = 0; !Zavrsi; Iteracija++)
                {
                    Algorithm.OdradiGeneraciju();
                    if (Iteracija!= 0 && Iteracija % GenerationFrequencyLogging == 0)
                        WriteToLog(Algorithm.NajboljaJedinka, true);

                    TerminationConditions.ForEach(t => t.ConditionMet(this));
                }

                populacijaNajboljih.Add(Algorithm.NajboljaJedinka.Kopiraj());
                zavrsenoUcenje = EvaluationOp.SlijedeciPodaciZaUcenje();
            }while (_crossvalidation && !zavrsenoUcenje);

            if (_crossvalidation)
            {
                EvaluationOp.PromijeniUPodatkeZaEvaluaciju();
                WriteToLog("slijedi evaluacija najboljih jedinki na evaluation set");
                foreach (var jedinka in populacijaNajboljih)
                {
                    EvaluationOp.Evaluiraj(jedinka);
                    WriteToLog(jedinka, false);
                }
            }
           
            Console.ReadKey();
            
        }

        void WriteToLog(Jedinka jedinka, bool validation)
        {
            
            var writer = new StreamWriter(_logPath, true);
            var zapis = jedinka.Serijaliziraj();       

            if (validation)
            {
                zapis.Add(new XAttribute("iteracija", Iteracija));
                EvaluationOp.PromijeniUPodatkeZaProvjeru();
                zapis.Add(new XAttribute("validationError", Math.Round(EvaluationOp.Evaluiraj(jedinka), 4)));
                EvaluationOp.VratiUPodatkeZaUcenje();
                EvaluationOp.Evaluiraj(jedinka);
            }

            writer.WriteLine(zapis);
            writer.Close();
            Console.WriteLine("{0,5}: {1}", Iteracija, jedinka.GreskaJedinke);
        }

        void WriteToLog(string msg)
        {
            var writer = new StreamWriter(_logPath, true);
            var xelement = new XElement("Info");
            xelement.Add(new XAttribute("info", msg));
            writer.WriteLine(Environment.NewLine + xelement);
            writer.Close();
        }


        /// <summary>
        /// Osnovna funkcija za učitavanje parametara iz xml datoteke.
        /// Postoje hardkodirane vrijednosti tagova i defaultne vrijednost
        /// </summary>
        /// <param name="xmlElement">Čvor iz xml sa nazivom imeKomponente i svi njegovi atributi i elementi</param>
        private void InicijalizirajKomponentu(XElement xmlElement)
        {             
            var djeca = xmlElement.Descendants() as IList<XElement> ?? xmlElement.Descendants().ToList();

            switch ((NodeType) Enum.Parse(typeof(NodeType), xmlElement.Name.ToString()))
            {
                case NodeType.Tree:
                    Jedinka.MaxDubina = (int?)djeca.FirstOrDefault(xe => xe.Name == "MaxDepth") ?? 8;
                    Jedinka.MinDubina = (int?)djeca.FirstOrDefault(xe => xe.Name == "MinDepth") ?? 3;

                    //todo gdje ovo staviti?? ne bi smijelo biti veze na evOp
                    Cvor.ZavrsniCvorovi = new List<Cvor> ();
                    for (int i = 0; i < EvaluationOp.DataSet.BrojVarijabli; i++)
                    {
                        Cvor.ZavrsniCvorovi.Add(new Cvor(i));
                        Cvor.ZavrsniCvorovi.Add(new Cvor(false));
                    }

                    var funkcijskiCvorovi = (string) djeca.FirstOrDefault(xe => xe.Name == "FunctionSet");
                    if (funkcijskiCvorovi != null)
                    {
                        Cvor.FunkcijskiCvorovi =
                            funkcijskiCvorovi.Split(' ').Select(
                                c => new Cvor(StringEnum.GetValueFromDescription<TipFunkcije>(c))).ToList();
                    }
                    else
                    {
                        Cvor.FunkcijskiCvorovi = new List<Cvor>
                        {
                            new Cvor(TipFunkcije.Plus),
                            new Cvor(TipFunkcije.Minus),
                            new Cvor(TipFunkcije.Puta),
                            new Cvor(TipFunkcije.Djeljeno)
                        };
                    }
                    break;

                case NodeType.Algorithm:
                    var att = xmlElement.Attribute("name").Value;
                    var popSize = (int?) djeca.FirstOrDefault(xe => xe.Name == "PopulationSize") ?? 100;
                    switch (att)
                    {
                        case "SteadyStateTournament":
                            var k = (int?)djeca.FirstOrDefault(xe => xe.Name == "ParamK") ?? 3;
                            Algorithm = new SteadyStateTournament(popSize, k);
                            break;
                            //todo dodati druge algoritme tu
                    }
                    TerminationConditions = TerminationConditions ?? new List<TerminationCondition>();
                    var term = djeca.FirstOrDefault(xe => xe.Name == "Termination");
                    if (term != null)
                    {
                        var termConditions = term.Descendants();
                        foreach (var termCondition in termConditions)
                        {
                            switch (termCondition.Attribute("name").Value)
                            {
                                case "NumberOfGenerations":
                                    var maxBrojGen = (int?) termCondition ?? 100;
                                    TerminationConditions.Add(new MaxGenerationCondition(maxBrojGen));
                                    break;
                                    //todo dodati druge term cond tu
                            }
                        }
                    }
                    break;

                case NodeType.Mutation:
                    att = xmlElement.Attribute("name").Value;
                    var mutFactor = (double?) djeca.FirstOrDefault(xe => xe.Name == "MutFactor") ?? 0.01;
                    switch (att)
                    {
                        case "SimpleMut":
                            MutationOp = new SimpleMutation { MutationFactor = mutFactor };
                            break;
                            // todo dodati za ostale mutacije
                    }
                    break;
                
                case NodeType.Crossover:
                    att = xmlElement.Attribute("name").Value;
                    switch (att)
                    {
                        case "SimpleCrx":
                            CrossoverOp = new SimpleCrossover();
                            break;
                            //todo tu dodati druge operatore križanja
                    }
                    break;

                case NodeType.Evaluation:
                    Podaci.BrojPodatakaPoSkupu = (int?)djeca.FirstOrDefault(xe => xe.Name == "FoldSize") ?? 20;
                    att = (string) djeca.FirstOrDefault(xe => xe.Name == "TrainEvaluator") ?? "MSE";
                    switch (att)
                    {
                        case "MSE":
                            EvaluationOp = new MseEvaluation();
                            break;
                        case "MAPE":
                            EvaluationOp = new MapeEvaluation();
                            break;
                        case "MAE":
                            EvaluationOp = new MaeEvaluation();
                            break;
                    }
                    EvaluationOp.UcitajDataSet(PotrosnjaPath);
                    _crossvalidation = (bool?) djeca.FirstOrDefault(xe => xe.Name == "Crossvalidation") ?? false;
                    break;

                case NodeType.Log:
                    GenerationFrequencyLogging = (int?) djeca.FirstOrDefault(xe => xe.Name == "GenerationFrequency") ?? 10;
                    _logPath = (string) djeca.FirstOrDefault(xe => xe.Name == "FileName") ?? "log.txt";
                    break;

                default:
                    throw new Exception("nepoznato ime komponente");
            }
        }

        private void InicijalizirajKomponentu(string imeKomponente)
        {
            switch ((NodeType)Enum.Parse(typeof(NodeType), imeKomponente))
            {
                case NodeType.Tree:
                    Jedinka.MaxDubina = 8;
                    Jedinka.MinDubina = 3;
                    Cvor.ZavrsniCvorovi = new List<Cvor> ();
                    for (var i = 0; i < EvaluationOp.DataSet.BrojVarijabli; i++) //todo ovdje ne bi smijelo biti veze na evOp
                        Cvor.ZavrsniCvorovi.Add(new Cvor(i));

                    Cvor.FunkcijskiCvorovi = new List<Cvor>
                        {
                            new Cvor(TipFunkcije.Plus),
                            new Cvor(TipFunkcije.Minus),
                            new Cvor(TipFunkcije.Puta),
                            new Cvor(TipFunkcije.Djeljeno)
                        };
                    break;
                case NodeType.Algorithm:
                    Algorithm = new SteadyStateTournament(velicinaPopulacije: 100, velicinaTurnira: 3);
                    TerminationConditions = TerminationConditions ?? new List<TerminationCondition>();
                    TerminationConditions.Add(new MaxGenerationCondition(100));
                    _crossvalidation = false;
                    break;
                case NodeType.Mutation:
                    MutationOp = new SimpleMutation { MutationFactor = 0.01 };
                    break;
                case NodeType.Crossover:
                    CrossoverOp = new SimpleCrossover();
                    break;
                case NodeType.Evaluation:
                    Podaci.BrojPodatakaPoSkupu = 20;
                    EvaluationOp = new MseEvaluation();
                    EvaluationOp.UcitajDataSet(PotrosnjaPath);
                    break;
                case NodeType.Log:
                    GenerationFrequencyLogging = 10;
                    break;
                default:
                    throw new Exception("nepoznato ime komponente");
            }
        }
    }
}
