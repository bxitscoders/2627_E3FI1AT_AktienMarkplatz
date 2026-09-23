using System.Collections.Generic;

namespace AktienMarkplatz.Classes
{
    public class Boerse
    {
        private List<Aktie> Aktien { get; set; } = new();

        public void Hinzufuegen(Aktie aktie)
        {
            Aktien.Add(aktie);
        }

        // Alle Aktien der Boerse in der Reihenfolge, in der sie hinzugefuegt wurden.
        public List<Aktie> GroessteAktien()
        {
            return Aktien;
        }
    }
}
