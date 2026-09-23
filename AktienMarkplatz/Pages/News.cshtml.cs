using AktienMarkplatz.API;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AktienMarkplatz.Pages
{
    // Seite zum Anzeigen von News-Einträgen (zwei pro Reihe, max. 30 Einträge)
    public class NewsModel : PageModel
    {
        private readonly Connection _connection;

        public NewsModel(IConfiguration configuration)
        {
            string apiKey = configuration["StockApi:ApiKey"] ?? string.Empty;
            _connection = new Connection(apiKey);
        }

        public NewsSucheAntwort? Antwort { get; private set; }

        // Ausgewählte Sortierreihenfolge ("desc" oder "asc").
        public string SelectedOrder { get; private set; } = "desc";

        public async Task OnGetAsync(string? order)
        {
            // Bestimme Sortierreihenfolge (standard: neueste zuerst)
            SelectedOrder = string.IsNullOrWhiteSpace(order) ? "desc" : order;

            // Hole bis zu 30 News, mit gewählter Reihenfolge
            Antwort = await _connection.NewsLaden(30, SelectedOrder);
        }
    }
}
