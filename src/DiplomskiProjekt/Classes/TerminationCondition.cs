
namespace DiplomskiProjekt.Classes
{
    abstract public class TerminationCondition
    {
        public abstract void ConditionMet(GP gp);
    }

    public class MaxGenerationCondition : TerminationCondition
    {
        private readonly int _maxBrojGeneracija;
        public MaxGenerationCondition(int maxBrojGeneracija)
        {
            _maxBrojGeneracija = maxBrojGeneracija;
        }
        public override void ConditionMet(GP gp)
        {
            GP.Zavrsi = (gp.Iteracija >= _maxBrojGeneracija);
        }
    }
}
