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
            Zeitraum = ZeitraumPruefen(zeitraum);
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

        // Unbekannte Zeitraeume (z. B. aus der URL) werden zu "1M".
        public static string ZeitraumPruefen(string? zeitraum)
        {
            if (zeitraum != null && Zeitraeume.Contains(zeitraum))
                return zeitraum;

            return "1M";
        }

        // Ab welchem Tag die Kurse geladen werden.
        public static DateTime StartDatum(string zeitraum, DateTime heute)
        {
            switch (ZeitraumPruefen(zeitraum))
            {
                case "1W": return heute.AddDays(-7);
                case "6M": return heute.AddMonths(-6);
                case "1J": return heute.AddYears(-1);
                default: return heute.AddMonths(-1);
            }
        }
    }
}
