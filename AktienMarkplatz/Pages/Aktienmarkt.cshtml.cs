using System.Linq;
using System.Threading.Tasks;
using AktienMarkplatz.API.Finnhub;
using AktienMarkplatz.API.Massive;
using AktienMarkplatz.Classes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AktienMarkplatz.Pages
{
    // Seite zum Suchen einer Aktie und Anzeigen ihres aktuellen Kurses.
    public class AktienmarktModel : PageModel
    {
        // Feste Auswahl bekannter Aktien fuer die "Groesste Aktien"-Uebersicht.
        private static readonly (string Ticker, string Name)[] BeispielAktien =
        {
            ("AAPL", "Apple Inc."),
            ("MSFT", "Microsoft Corp."),
            ("SAP", "SAP SE"),
            ("AMZN", "Amazon.com Inc."),
            ("NVDA", "NVIDIA Corp."),
        };

        private readonly FinnhubVerbindung _finnhub;
        private readonly MassiveVerbindung _massive;

        public AktienmarktModel(IConfiguration configuration)
        {
            string finnhubApiKey = configuration["FinnhubApi:ApiKey"] ?? string.Empty;
            _finnhub = new FinnhubVerbindung(finnhubApiKey);

            string massiveApiKey = configuration["MassiveApi:ApiKey"] ?? string.Empty;
            _massive = new MassiveVerbindung(massiveApiKey);
        }

        // Die gesuchte Aktie, null wenn nichts gefunden wurde.
        public Aktie? GefundeneAktie { get; private set; }

        // Kursverlauf der gesuchten Aktie, null wenn keine Aktie gefunden wurde.
        public Kursverlauf? Verlauf { get; private set; }

        public string Symbol { get; set; } = string.Empty;

        public Boerse Boerse { get; private set; } = new();

        public async Task OnGetAsync(string symbol, string? zeitraum)
        {
            Symbol = symbol;

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                GefundeneAktie = await _finnhub.AktieSuchen(symbol);

                if (GefundeneAktie is not null)
                {
                    Verlauf = await _massive.KursverlaufLaden(GefundeneAktie.Symbol, zeitraum);
                }
            }

            Aktie[] geladeneAktien = await Task.WhenAll(
                BeispielAktien.Select(beispiel => _finnhub.AktieLaden(beispiel.Ticker, beispiel.Name)));

            foreach (Aktie aktie in geladeneAktien)
            {
                Boerse.Hinzufuegen(aktie);
            }
        }
    }
}
