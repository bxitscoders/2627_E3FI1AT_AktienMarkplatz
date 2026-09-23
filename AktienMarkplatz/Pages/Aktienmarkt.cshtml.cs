using System.Linq;
using System.Threading.Tasks;
using AktienMarkplatz.API;
using AktienMarkplatz.Classes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AktienMarkplatz.Pages
{
    // Seite zum Suchen einer Aktie und Anzeigen ihrer Dividenden.
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

        private readonly Connection _connection;

        public AktienmarktModel(IConfiguration configuration)
        {
            string apiKey = configuration["StockApi:ApiKey"] ?? string.Empty;
            _connection = new Connection(apiKey);
        }

        public DividendenAntwort? Antwort { get; private set; }

        public string Symbol { get; set; } = string.Empty;

        public Boerse Boerse { get; private set; } = new();

        public async Task OnGetAsync(string symbol)
        {
            Symbol = symbol;

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                Antwort = await _connection.DividendenSuchen(symbol);
            }

            Aktie[] geladeneAktien = await Task.WhenAll(
                BeispielAktien.Select(beispiel => _connection.AktieLaden(beispiel.Ticker, beispiel.Name)));

            foreach (Aktie aktie in geladeneAktien)
            {
                Boerse.Hinzufuegen(aktie);
            }
        }
    }
}
