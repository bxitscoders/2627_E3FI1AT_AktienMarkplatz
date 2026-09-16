using AktienMarkplatz.API;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AktienMarkplatz.Pages
{
    public class IndexModel : PageModel
    {
        private readonly Connection _connection;

        public IndexModel(IConfiguration configuration)
        {
            string apiKey = configuration["StockApi:ApiKey"] ?? string.Empty;
            _connection = new Connection(apiKey);
        }

        public DividendApiResponse? ApiAntwort { get; private set; }

        public string Symbol { get; set; } = string.Empty;

        public async Task OnGetAsync(string symbol)
        {
            Symbol = symbol;

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                ApiAntwort = await _connection.GetAktie(symbol.ToUpper());
            }
        }
    }
}
