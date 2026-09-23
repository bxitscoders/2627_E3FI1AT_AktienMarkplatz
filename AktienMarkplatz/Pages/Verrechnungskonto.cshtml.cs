using AktienMarkplatz.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AktienMarkplatz.Pages
{
    // Übersicht über das eigene Konto und das zentrale Verrechnungskonto.
    public class VerrechnungskontoModel : PageModel
    {
        // Für dieses Beispiel verwenden wir eine einfache Test-User-ID.
        private const string TestUserId = "local-test-user";

        public Konto? MeinKonto { get; private set; }
        public Konto Verrechnungskonto => KontoManager.Verrechnungskonto;
        [BindProperty]
        public decimal Betrag { get; set; }

        // Ausgewählte Transfer-Richtung (verrechnung_to_me oder me_to_verrechnung)
        [BindProperty]
        public string TransferDirection { get; set; } = "verrechnung_to_me";

        // Gebühren-Satz für Überweisungen vom Verrechnungskonto (z.B. 2% = 0.02m)
        private const decimal FeePercent = 0.02m;

        // Öffentliches Readonly-Feld für die Anzeige / JS
        public decimal FeePercentPublic => FeePercent;

        public string? Message { get; private set; }

        public void OnGet()
        {
            MeinKonto = KontoManager.GetOrCreateUserKonto(TestUserId);
        }

        // Allgemeiner Transfer-Handler: erlaubt Überweisungen in beide Richtungen.
        public IActionResult OnPostTransfer()
        {
            if (Betrag <= 0)
            {
                Message = "Bitte einen Betrag größer als 0 eingeben.";
                MeinKonto = KontoManager.GetOrCreateUserKonto(TestUserId);
                return Page();
            }

            // Bestimme Quelle und Ziel anhand der Richtung
            if (TransferDirection == "verrechnung_to_me")
            {
                // Transfer vom Verrechnungskonto zum eigenen Konto (mit Gebühr)
                try
                {
                    var verrechnung = KontoManager.Verrechnungskonto;
                    var mein = KontoManager.GetOrCreateUserKonto(TestUserId);
                    decimal beforeVerrechnung = verrechnung.Kontostand;
                    decimal beforeMein = mein.Kontostand;

                    if (beforeVerrechnung < Betrag)
                    {
                        Message = $"Überweisung fehlgeschlagen: Verrechnungskonto hat nur {beforeVerrechnung:C} (benötigt {Betrag:C}).";
                    }
                    else
                    {
                        decimal fee = decimal.Round(Betrag * FeePercent, 2);
                        decimal receive = Betrag - fee;
                        bool ok = KontoManager.TransferWithFee("__verrechnung__", TestUserId, Betrag, FeePercent);
                        var afterVerrechnung = verrechnung.Kontostand;
                        var afterMein = mein.Kontostand;
                        if (ok)
                        {
                            Message = $"{Betrag:C} erfolgreich überwiesen. Gebühr: {fee:C}. Empfänger erhält: {receive:C}.\n" +
                                      $"Kontostände: Verrechnung {beforeVerrechnung:C} → {afterVerrechnung:C}, Mein {beforeMein:C} → {afterMein:C}.";
                        }
                        else
                        {
                            Message = "Überweisung fehlgeschlagen (Fehler beim Ausführen).";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Message = "Fehler beim Überweisen: " + ex.Message;
                }
            }
            else
            {
                // Transfer vom eigenen Konto ins Verrechnungskonto (ohne Gebühr)
                var mein = KontoManager.GetOrCreateUserKonto(TestUserId);
                if (mein.Kontostand < Betrag)
                {
                    Message = $"Überweisung fehlgeschlagen: Dein Konto hat nur {mein.Kontostand:C} (benötigt {Betrag:C}).";
                }
                else
                {
                    bool ok = KontoManager.TransferWithFee(TestUserId, "__verrechnung__", Betrag, 0m);
                    Message = ok ? $"{Betrag:C} erfolgreich an das Verrechnungskonto überwiesen." : "Überweisung fehlgeschlagen (Fehler beim Ausführen).";
                }
            }

            // Aktualisiere Kontostände
            MeinKonto = KontoManager.GetOrCreateUserKonto(TestUserId);
            return Page();
        }

        // Einzahlen (Load) auf das ausgewählte Konto (separater Button/Formular)
        public IActionResult OnPostLoad()
        {
            if (Betrag <= 0)
            {
                Message = "Bitte einen Betrag größer als 0 eingeben.";
                MeinKonto = KontoManager.GetOrCreateUserKonto(TestUserId);
                return Page();
            }
            // Einzahlen (Load) auf das Ziel, bestimmt durch die Richtung
            bool ok;
            if (TransferDirection == "verrechnung_to_me")
            {
                // Load auf mein Konto (z.B. externe Einzahlung)
                var konto = KontoManager.GetOrCreateUserKonto(TestUserId);
                ok = konto.Einzahlen(Betrag);
                Message = ok ? $"{Betrag:C} erfolgreich auf dein Konto eingezahlt." : "Einzahlung fehlgeschlagen.";
            }
            else
            {
                // me_to_verrechnung: Load auf das Verrechnungskonto
                ok = KontoManager.FundVerrechnungskonto(Betrag);
                Message = ok ? $"{Betrag:C} erfolgreich dem Verrechnungskonto gutgeschrieben." : "Fehler beim Aufstocken des Verrechnungskontos.";
            }

            // Aktualisiere Kontostand
            MeinKonto = KontoManager.GetOrCreateUserKonto(TestUserId);
            return Page();
        }
    }
}
