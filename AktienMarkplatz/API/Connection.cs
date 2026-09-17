using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AktienMarkplatz.API
{
    // Verbindung zur Aktien-API. Sucht zuerst den passenden Ticker zum Suchbegriff
    // und holt danach die Dividenden zu diesem Ticker.
    public class Connection
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly string _apiKey;

        public Connection(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<DividendenAntwort?> GetAktie(string suchbegriff)
        {
            string? ticker = await TickerSuchen(suchbegriff);

            if (ticker is null)
            {
                return null;
            }

            string url = $"https://api.massive.com/v3/reference/dividends?ticker={ticker}&apikey={_apiKey}";

            HttpResponseMessage antwort = await _httpClient.GetAsync(url);
            antwort.EnsureSuccessStatusCode();

            return await antwort.Content.ReadFromJsonAsync<DividendenAntwort>();
        }

        // Sucht zu einem Firmennamen oder Tickersymbol den passenden Ticker,
        // z. B. "ServiceNow" -> "NOW".
        private async Task<string?> TickerSuchen(string suchbegriff)
        {
            string suchbegriffCodiert = WebUtility.UrlEncode(suchbegriff);
            string url = $"https://api.massive.com/v3/reference/tickers?search={suchbegriffCodiert}&apikey={_apiKey}";

            HttpResponseMessage antwort = await _httpClient.GetAsync(url);
            antwort.EnsureSuccessStatusCode();

            TickerSucheAntwort? ergebnis = await antwort.Content.ReadFromJsonAsync<TickerSucheAntwort>();

            if (ergebnis is null)
            {
                return null;
            }

            // Nur echte Aktien von der normalen Börse anschauen, keine OTC-Pennystocks
            // und keine ETFs. Sonst findet man bei "Apple" z. B. zuerst "Apple iSports
            // Group" statt "Apple Inc.".
            List<TickerTreffer> aktien = ergebnis.Ergebnisse
                .Where(treffer => treffer.Markt == "stocks" && treffer.Typ == "CS")
                .ToList();

            // Am liebsten die Aktie nehmen, deren Name mit dem Suchbegriff anfängt.
            foreach (TickerTreffer aktie in aktien)
            {
                if (aktie.Name != null && aktie.Name.StartsWith(suchbegriff, StringComparison.OrdinalIgnoreCase))
                {
                    return aktie.Tickersymbol;
                }
            }

            // Sonst einfach die erste passende Aktie nehmen.
            return aktien.FirstOrDefault()?.Tickersymbol;
        }
    }
}
