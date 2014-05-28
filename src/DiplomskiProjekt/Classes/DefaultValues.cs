
namespace DiplomskiProjekt.Classes
{
    public static class DefaultValues
    {
        // Algortihm
        public const string AlgorithmName = "SteadyStateTournamentTwoOperators";
        public const int PopulationSize = 500;
        public const int TournamentSize = 3;
        public const string TerminationName = "NumberOfGenerations";
        public const int TerminationValue = 100;

        // Tree
        public const int MaxDepth = 8;
        public const int MinDepth = 3;
        public const string FunctionSet = "+ - * /";

        // Crossover
        public const string CrxName = "OnePointCrossover";
        public const double CrxFactor = 0.95d;

        // Mutation
        public const string MutationName = "PointMutation";
        public const double MutationFactor = 0.01;
        public const string ExtraConstantMutation = "true";

        // Evaluation
        public const string TrainEvaluatior = "MSE";
        public const bool CrossValidation = false;
        public const int FoldSize = 20;
        public const string DataPath = "PoSatima/sat{ID}.txt";
        public const int BrojPrijasnjihMjerenja = 7;

        // Log
        public const int LogGenerationFrequency = 10;
        public const string LogFilename = "Logovi/log{ID}.txt";
        public const int BatchSize = 1;
    }
}
