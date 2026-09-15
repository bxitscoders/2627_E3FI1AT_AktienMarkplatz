using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Text;

namespace AktienMarkplatz.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string DbInhalt { get; private set; } = string.Empty;

        public void OnGet()
        {
            var connectionString = _configuration.GetConnectionString("AktienMarkplatzDb");

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name FROM Test ORDER BY Id";

            var sb = new StringBuilder();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var name = reader.GetString(1);
                sb.AppendLine($"{id}: {name}");
            }

            DbInhalt = sb.Length > 0 ? sb.ToString() : "Keine Daten gefunden.";
        }
    }
}
