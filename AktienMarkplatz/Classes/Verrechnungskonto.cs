using AktienMarkplatz.Models;

namespace AktienMarkplatz.Classes
{
    // Geldkonto eines Benutzers. Jeder Benutzer hat genau ein Verrechnungskonto,
    // von dem spaeter auch Aktienkaeufe bezahlt werden.
    public class Verrechnungskonto
    {
        public int Id { get; set; }

        // Id des Identity-Benutzers, dem das Konto gehoert.
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public decimal Kontostand { get; set; }

        // Gibt true zurueck, wenn der Betrag gutgeschrieben wurde.
        public bool Einzahlen(decimal betrag)
        {
            if (betrag <= 0) return false;
            Kontostand += betrag;
            return true;
        }

        // Fuer Aktienkaeufe: bucht nur ab, wenn genug Geld da ist.
        public bool Abheben(decimal betrag)
        {
            if (betrag <= 0) return false;
            if (Kontostand < betrag) return false;
            Kontostand -= betrag;
            return true;
        }
    }
}
