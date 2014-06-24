using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace DiplomskiProjekt.Classes
{
    [Serializable]
    public class GP
    {
        private readonly int _id;
        private string _logPath;

        public int Iteracija;
        private int _batchNo;
        private bool _dodatniSkupZaEvaluaciju;

        public static bool Zavrsi;
        public static Mutation MutationOp;
        public static Crossover CrossoverOp;
        public static Evaluation EvaluationOp;
        private static Algorithm _algorithm;
        private static List<TerminationCondition> _terminationConditions;

        private static int _generationFrequencyLogging;
        
// ReSharper disable once MemberCanBePrivate.Global
        public GP(){}

        public GP(int id, string configPath)
        {
            _id = id;
            Zavrsi = false;
            Iteracija = 0;
            var configDocument = XDocument.Load(configPath);
            foreach (var node in Enum.GetNames(typeof(NodeType)))
            {
                // uvijek vraća samo prvi node sa zadanim nazivom, ako ga ima
                var xe = configDocument.Descendants(node).FirstOrDefault();
                InicijalizirajKomponentu(xe, node);
            }
        }

        public void Pokreni()
        {
            System.IO.Directory.CreateDirectory("Logovi");
            new XDocument(new XElement("Log")).Save(_logPath);
            if (_batchNo == 1)
            {
                if (EvaluationOp.IsCrossValidation)
                    PokreniCrossValidation();
                else
                    Iteriraj();
            }
            else
            {
                for (var batch = 1; batch <= _batchNo; batch++)
                {
                    InitNewBatchInLog(batch);

                    if (EvaluationOp.IsCrossValidation)
                        PokreniCrossValidation();
                    else
                    {
                        _algorithm.ResetirajPopulaciju();
                        Zavrsi = false;
                        Iteriraj();
                    }
                }
            }
        }

        private void Iteriraj()
        {
            for (Iteracija = 0; !Zavrsi; Iteracija++)
            {
                _algorithm.OdradiGeneraciju();
                if (Iteracija != 0 && Iteracija % _generationFrequencyLogging == 0)
                    WriteToLog(_algorithm.NajboljaJedinka, EvaluationOp.IsCrossValidation, Iteracija);

                _terminationConditions.ForEach(t => t.ConditionMet(this));
            }
        }

        private void PokreniCrossValidation()
        {
            var populacijaNajboljih = new Populacija();
            var zavrsenoUcenje = false;
            EvaluationOp.ResetirajFoldove();
            while (!zavrsenoUcenje)
            {
                _algorithm.ResetirajPopulaciju();
                Zavrsi = false;
                InitNewFoldInLog("Train", EvaluationOp.FoldNaKojemuSeUci + 1);

                Iteriraj();

                populacijaNajboljih.Add(_algorithm.NajboljaJedinka.Kopiraj());
                zavrsenoUcenje = EvaluationOp.SlijedeciPodaciZaUcenje();
            }

            if (!_dodatniSkupZaEvaluaciju) 
                return;

            InitNewFoldInLog("Evaluation", -1);
            foreach (var jedinka in populacijaNajboljih)
            {
                EvaluationOp.Evaluiraj(jedinka);
                WriteToLog(jedinka, false, -1);
            }
        }

        void WriteToLog(Jedinka jedinka, bool validation, int iteracija)
        {
            var log = XDocument.Load(_logPath);
            var zapis = jedinka.Serijaliziraj();

            if (iteracija > 0)
            {
                zapis.Add(new XAttribute("iteracija", Iteracija));
            }
            if (validation)
            {
                zapis.Add(new XAttribute("validationError", Math.Round(EvaluationOp.Validiraj(jedinka), 4)));
            }
            

            if (log.Root == null)
                return;

            if (EvaluationOp.IsCrossValidation && _batchNo != 1)
                log.Root.Elements("Batch").Last().Elements("XValidation").Last().Add(zapis);
            else if (_batchNo != 1)
                log.Root.Elements().Last().Add(zapis);
            else
                log.Root.Add(zapis);

            log.Save(_logPath);
            Console.WriteLine("{0,5}: {1}", Iteracija, jedinka.GreskaJedinke);
        }

        void InitNewBatchInLog(int batchNo)
        {
            var log = XDocument.Load(_logPath);
            if (log.Root != null) 
                log.Root.Add(new XElement("Batch", new XAttribute("number", batchNo)));
            log.Save(_logPath);
            Console.WriteLine("\nBatch:" + batchNo);
        }

        void InitNewFoldInLog(string type, int foldNo)
        {
            var log = XDocument.Load(_logPath);
            if (log.Root == null) return;

            if (_batchNo == 1)
                log.Root.Add(new XElement("XValidation", new XAttribute("type", type), new XAttribute("foldNumber", foldNo)));
            else
                log.Root.Elements("Batch").Last().Add(new XElement("XValidation", new XAttribute("type", type), new XAttribute("foldNumber", foldNo)));
            log.Save(_logPath);
            Console.WriteLine("Fold: " + foldNo);
        }

        /// <summary>
        /// Osnovna funkcija za učitavanje parametara iz xml datoteke.
        /// Postoje hardkodirane vrijednosti tagova i defaultne vrijednost u Klasi DefaultValues
        /// </summary>
        /// <param name="xmlElement">Čvor iz xml sa nazivom imeKomponente i svi njegovi atributi i elementi</param>
        /// <param name="nodeName">Ime čvora iz xmla za koji se učitavaju parametri</param>
        private void InicijalizirajKomponentu(XElement xmlElement, string nodeName)
        {
            var djeca = (xmlElement != null)
                ? (xmlElement.Descendants() as IList<XElement> ?? xmlElement.Descendants().ToList())
                : new XElement[0];

            switch ((NodeType) Enum.Parse(typeof(NodeType), nodeName))
            {
                case NodeType.Tree:
                    Jedinka.MaxDubina = (int?)djeca.FirstOrDefault(xe => xe.Name == "MaxDepth") ?? DefaultValues.MaxDepth;
                    Jedinka.MinDubina = (int?)djeca.FirstOrDefault(xe => xe.Name == "MinDepth") ?? DefaultValues.MinDepth;

                    var funkcijskiCvorovi = (string) djeca.FirstOrDefault(xe => xe.Name == "FunctionSet") ?? DefaultValues.FunctionSet;
                    Cvor.FunkcijskiCvorovi =
                        funkcijskiCvorovi.Split(' ').Select(
                            c => new Cvor(StringEnum.GetValueFromDescription<TipFunkcije>(c))).ToList();
                    if (Cvor.FunkcijskiCvorovi.Count(c => c.BrojDjece == 1) == 1)
                    {
                        Cvor.FunkcijskiCvorovi.Add(new Cvor(TipFunkcije.ID));
                    }
                    break;

                case NodeType.Algorithm:
                    var att = (xmlElement != null) ? xmlElement.Attribute("name").Value : DefaultValues.AlgorithmName;
                    var popSize = (int?) djeca.FirstOrDefault(xe => xe.Name == "PopulationSize") ?? DefaultValues.PopulationSize;
                    var k = (int?)djeca.FirstOrDefault(xe => xe.Name == "ParamK") ?? DefaultValues.TournamentSize;
                    switch (att)
                    {
                        case "SteadyStateTournamentOneOperator":
                            _algorithm = new SteadyStateTournamentOneOperator(popSize, k);
                            break;
                        case "SteadyStateTournamentTwoOperators":
                            _algorithm = new SteadyStateTournamentTwoOperators(popSize, k);
                            break;
                        case "GenerationalTournamentOneOperator":
                            _algorithm = new GenerationalTournamentOneOperator(popSize, k);
                            break;
                        case "GenerationalTournamentTwoOperators":
                            _algorithm = new GenerationalTournamentTwoOperators(popSize, k);
                            break;
                    }
                    _terminationConditions = _terminationConditions ?? new List<TerminationCondition>();
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
                                    _terminationConditions.Add(new MaxGenerationCondition(maxBrojGen));
                                    break;
                                    //todo dodati druge term cond tu
                            }
                        }
                        if (_terminationConditions.Count == 0)
                        {
                            switch (DefaultValues.TerminationName)
                            {
                                case "NumberOfGenerations":
                                    _terminationConditions.Add(new MaxGenerationCondition(DefaultValues.TerminationValue));
                                    break;
                            }
                        }
                    }
                    else 
                    {
                        switch (DefaultValues.TerminationName)
                        {
                            case "NumberOfGenerations":
                                _terminationConditions.Add(new MaxGenerationCondition(DefaultValues.TerminationValue));
                                break;
                        }
                    }
                    break;

                case NodeType.Mutation:
                    att = (string) djeca.FirstOrDefault(xe => xe.Name == "Name") ?? DefaultValues.MutationName;
                    var mutFactor = (double?) djeca.FirstOrDefault(xe => xe.Name == "MutFactor") ?? DefaultValues.MutationFactor;
                    switch (att)
                    {
                        case "PointMutation":
                            MutationOp = new PointMutation { MutationFactor = mutFactor };
                            break;
                        case "HoistMutation":
                            MutationOp = new HoistMutation {MutationFactor = mutFactor};
                            break;
                    }
                    MutationOp.ConstantMutationEnabled =
                        ((string) djeca.FirstOrDefault(xe => xe.Name == "ExtraConstantMutation") ??
                         DefaultValues.ExtraConstantMutation) == "true";
                    break;
                
                case NodeType.Crossover:
                    att = (string) djeca.FirstOrDefault(xe => xe.Name == "Name") ?? DefaultValues.CrxName;
                    var crxFactor = (double?)djeca.FirstOrDefault(xe => xe.Name == "CrxFactor") ?? DefaultValues.CrxFactor;
                    switch (att)
                    {
                        case "OnePointCrossover":
                            CrossoverOp = new OnePointCrossover {CrossoverFactor = crxFactor};
                            break;
                        case "UniformCrossover":
                            CrossoverOp = new UniformCrossover {CrossoverFactor = crxFactor};
                            break;
                    }
                    break;

                case NodeType.Evaluation:
                    Podaci.BrojPrethodnihMjerenja = (int?)djeca.FirstOrDefault(xe => xe.Name == "PreviousLoads") ?? DefaultValues.BrojPrijasnjihMjerenja;
                    att = (string) djeca.FirstOrDefault(xe => xe.Name == "TrainEvaluator") ?? DefaultValues.TrainEvaluatior;
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
                    var potrosnjaPath =
                        ((string) djeca.FirstOrDefault(xe => xe.Name == "DataPath") ?? DefaultValues.DataPath).Replace("{ID}",
                            _id.ToString(CultureInfo.InvariantCulture));
                    var crossValidation = djeca.FirstOrDefault(xe => xe.Name == "Crossvalidation");
                    EvaluationOp.IsCrossValidation = crossValidation != null;
                    if (crossValidation != null)
                    {
                        var brojFoldova = (int?) crossValidation.Element("NoOfFolds") ?? DefaultValues.NumberOfFolds;
                        var rotacijaFoldova = (bool?)crossValidation.Element("RotateFolds") ?? DefaultValues.RotateFolds;
                        _dodatniSkupZaEvaluaciju = (bool?) crossValidation.Element("CreateEvaluationSet") ??
                                                      DefaultValues.CreateEvaluationSet;
                        EvaluationOp.UcitajDataSet(potrosnjaPath, brojFoldova, rotacijaFoldova, _dodatniSkupZaEvaluaciju);
                    }
                    else
                    {
                        EvaluationOp.UcitajDataSet(potrosnjaPath);
                    }

                    Cvor.ZavrsniCvorovi = new List<Cvor>();
                    for (var i = 0; i < EvaluationOp.BrojVarijabli; i++)
                    {
                        Cvor.ZavrsniCvorovi.Add(new Cvor(i));     //varijabla
                        Cvor.ZavrsniCvorovi.Add(new Cvor(false)); //konstanta
                    }
                    break;

                case NodeType.Log:
                    _generationFrequencyLogging = (int?) djeca.FirstOrDefault(xe => xe.Name == "GenerationFrequency") ?? DefaultValues.LogGenerationFrequency;
                    _logPath =
                        ((string) djeca.FirstOrDefault(xe => xe.Name == "FileName") ?? DefaultValues.LogFilename).Replace("{ID}",
                            _id.ToString(CultureInfo.InvariantCulture));
                    _batchNo = (int?) djeca.FirstOrDefault(xe => xe.Name == "BatchNo") ?? DefaultValues.BatchSize;
                    break;

                default:
                    throw new Exception("nepoznato ime komponente");
            }
        }
    }
}
