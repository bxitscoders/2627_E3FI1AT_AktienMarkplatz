namespace AktienMarkplatz.Classes
{
    public class Aktie
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double AktuellerKurs { get; set; }

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
    }
}
