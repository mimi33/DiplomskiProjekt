using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomskiProjekt.Classes
{
    public enum TipCvora { Funkcija, Konstanta, Varijabla};

    public enum TipFunkcije
    {
        [StringValue("+")]
        Plus,
        [StringValue("-")]
        Minus,
        [StringValue("*")]
        Puta,
        [StringValue("/")]
        Djeljeno,
        [StringValue("max")]
        Max,
        [StringValue("min")]
        Min,
        [StringValue("exp")]
        Exponent,
        [StringValue("sqrt")]
        Korijen,
        [StringValue("log")]
        Log,
        [StringValue("sin")]
        Sin,
        [StringValue("ifRadniDan")]
        IfRadniDan,
        [StringValue("id")]
        ID
    };

    public class Cvor
    {
        public static List<Cvor> ZavrsniCvorovi;
        public static List<Cvor> FunkcijskiCvorovi;

        public Cvor Roditelj;
        public List<Cvor> Djeca;
        public int BrojDjece;
        public double Vrijednost;
        public TipCvora Tip;

        /// <summary>
        /// Generički konstruktor.
        /// </summary>
        private Cvor(){}

        /// <summary>
        /// Za kreiranje novog funkcijskog čvora bez roditelja.
        /// Služi za početno generiranje čvorova koji će se kasnije kopirati.
        /// </summary>
        /// <param name="tipFunkcije">Funkcija koja će biti u tom čvoru</param>
        public Cvor(TipFunkcije tipFunkcije)
        {
            Roditelj = null;
            Tip = TipCvora.Funkcija;
            Vrijednost = (double) tipFunkcije;
            Djeca = new List<Cvor>();
            switch ((TipFunkcije)Vrijednost)
            {
                case TipFunkcije.Plus:
                case TipFunkcije.Minus:
                case TipFunkcije.Puta:
                case TipFunkcije.Djeljeno:
                case TipFunkcije.IfRadniDan:
                case TipFunkcije.Max:
                case TipFunkcije.Min:
                    BrojDjece = 2;
                    break;
                case TipFunkcije.Log:
                case TipFunkcije.Sin:
                case TipFunkcije.Exponent:
                case TipFunkcije.Korijen:
                case TipFunkcije.ID:
                    BrojDjece = 1;
                    break;
            }
        }

        /// <summary>
        /// Za kreiranje novog završnog čvora u kojem će biti varijabla.
        /// Služi za početno generiranje čvorova koji će se kasnije kopirati.
        /// </summary>
        /// <param name="indexVarijable">Redni broj varijable u listi varijabli.</param>
        public Cvor(int indexVarijable)
        {
            Roditelj = null;
            Tip = TipCvora.Varijabla;
            Vrijednost = indexVarijable;
            BrojDjece = 0;
            Djeca = null;
        }

        /// <summary>
        /// Za kreiranje novog završnog čvora u kojem će biti konstanta.
        /// Služi za početno generiranje čvorova koji će se kasnije kopirati. Tek nakon kopiranja će konstanta poprimiti vrijednost.
        /// </summary>
        /// <param name="dodijelitiVrijednost">Zastavica kojom se određuje da li će čvor odmah dobiti vrijednost konstante.</param>
        public Cvor(bool dodijelitiVrijednost)
        {
            Roditelj = null;
            Tip = TipCvora.Konstanta;
            Vrijednost = dodijelitiVrijednost ? RandomGenerator.GetNormal(0, 10) : double.NaN;
            BrojDjece = 0;
            Djeca = null;
        }

        public Cvor DodajCvor()
        {
            return RandomGenerator.GetUniform() < 0.5d ? DodajFunkcijskiCvor() : DodajZavrsniCvor();
        }

        /// <summary>
        /// Dodaje kopiju cvora u svoju listu cvorova, ako stane.
        /// Ako ne stane, vraća null.
        /// </summary>
        /// <param name="dijeteCvor">Cvor koji se dodaje kao dijete.</param>
        /// <returns>Kopirani cvor kojemu je postavljen roditelj.</returns>
        private Cvor DodajDijeteKaoKopiju(Cvor dijeteCvor)
        {
            if (Djeca == null || Djeca.Count == BrojDjece)
                return null;
            var novoDijete = dijeteCvor.Kopiraj();
            novoDijete.Roditelj = this;
            Djeca.Add(novoDijete);
            return novoDijete;
        }

        public Cvor DodajZavrsniCvor()
        {
            if (Djeca == null || Djeca.Count == BrojDjece)
                return null;
            var dijete = SlucajniZavrsniCvor();
            dijete.Roditelj = this;
            Djeca.Add(dijete);
            return dijete;
        }

        public Cvor DodajFunkcijskiCvor()
        {
            if (Djeca == null || Djeca.Count == BrojDjece)
                return null;
            var dijete = SlucajniFunkcijskiCvor();
            dijete.Roditelj = this;
            Djeca.Add(dijete);
            return dijete;
        }

        private int IndexOf(Cvor dijete)
        {
            return Djeca.IndexOf(dijete);
        }

        public string ToString(bool rekurzija)
        {
            switch (Tip)
            {
                case TipCvora.Funkcija:
                    {
                        var s = new StringBuilder();
                        var tip = ((TipFunkcije) Vrijednost);
                        s.Append(tip.GetStringValue() + " ");
                        
                        if (!rekurzija) return s.ToString();

                        foreach (var dijete in Djeca)
                            s.Append(dijete.ToString(true));
                        return s.ToString();
                    }
                case TipCvora.Konstanta:
                    return Math.Round(Vrijednost, 2).ToString("0.000 ");
                case TipCvora.Varijabla:
                    return Math.Round(Vrijednost, 0).ToString("$0 ");
            }
            return "";
        }

        public double RekurzivnoIzracunaj(List<double> varijable)
        {

            if (varijable == null) throw new Exception();
            var a = (BrojDjece == 0) ? 0 : Djeca[0].RekurzivnoIzracunaj(varijable);
            var b = (BrojDjece <= 1) ? 0 : Djeca[1].RekurzivnoIzracunaj(varijable);
            switch (Tip)
            {
                case TipCvora.Funkcija:
                    switch ((TipFunkcije)Vrijednost)
                    {
                        case TipFunkcije.Log:           return (Math.Abs(a) <= 1e-10) ? 0 : Math.Log(Math.Abs(a));
                        case TipFunkcije.Plus:          return a + b;
                        case TipFunkcije.Minus:         return a - b;
                        case TipFunkcije.Puta:          return a * b;
                        case TipFunkcije.Djeljeno:      return (Math.Abs(b) < 1e-10) ? 0 : a / b;
                        case TipFunkcije.Sin:           return Math.Sin(a);
                        case TipFunkcije.IfRadniDan:    return (Math.Abs(varijable[0]) < 1e-5) ? a : b;
                        case TipFunkcije.Max:           return Math.Max(a, b);
                        case TipFunkcije.Min:           return Math.Min(a, b);
                        case TipFunkcije.Exponent:      return (a > 10) ? Math.Exp(10) : Math.Exp(a);
                        case TipFunkcije.Korijen:       return Math.Sqrt(Math.Abs(a));
                        case TipFunkcije.ID:            return a;
                        default: return 0;
                    }
                case TipCvora.Konstanta:
                    return Vrijednost;
                case TipCvora.Varijabla:
                    return varijable[(int)Vrijednost];
            }
            return 0;
        }

        /// <summary>
        /// Kopiranje trenutrnog cvora, ne kopira djecu
        /// </summary>
        /// <returns>kopirani čvor</returns>
        public Cvor Kopiraj()
        {
            return new Cvor
            {
                Roditelj = null,
                BrojDjece = BrojDjece,
                Vrijednost = Vrijednost,
                Tip = Tip,
                Djeca = Djeca == null ? null : new List<Cvor>(),
            };
        }

        public void RekurzivnoKopiraj(Cvor zamjenskiCvor)
        {
            for (var i = 0; i < BrojDjece; i++)
            {
                var novoDijete = zamjenskiCvor.DodajDijeteKaoKopiju(Djeca[i]);
                Djeca[i].RekurzivnoKopiraj(novoDijete);
            }
        }

        public void ZamjeniSaIstimTipom() 
        {
            switch (Tip)
            {
                case TipCvora.Konstanta:
                case TipCvora.Varijabla:
                    var c = SlucajniZavrsniCvor();
                    Tip = c.Tip;
                    Vrijednost = c.Vrijednost;
                    break;
                case TipCvora.Funkcija:
                    c = SlucajniFunkcijskiCvor(BrojDjece);
                    while ((TipFunkcije)c.Vrijednost == (TipFunkcije)Vrijednost)
                        c = SlucajniFunkcijskiCvor(BrojDjece);
                    Vrijednost = c.Vrijednost;
                    break;
            }
        }

        public static Cvor SlucajniFunkcijskiCvor()
        {
            return RandomGenerator.GetRandomElement(FunkcijskiCvorovi).Kopiraj();
        }

        static Cvor SlucajniZavrsniCvor()
        {
            return RandomGenerator.GetRandomElement(ZavrsniCvorovi).Kopiraj();
        }

        static Cvor SlucajniFunkcijskiCvor(int brojDjece)
        {
            return RandomGenerator.GetRandomElement(FunkcijskiCvorovi.Where(x => x.BrojDjece == brojDjece).ToList()).Kopiraj();
        }

        public static void ZamjeniRoditelje(Cvor cvor1, Cvor cvor2)
        {
            var i1 = cvor1.Roditelj.IndexOf(cvor1);
            var i2 = cvor2.Roditelj.IndexOf(cvor2);
            cvor1.Roditelj.Djeca[i1] = cvor2;
            cvor2.Roditelj.Djeca[i2] = cvor1;
            var tmp = cvor1.Roditelj;
            cvor1.Roditelj = cvor2.Roditelj;
            cvor2.Roditelj = tmp;
        }

    }
}
