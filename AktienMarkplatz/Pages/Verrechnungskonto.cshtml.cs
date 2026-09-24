using AktienMarkplatz.Classes;
using AktienMarkplatz.Data;
using AktienMarkplatz.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AktienMarkplatz.Pages
{
    // Zeigt den Kontostand des eingeloggten Benutzers und erlaubt Einzahlungen.
    [Authorize]
    public class VerrechnungskontoModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public VerrechnungskontoModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public decimal Kontostand { get; private set; }

        [BindProperty]
        public decimal Betrag { get; set; }

        // TempData, damit die Meldung den Redirect nach dem Einzahlen ueberlebt.
        [TempData]
        public string? Meldung { get; set; }

        public async Task OnGetAsync()
        {
            Verrechnungskonto konto = await KontoLadenAsync();
            Kontostand = konto.Kontostand;
        }

        public async Task<IActionResult> OnPostEinzahlenAsync()
        {
            Verrechnungskonto konto = await KontoLadenAsync();

            if (konto.Einzahlen(Betrag))
            {
                await _db.SaveChangesAsync();
                Meldung = $"{Betrag:C} wurden eingezahlt.";
            }
            else
            {
                Meldung = "Bitte einen Betrag größer als 0 eingeben.";
            }

            // Redirect, damit ein Neuladen der Seite nicht nochmal einzahlt.
            return RedirectToPage();
        }

        // Holt das Konto des eingeloggten Benutzers und legt es beim ersten Aufruf an.
        private async Task<Verrechnungskonto> KontoLadenAsync()
        {
            string userId = _userManager.GetUserId(User)!;

            Verrechnungskonto? konto = await _db.Verrechnungskonten.FirstOrDefaultAsync(k => k.UserId == userId);
            if (konto == null)
            {
                konto = new Verrechnungskonto { UserId = userId };
                _db.Verrechnungskonten.Add(konto);
                await _db.SaveChangesAsync();
            }

            return konto;
        }
    }
}
