using System.Net.Http;
using System.Threading.Tasks;

namespace AktienMarkplatz.API
{
    public class Connection
    {
        private readonly HttpClient _httpClient;

        private const string ApiKey = "1DFs7Kd8n_tXP0ZiH3xYKv6Ymya7XvnN";

        public Connection()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAktie(string symbol)
        {

            string url =
                $"https://api.massive.com/v3/reference/dividends?&apikey={ApiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            return json;
        }

    }
}



// ApiKey = "PVCI1JJ9R32DQF2F";







