namespace DiplomskiProjekt.Classes
{
    public abstract class Mutation
    {
        public bool ConstantMutationEnabled;
        public double MutationFactor;
        public abstract Jedinka Mutiraj(Jedinka jedinka);

        public void MutirajKonstantu(Cvor cvor)
        {
            if (cvor.Tip != TipCvora.Konstanta)
                return;
            cvor.Vrijednost += RandomGenerator.GetNormal(0, 5);
            //todo odrediti koliko je raspršenje i odrediti vjerojatnost i čitanje iz configa?
        }
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
            {
                if (cvor.Tip == TipCvora.Konstanta && RandomGenerator.GetUniform() < 0.5d)
                    MutirajKonstantu(cvor);
                else
                    cvor.ZamjeniSaIstimTipom();
            }
            if (cvor.Djeca == null) 
                return;
            foreach (var c in cvor.Djeca)
                MutirajCvor(c);
        }
    }

    //todo implementirati još jedan operator mutacije
}
