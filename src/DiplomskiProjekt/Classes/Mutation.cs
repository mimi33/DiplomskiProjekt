namespace DiplomskiProjekt.Classes
{
    public abstract class Mutation
    {
        public double MutationFactor;
        public abstract Jedinka Mutiraj(Jedinka jedinka);
    }

    public class SimpleMutation : Mutation
    {
        public override Jedinka Mutiraj(Jedinka jedinka)
        {
            MutirajCvor(jedinka.Korjen);
            return jedinka;
        }

        void MutirajCvor(Cvor cvor)
        {
            if (RandomGenerator.GetUniform() < MutationFactor)
                cvor.ZamjeniSaIstimTipom();
            if (cvor.Djeca == null) 
                return;
            foreach (var c in cvor.Djeca)
                MutirajCvor(c);
        }
    }

    //todo implementirati još jedan operator mutacije
}
