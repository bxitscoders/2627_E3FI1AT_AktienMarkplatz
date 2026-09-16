using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AktienMarkplatz.API
{
    public class Connection
    {
        // Ein HttpClient wird für die ganze App wiederverwendet (nicht pro Aufruf neu erzeugt).
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly string _apiKey;

        public Connection(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<DividendApiResponse?> GetAktie(string symbol)
        {
            string url = $"https://api.massive.com/v3/reference/dividends?ticker={symbol}&apikey={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DividendApiResponse>();
        }
    }
}
