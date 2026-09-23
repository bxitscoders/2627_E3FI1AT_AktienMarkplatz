using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AktienMarkplatz.Classes;

namespace AktienMarkplatz.API
{
    // Verbindung zur Aktien-API. Sucht zuerst den passenden Ticker zum Suchbegriff
    // und holt danach die Dividenden zu diesem Ticker.
    public class Connection
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // AktieLaden wird bei jedem Seitenaufruf fuer mehrere Ticker gleichzeitig
        // aufgerufen. Ohne Cache wuerde das schnell das Anfragelimit der API sprengen,
        // deshalb werden geladene Aktien fuer eine Weile wiederverwendet.
        private static readonly ConcurrentDictionary<string, (Aktie Aktie, DateTime GeladenAm)> _aktienCache = new();
        private static readonly TimeSpan _cacheDauer = TimeSpan.FromMinutes(10);

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

            return await DividendenLaden(ticker);
        }

        // Laedt den Kurs zu einem bereits bekannten Ticker (kein Ticker-Search noetig).
        // Pro Aktie nur eine Anfrage, damit fuer die Suche noch genug vom Anfragelimit
        // uebrig bleibt. Ergebnisse werden fuer ein paar Minuten zwischengespeichert.
        public async Task<Aktie> AktieLaden(string ticker, string name)
        {
            if (_aktienCache.TryGetValue(ticker, out var eintrag) && DateTime.UtcNow - eintrag.GeladenAm < _cacheDauer)
            {
                return eintrag.Aktie;
            }

            var aktie = new Aktie(ticker, name, 1);

            Handelstag? handelstag = await LetzterHandelstagLaden(ticker);
            if (handelstag is not null)
            {
                aktie.HandelstagFestlegen(handelstag.Eroeffnung, handelstag.Schluss);

                // Nur erfolgreich geladene Aktien merken, sonst wird beim naechsten
                // Seitenaufruf erneut versucht zu laden.
                _aktienCache[ticker] = (aktie, DateTime.UtcNow);
            }

            return aktie;
        }

        private Task<DividendenAntwort?> DividendenLaden(string ticker)
        {
            string url = $"https://api.massive.com/v3/reference/dividends?ticker={ticker}&apikey={_apiKey}";
            return AbfrageAusfuehren<DividendenAntwort>(url);
        }

        private async Task<Handelstag?> LetzterHandelstagLaden(string ticker)
        {
            string url = $"https://api.massive.com/v2/aggs/ticker/{ticker}/prev?apikey={_apiKey}";
            HandelstagAntwort? ergebnis = await AbfrageAusfuehren<HandelstagAntwort>(url);
            return ergebnis?.Ergebnisse.FirstOrDefault();
        }

        // Fuehrt einen GET-Aufruf gegen die API aus. Wenn die API voruebergehend nicht
        // erreichbar ist oder das Anfragelimit erreicht wurde (z. B. 429), wird null
        // zurueckgegeben, statt die ganze Seite mit einer Exception abstuerzen zu lassen.
        private async Task<T?> AbfrageAusfuehren<T>(string url) where T : class
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

        // Sucht zu einem Firmennamen oder Tickersymbol den passenden Ticker,
        // z. B. "ServiceNow" -> "NOW".
        private async Task<string?> TickerSuchen(string suchbegriff)
        {
            string suchbegriffCodiert = WebUtility.UrlEncode(suchbegriff);
            string url = $"https://api.massive.com/v3/reference/tickers?search={suchbegriffCodiert}&apikey={_apiKey}";

            TickerSucheAntwort? ergebnis = await AbfrageAusfuehren<TickerSucheAntwort>(url);

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

        // Holt die aktuellsten News-Einträge (Standard: aufsteigend, 10 Einträge,
        // sortiert nach published_utc). Verwendet denselben API-Key wie die
        // anderen Methoden.
        public async Task<NewsSucheAntwort?> GetNews(int limit = 10, string order = "desc")
        {
            if (limit < 1) limit = 1;
            if (limit > 100) limit = 100; // API-safety upper bound

            // order: "desc" = neueste zuerst, "asc" = älteste zuerst
            string orderParam = string.IsNullOrWhiteSpace(order) ? "desc" : order;
            string url = $"https://api.massive.com/v2/reference/news?order={orderParam}&limit={limit}&sort=published_utc&apiKey={_apiKey}";

            HttpResponseMessage antwort = await _httpClient.GetAsync(url);
            antwort.EnsureSuccessStatusCode();

            return await antwort.Content.ReadFromJsonAsync<NewsSucheAntwort>();
        }
    }

}
