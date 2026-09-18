namespace AktienMarkplatz.Classes
{
    public class Position
    {
        public Aktie Aktie { get; set; }
        public int Anzahl { get; set; }

        public double DurchschnittlicherKaufpreis { get; set; }


        public Position(Aktie aktie, int anzahl, double kaufpreis)
        {
            Aktie = aktie;
            Anzahl = anzahl;
            DurchschnittlicherKaufpreis = kaufpreis;
        }

        public void AnzahlErhoehen(int menge, double kaufpreis)
        {
            var gesamtwertAlt = Anzahl * DurchschnittlicherKaufpreis;
            var gesamtwertNeu = menge * kaufpreis;

            Anzahl += menge;
            DurchschnittlicherKaufpreis = (gesamtwertAlt + gesamtwertNeu) / Anzahl;
        }

        public bool AnzahlVerringern(int menge)
        {
            if (menge > Anzahl)
                return false;

            Anzahl -= menge;
            return true;
        }

        public double GetAktuellerWert() => Anzahl * Aktie.AktuellerKurs;

    }
}
