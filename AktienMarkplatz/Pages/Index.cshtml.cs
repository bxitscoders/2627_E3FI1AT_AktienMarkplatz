using AktienMarkplatz.API;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AktienMarkplatz.Pages
{
    public class IndexModel : PageModel
    {
        private readonly Connection _connection;

        public IndexModel()
        {
            _connection = new Connection();
        }

        public string ApiAntwort { get; private set; } = string.Empty;

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