namespace AktienMarkplatz.Classes
{
    public class Konto
    {
        // Kontostand in Dezimal für Geld.
        private readonly object _lock = new object();

        public decimal Kontostand { get; private set; }

        // Optionaler Besitzer-Identifikator (z.B. Benutzername oder ID)
        public string OwnerId { get; }

        public Konto(string ownerId, decimal startbetrag = 0m)
        {
            OwnerId = ownerId;
            Kontostand = startbetrag;
        }

        // Einfaches Einzahlen. Gibt true zurück, wenn erfolgreich.
        public bool Einzahlen(decimal betrag)
        {
            if (betrag <= 0) return false;
            lock (_lock)
            {
                Kontostand += betrag;
                return true;
            }
        }

        // Abheben, nur wenn ausreichend Guthaben vorhanden ist.
        public bool Abheben(decimal betrag)
        {
            if (betrag <= 0) return false;
            lock (_lock)
            {
                if (Kontostand < betrag) return false;
                Kontostand -= betrag;
                return true;
            }
        }

        // Transfer zu einem anderen Konto. Vermeidet Deadlocks durch
        // festgelegte Lock-Reihenfolge (OwnerId Vergleich).
        public bool TransferTo(Konto ziel, decimal betrag)
        {
            if (ziel == null) return false;
            if (betrag <= 0) return false;

            // Bestimme Lock-Reihenfolge
            var first = this;
            var second = ziel;
            if (string.CompareOrdinal(this.OwnerId, ziel.OwnerId) > 0)
            {
                first = ziel;
                second = this;
            }

            lock (first._lock)
            {
                lock (second._lock)
                {
                    if (Kontostand < betrag) return false;
                    Kontostand -= betrag;
                    ziel.Kontostand += betrag;
                    return true;
                }
            }
        }

        // Transfer mit Gebühr: Die Gebühr wird vom Betrag abgezogen und nicht weiterverbucht.
        public bool TransferToWithFee(Konto ziel, decimal betrag, decimal feePercent)
        {
            if (ziel == null) return false;
            if (betrag <= 0) return false;

            var first = this;
            var second = ziel;
            if (string.CompareOrdinal(this.OwnerId, ziel.OwnerId) > 0)
            {
                first = ziel;
                second = this;
            }

            lock (first._lock)
            {
                lock (second._lock)
                {
                    if (Kontostand < betrag) return false;

                    decimal fee = decimal.Round(betrag * feePercent, 2);
                    decimal amountAfterFee = betrag - fee;

                    Kontostand -= betrag;
                    ziel.Kontostand += amountAfterFee;
                    return true;
                }
            }
        }
    }

    // Manager für Benutzerkonten und das zentrale Verrechnungskonto.
    // Diese Klasse ist bewusst einfach gehalten und kann später erweitert
    // werden (z.B. Persistenz, mehrere Währungen, Benutzerverwaltung).
    public static class KontoManager
    {
        private static readonly object _sync = new object();
        private static readonly Dictionary<string, Konto> _konten = new();

        // Zentrales Verrechnungskonto (z.B. für Tests / interne Transfers)
        private static readonly Konto _verrechnung = new Konto("__verrechnung__", 1000m);

        public static Konto Verrechnungskonto => _verrechnung;

        // Liefert ein Konto für einen Benutzer (erstellt, wenn nicht vorhanden).
        public static Konto GetOrCreateUserKonto(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) userId = "guest";
            lock (_sync)
            {
                if (!_konten.TryGetValue(userId, out var konto))
                {
                    konto = new Konto(userId, 0m);
                    _konten[userId] = konto;
                }
                return konto;
            }
        }

        // Gibt Testgeld vom Verrechnungskonto an den Benutzer (true wenn erfolgreich)
        public static bool GiveTestMoney(string userId, decimal betrag)
        {
            if (betrag <= 0) return false;
            var ziel = GetOrCreateUserKonto(userId);
            // Transfer vom Verrechnungskonto zum Benutzer
            return _verrechnung.TransferTo(ziel, betrag);
        }

        // Erlaube Aufstockung des Verrechnungskontos (z.B. Admin)
        public static bool FundVerrechnungskonto(decimal betrag)
        {
            return _verrechnung.Einzahlen(betrag);
        }

        // Für Debugging / Anzeige
        public static IReadOnlyCollection<Konto> GetAllUserKonten()
        {
            lock (_sync) return _konten.Values.ToList().AsReadOnly();
        }

        // Transfer zwischen zwei Konten mit optionaler Gebühr (als Dezimal, z.B. 0.02m = 2%).
        // Die Gebühr wird vom Betrag abgezogen und nicht weiterverbucht.
        public static bool TransferWithFee(string fromId, string toId, decimal amount, decimal feePercent = 0m)
        {
            if (amount <= 0) return false;

            Konto from = fromId == "__verrechnung__" ? _verrechnung : GetOrCreateUserKonto(fromId);
            Konto to = toId == "__verrechnung__" ? _verrechnung : GetOrCreateUserKonto(toId);

            // Delegate to Konto method which handles locking
            return from.TransferToWithFee(to, amount, feePercent);
        }
    }
}
