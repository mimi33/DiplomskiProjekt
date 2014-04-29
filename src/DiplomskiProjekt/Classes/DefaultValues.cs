
namespace DiplomskiProjekt.Classes
{
    public static class DefaultValues
    {
        // Algortihm
        public static string AlgorithmName = "SteadyStateTournament";
        public static int PopulationSize = 150;
        public static int TournamentSize = 3;
        public static string TerminationName = "NumberOfGenerations";
        public static int TerminationValue = 500;

        // Tree
        public static int MaxDepth = 8;
        public static int MinDepth = 3;
        public static string FunctionSet = "+ - * /";

        // Crossover
        public static string CrxName = "SimpleCrx";
        
        // Mutation
        public static string MutationName = "SimpeMutation";
        public static double MutationFactor = 0.01;
        public static string ExtraConstantMutation = "true";

        // Evaluation
        public static string TrainEvaluatior = "MSE";
        public static bool CrossValidation = false;
        public static int FoldSize = 20;
        public static string DataPath = "PoSatima/sat{ID}.txt";
        public static int BrojPrijasnjihMjerenja = 7;

        // Log
        public static int LogGenerationFrequency = 10;
        public static string LogFilename = "Logovi/log{ID}.txt";
        public static int BatchSize = 1; 
    }
}
