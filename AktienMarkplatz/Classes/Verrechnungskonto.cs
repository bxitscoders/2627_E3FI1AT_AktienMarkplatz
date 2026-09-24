using AktienMarkplatz.Data;
using AktienMarkplatz.Models;
using Microsoft.EntityFrameworkCore;

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

        // Geld aufs Konto. Gibt true zurueck, wenn der Betrag gutgeschrieben wurde.
        public bool Einzahlen(decimal betrag)
        {
            if (betrag <= 0) return false;
            Kontostand += betrag;
            return true;
        }

        // Schaut nciht negativ 
        public bool Auszahlen(decimal betrag)
        {
            if (betrag <= 0) return false;
            if (Kontostand < betrag) return false;
            Kontostand -= betrag;
            return true;
        }

        // Holt das Konto des Benutzers aus der Datenbank und legt es beim ersten Mal an.
        public static async Task<Verrechnungskonto> LadenAsync(ApplicationDbContext db, string userId)
        {
            Verrechnungskonto? konto = await db.Verrechnungskonten.FirstOrDefaultAsync(k => k.UserId == userId);
            if (konto == null)
            {
                konto = new Verrechnungskonto { UserId = userId };
                await konto.SaveAsync(db);
            }
            return konto;
        }

        // Schreibt das Konto in die Datenbank. Neue Konten (Id 0) werden dabei angelegt.
        public async Task SaveAsync(ApplicationDbContext db)
        {
            if (Id == 0)
            {
                db.Verrechnungskonten.Add(this);
            }
            await db.SaveChangesAsync();
        }
    }
}
