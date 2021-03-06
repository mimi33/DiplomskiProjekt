﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DiplomskiProjekt.Classes
{
    public class Jedinka : IComparable<Jedinka>
    {
        public static int MaxDubina;
        public static int MinDubina;

        public Cvor Korjen;
        public double GreskaJedinke;

        /// <summary>
        /// Generira novu, random jedinku.
        /// </summary>
        /// <param name="generirajDoKraja"></param>
        public Jedinka(bool generirajDoKraja = true)
        {
            if (generirajDoKraja)
            {
                Korjen = Cvor.SlucajniFunkcijskiCvor();
                Generiraj(Korjen, 1);
            }
        }

        private static void Generiraj(Cvor cvor, int dubina)
        {
            if (cvor.BrojDjece == 0)
                return;
            for (var i = 0; i < cvor.BrojDjece; i++)
            {
                if (dubina == MaxDubina)
                    cvor.DodajZavrsniCvor();
                else if (dubina < MinDubina)
                    Generiraj(cvor.DodajFunkcijskiCvor(), dubina + 1);
                else
                    Generiraj(cvor.DodajCvor(), dubina + 1);
            }
        }

        public double Izracunaj(List<double> varijable)
        {
            return Korjen.RekurzivnoIzracunaj(varijable);
        }

        public Jedinka Kopiraj()
        {
            var noviKorjen = Korjen.Kopiraj();
            Korjen.RekurzivnoKopiraj(noviKorjen);
            return new Jedinka (false)
            {
                Korjen = noviKorjen
            };
        }

        public XElement Serijaliziraj()
        {
            var jedinkaKaoXElement = new XElement("Jedinka");
            jedinkaKaoXElement.Add(new XAttribute("greska", Math.Round(GreskaJedinke, 4)));
            jedinkaKaoXElement.Value = ToString();

            return jedinkaKaoXElement;
        }

        public static Jedinka Deserijaliziraj(string xmlString)
        {
            var el = (string) XElement.Parse(xmlString);

            // pretvaranje niza znakova u niz cvorova
            var cvorovi = new List<Cvor>();
            foreach (var znak in el.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var t = StringEnum.GetValueFromDescription<TipFunkcije>(znak);
                    cvorovi.Add(new Cvor(t));

                }
                catch (Exception)
                {
                    // nije funkcijski cvor
                    if (znak.Contains('$'))
                    {
                        //varijabla
                        var i = int.Parse(znak.Remove(0, 1));
                        cvorovi.Add(new Cvor(i));
                    }
                    else
                    {
                        // konstanta
                        cvorovi.Add(new Cvor(false) {Vrijednost = double.Parse(znak)});
                    }
                }
            }

            // gradnja stabla iz niza cvorova
            var j = new Jedinka (false) {Korjen = cvorovi[0]};
            cvorovi.RemoveAt(0);
            DodajDjecu(j.Korjen, cvorovi);

            return j;
        }

        private static void DodajDjecu(Cvor cvor, List<Cvor> listaCvors)
        {
            // todo provijeriti deserijalizaciju
            if (cvor.BrojDjece == 0 || cvor.BrojDjece == cvor.Djeca.Count)
                return;

            for (var i = 0; i < cvor.BrojDjece; i++)
            {
                var dijete = listaCvors[0];
                listaCvors.RemoveAt(0);
                dijete.Roditelj = cvor;
                cvor.Djeca.Add(dijete);
                DodajDjecu(dijete, listaCvors);
            }
            
        }

        public void FiksirajKonstante()
        {
            FiksirajKonstante(Korjen);
        }

        private static void FiksirajKonstante(Cvor cvor)
        {
            if (cvor.Tip == TipCvora.Konstanta && double.IsNaN(cvor.Vrijednost))
                cvor.Vrijednost = RandomGenerator.GetNormal(0, 10);
            if (cvor.Djeca == null) return;
            foreach (var c in cvor.Djeca)
            {
                FiksirajKonstante(c);
            }
        }

        public int BrojCvorova()
        {
            return BrojCvorova(Korjen);   
        }

        private static int BrojCvorova(Cvor cvor)
        {
            if (cvor.BrojDjece == 0)
            {
                return 1;
            }
            return cvor.Djeca.Select(BrojCvorova).Sum() + 1;
        }

        public int CompareTo(Jedinka other)
        {
            return -String.Compare(ToString(), other.ToString(), StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return Korjen.ToString(true);
        }
    }
}
