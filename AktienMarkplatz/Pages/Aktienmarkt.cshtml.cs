using AktienMarkplatz.API;
using AktienMarkplatz.Classes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AktienMarkplatz.Pages
{
    // Seite zum Suchen einer Aktie und Anzeigen ihrer Dividenden.
    public class AktienmarktModel : PageModel
    {
        private readonly Connection _connection;

        public AktienmarktModel(IConfiguration configuration)
        {
            string apiKey = configuration["StockApi:ApiKey"] ?? string.Empty;
            _connection = new Connection(apiKey);
        }

        public DividendenAntwort? Antwort { get; private set; }

        public string Symbol { get; set; } = string.Empty;

        public List<(Aktie Aktie, double VeraenderungProzent)> GroessteAktien { get; private set; } = new();

        public async Task OnGetAsync(string symbol)
        {
            Symbol = symbol;

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                Antwort = await _connection.GetAktie(symbol);
            }

            GroessteAktien = new List<(Aktie, double)>
            {
                (new Aktie("AAPL", "Apple Inc.", 227.50), 1.2),
                (new Aktie("MSFT", "Microsoft Corp.", 415.20), 0.8),
                (new Aktie("SAP", "SAP SE", 198.10), -0.5),
                (new Aktie("AMZN", "Amazon.com Inc.", 178.90), 2.1),
                (new Aktie("NVDA", "NVIDIA Corp.", 121.30), -1.4),
            };
        }
    }
}
