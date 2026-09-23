using System;

namespace AktienMarkplatz.Classes
{
    public class Aktie
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double AktuellerKurs { get; set; }
        public double VeraenderungHeuteProzent { get; private set; }

        public Aktie(string symbol, string name, double startkurs)
        {
            Symbol = symbol;
            Name = name;
            AktuellerKurs = startkurs;
        }

        public void KursAktualisieren(double neuerKurs)
        {
            if (neuerKurs <= 0)
                throw new ArgumentException("Der Kurs darf nicht negativ oder null sein.");

            AktuellerKurs = neuerKurs;
        }

        // Setzt den Kurs auf den Schlusskurs und berechnet daraus die Veraenderung
        // gegenueber dem Eroeffnungskurs in Prozent.
        public void HandelstagFestlegen(double eroeffnung, double schluss)
        {
            KursAktualisieren(schluss);
            VeraenderungHeuteProzent = (schluss - eroeffnung) / eroeffnung * 100;
        }

        public bool Steigt()
        {
            if (VeraenderungHeuteProzent >= 0)
                return true;

            return false;
        }

        // Liefert z. B. "veraenderung--gewinn" oder "veraenderung--verlust" fuer die CSS-Klasse.
        public string VeraenderungCssKlasse()
        {
            if (Steigt())
                return "veraenderung--gewinn";

            return "veraenderung--verlust";
        }

        // Liefert z. B. "▲ 1,2 %" oder "▼ 0,5 %".
        public string KursAenderung()
        {
            double prozent = Math.Abs(VeraenderungHeuteProzent);
            return prozent.ToString("0.0") + " %";
        }
    }
}
