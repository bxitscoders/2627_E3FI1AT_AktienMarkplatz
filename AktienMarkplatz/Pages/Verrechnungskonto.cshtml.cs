using System.Security.Claims;
using AktienMarkplatz.Classes;
using AktienMarkplatz.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AktienMarkplatz.Pages
{
    // Zeigt den Kontostand des eingeloggten Benutzers und erlaubt Einzahlungen.
    [Authorize]
    public class VerrechnungskontoModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public VerrechnungskontoModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public decimal Kontostand { get; private set; }

        [BindProperty]
        public decimal Betrag { get; set; }

        // TempData, damit die Meldung den Redirect nach dem Einzahlen ueberlebt.
        [TempData]
        public string? Meldung { get; set; }

        public async Task OnGetAsync()
        {
            Verrechnungskonto konto = await Verrechnungskonto.LadenAsync(_db, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Kontostand = konto.Kontostand;
        }

        public async Task<IActionResult> OnPostEinzahlenAsync()
        {
            Verrechnungskonto konto = await Verrechnungskonto.LadenAsync(_db, User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (konto.Einzahlen(Betrag))
            {
                await konto.SaveAsync(_db);
                Meldung = $"{Betrag:C} wurden eingezahlt.";
            }
            else
            {
                Meldung = "Bitte einen Betrag größer als 0 eingeben.";
            }

            // Redirect, damit ein Neuladen der Seite nicht nochmal einzahlt.
            return RedirectToPage();
        }
    }
}
