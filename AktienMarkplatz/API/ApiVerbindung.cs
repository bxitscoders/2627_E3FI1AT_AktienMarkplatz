using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AktienMarkplatz.API
{
    // Gemeinsame Basisklasse fuer alle API-Verbindungen (FinnhubVerbindung und MassiveVerbindung).
    // Enthaelt den HttpClient und die Methode, die einen GET-Aufruf ausfuehrt.
    public abstract class ApiVerbindung
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        protected readonly string _apiKey;

        protected ApiVerbindung(string apiKey)
        {
            _apiKey = apiKey;
        }

        // Fuehrt einen GET-Aufruf aus. Ist die API nicht erreichbar oder das Anfragelimit
        // erreicht (z. B. 429), kommt null zurueck, statt dass die Seite abstuerzt.
        protected async Task<T?> AbfrageAusfuehren<T>(string url) where T : class
        {
            try
            {
                HttpResponseMessage antwort = await _httpClient.GetAsync(url);
                antwort.EnsureSuccessStatusCode();

                return await antwort.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
