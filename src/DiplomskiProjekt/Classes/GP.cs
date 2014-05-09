using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace DiplomskiProjekt.Classes
{
    public class GP
    {
        private readonly int _id;
        private string _logPath;

        public int Iteracija;
        private bool _crossvalidation;
        private int _batchNo;

        public static bool Zavrsi;
        public static Mutation MutationOp;
        public static Crossover CrossoverOp;
        public static Evaluation EvaluationOp;
        public static Algorithm Algorithm;
        public static List<TerminationCondition> TerminationConditions;

        public static int GenerationFrequencyLogging;
        
        public GP(int id, string configPath)
        {
            _id = id;
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
                if (_crossvalidation)
                    PokreniCrossValidation();
                else
                    Iteriraj();
            }
            else
            {
                for (var batch = 1; batch <= _batchNo; batch++)
                {
                    InitNewBatchInLog(batch);

                    if (_crossvalidation)
                        PokreniCrossValidation();
                    else
                    {
                        Algorithm.ResetirajPopulaciju();
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
                Algorithm.OdradiGeneraciju();
                if (Iteracija != 0 && Iteracija % GenerationFrequencyLogging == 0)
                    WriteToLog(Algorithm.NajboljaJedinka, _crossvalidation, Iteracija);

                TerminationConditions.ForEach(t => t.ConditionMet(this));
            }
        }

        private void PokreniCrossValidation()
        {
            var populacijaNajboljih = new Populacija();
            var zavrsenoUcenje = false;

            while (!zavrsenoUcenje)
            {
                Algorithm.ResetirajPopulaciju();
                Zavrsi = false;
                InitNewFoldInLog("Train", EvaluationOp.FoldNaKojemuSeUci + 1);

                Iteriraj();

                populacijaNajboljih.Add(Algorithm.NajboljaJedinka.Kopiraj());
                zavrsenoUcenje = EvaluationOp.SlijedeciPodaciZaUcenje();
            }
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

            if (_crossvalidation && _batchNo != 1)
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
                    break;

                case NodeType.Algorithm:
                    var att = (xmlElement != null) ? xmlElement.Attribute("name").Value : DefaultValues.AlgorithmName;
                    var popSize = (int?) djeca.FirstOrDefault(xe => xe.Name == "PopulationSize") ?? DefaultValues.PopulationSize;
                    switch (att)
                    {
                        case "SteadyStateTournament":
                            var k = (int?)djeca.FirstOrDefault(xe => xe.Name == "ParamK") ?? DefaultValues.TournamentSize;
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
                        if (TerminationConditions.Count == 0)
                        {
                            switch (DefaultValues.TerminationName)
                            {
                                case "NumberOfGenerations":
                                    TerminationConditions.Add(new MaxGenerationCondition(DefaultValues.TerminationValue));
                                    break;
                            }
                        }
                    }
                    else 
                    {
                        switch (DefaultValues.TerminationName)
                        {
                            case "NumberOfGenerations":
                                TerminationConditions.Add(new MaxGenerationCondition(DefaultValues.TerminationValue));
                                break;
                        }
                    }
                    break;

                case NodeType.Mutation:
                    att = (xmlElement != null) ? xmlElement.Attribute("name").Value : DefaultValues.MutationName;
                    var mutFactor = (double?) djeca.FirstOrDefault(xe => xe.Name == "MutFactor") ??
                                    DefaultValues.MutationFactor;
                    switch (att)
                    {
                        case "SimpleMut":
                            MutationOp = new SimpleMutation { MutationFactor = mutFactor };
                            break;
                            // todo dodati za ostale mutacije
                    }
                    MutationOp.ConstantMutationEnabled =
                        ((string) djeca.FirstOrDefault(xe => xe.Name == "ExtraConstantMutation") ??
                         DefaultValues.ExtraConstantMutation) == "true";
                    break;
                
                case NodeType.Crossover:
                    att = (xmlElement != null) ? xmlElement.Attribute("name").Value : DefaultValues.CrxName;
                    switch (att)
                    {
                        case "SimpleCrx":
                            CrossoverOp = new SimpleCrossover();
                            break;
                            //todo tu dodati druge operatore križanja
                    }
                    break;

                case NodeType.Evaluation:
                    Podaci.BrojPodatakaPoFoldu = (int?)djeca.FirstOrDefault(xe => xe.Name == "FoldSize") ?? DefaultValues.FoldSize;
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
                    _crossvalidation = (bool?)djeca.FirstOrDefault(xe => xe.Name == "Crossvalidation") ?? DefaultValues.CrossValidation;
                    var potrosnjaPath =
                        ((string) djeca.FirstOrDefault(xe => xe.Name == "DataPath") ?? DefaultValues.DataPath).Replace("{ID}",
                            _id.ToString(CultureInfo.InvariantCulture));
                    EvaluationOp.UcitajDataSet(potrosnjaPath, _crossvalidation);

                    //todo gdje ovo staviti?? ne bi smijelo biti veze na evOp
                    Cvor.ZavrsniCvorovi = new List<Cvor>();
                    for (var i = 0; i < EvaluationOp.BrojVarijabli; i++)
                    {
                        Cvor.ZavrsniCvorovi.Add(new Cvor(i));
                        Cvor.ZavrsniCvorovi.Add(new Cvor(false));
                    }
                    
                    break;

                case NodeType.Log:
                    GenerationFrequencyLogging = (int?) djeca.FirstOrDefault(xe => xe.Name == "GenerationFrequency") ?? DefaultValues.LogGenerationFrequency;
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
