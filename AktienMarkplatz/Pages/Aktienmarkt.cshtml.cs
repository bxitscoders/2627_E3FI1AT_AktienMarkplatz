using AktienMarkplatz.API;
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

        public async Task OnGetAsync(string symbol)
        {
            Symbol = symbol;

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                Antwort = await _connection.GetAktie(symbol);
            }
        }
    }
}
