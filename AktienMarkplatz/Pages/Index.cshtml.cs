using AktienMarkplatz.API;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.Common;

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

        public async Task OnGetAsync()
        {
            ApiAntwort = await _connection.GetAktie("MSFT");
        }
    }
}