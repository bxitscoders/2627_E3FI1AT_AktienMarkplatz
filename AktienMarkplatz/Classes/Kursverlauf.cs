using System;
using System.Collections.Generic;
using System.Linq;

namespace AktienMarkplatz.Classes
{
    // Werte ueber einen Zeitraum, z. B. die Tageskurse einer Aktie oder spaeter der Depotwert.
    // Wird vom Partial _Kursdiagramm als Liniendiagramm angezeigt.
    public class Kursverlauf
    {
        public static readonly string[] Zeitraeume = { "1W", "1M", "6M", "1J" };

        public string Titel { get; }
        public string Zeitraum { get; }
        public List<Kurspunkt> Punkte { get; } = new();

        public Kursverlauf(string titel, string? zeitraum)
        {
            Titel = titel;

            // Unbekannte Zeitraeume (z. B. aus der URL) werden zu "1M".
            if (zeitraum != null && Zeitraeume.Contains(zeitraum))
                Zeitraum = zeitraum;
            else
                Zeitraum = "1M";
        }

        public void PunktHinzufuegen(DateTime datum, double wert)
        {
            Punkte.Add(new Kurspunkt(datum, wert));
        }

        public bool IstLeer()
        {
            return Punkte.Count == 0;
        }

        public double Hoechst()
        {
            if (IstLeer()) return 0;
            return Punkte.Max(punkt => punkt.Wert);
        }

        public double Tiefst()
        {
            if (IstLeer()) return 0;
            return Punkte.Min(punkt => punkt.Wert);
        }

        // Veraenderung vom ersten zum letzten Punkt in Prozent.
        public double VeraenderungProzent()
        {
            if (Punkte.Count < 2) return 0;

            double erster = Punkte[0].Wert;
            double letzter = Punkte[^1].Wert;
            if (erster == 0) return 0;

            return (letzter - erster) / erster * 100;
        }

        public bool Steigt()
        {
            return VeraenderungProzent() >= 0;
        }

        public string VeraenderungText()
        {
            return Math.Abs(VeraenderungProzent()).ToString("0.0") + " %";
        }

        public string VeraenderungCssKlasse()
        {
            if (Steigt())
                return "veraenderung--gewinn";

            return "veraenderung--verlust";
        }

        // Ab welchem Tag die Kurse fuer den Zeitraum geladen werden.
        public DateTime StartDatum()
        {
            switch (Zeitraum)
            {
                case "1W": return DateTime.Today.AddDays(-7);
                case "6M": return DateTime.Today.AddMonths(-6);
                case "1J": return DateTime.Today.AddYears(-1);
                default: return DateTime.Today.AddMonths(-1);
            }
        }
    }
}
