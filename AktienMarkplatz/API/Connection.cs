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
    // Verbindung zur Aktien-API (massive.com). Liefert Dividenden, Kurse und News.
    public class Connection
    {
        private const string BasisUrl = "https://api.massive.com";

        private static readonly HttpClient _httpClient = new HttpClient();

        // Geladene Kurse werden eine Weile wiederverwendet, damit nicht bei jedem
        // Seitenaufruf das Anfragelimit der API aufgebraucht wird.
        private static readonly ConcurrentDictionary<string, (Aktie Aktie, DateTime GeladenAm)> _aktienCache = new();
        private static readonly TimeSpan _cacheDauer = TimeSpan.FromMinutes(10);

        private readonly string _apiKey;

        public Connection(string apiKey)
        {
            _apiKey = apiKey;
        }

        // ---------- Oeffentliche Methoden ----------

        // Sucht zum Suchbegriff (Firmenname oder Ticker) die passende Aktie
        // und liefert deren Dividenden.
        public async Task<DividendenAntwort?> GetAktie(string suchbegriff)
        {
            string? ticker = await TickerSuchen(suchbegriff);

            if (ticker is null)
            {
                return null;
            }

            return await DividendenLaden(ticker);
        }

        // Laedt den Kurs zu einem bekannten Ticker. Nur erfolgreich geladene Aktien
        // kommen in den Cache, sonst wird beim naechsten Seitenaufruf erneut geladen.
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
                _aktienCache[ticker] = (aktie, DateTime.UtcNow);
            }

            return aktie;
        }

        // Holt die aktuellen News. order: "desc" = neueste zuerst, "asc" = aelteste zuerst.
        public Task<NewsSucheAntwort?> GetNews(int limit = 10, string order = "desc")
        {
            limit = Math.Clamp(limit, 1, 100);

            if (order != "asc")
            {
                order = "desc";
            }

            string url = $"{BasisUrl}/v2/reference/news?order={order}&limit={limit}&sort=published_utc&apikey={_apiKey}";
            return AbfrageAusfuehren<NewsSucheAntwort>(url);
        }

        // ---------- Einzelne API-Endpunkte ----------

        // Sucht zu einem Firmennamen oder Tickersymbol den passenden Ticker,
        // z. B. "ServiceNow" -> "NOW".
        private async Task<string?> TickerSuchen(string suchbegriff)
        {
            string url = $"{BasisUrl}/v3/reference/tickers?search={WebUtility.UrlEncode(suchbegriff)}&apikey={_apiKey}";
            TickerSucheAntwort? ergebnis = await AbfrageAusfuehren<TickerSucheAntwort>(url);

            if (ergebnis is null)
            {
                return null;
            }

            // Nur echte Aktien von der normalen Börse, keine OTC-Pennystocks und keine ETFs.
            // Sonst findet man bei "Apple" z. B. zuerst "Apple iSports Group" statt "Apple Inc.".
            List<TickerTreffer> aktien = ergebnis.Ergebnisse
                .Where(treffer => treffer.Markt == "stocks" && treffer.Typ == "CS")
                .ToList();

            // Am liebsten die Aktie, deren Name mit dem Suchbegriff anfängt,
            // sonst einfach die erste passende.
            TickerTreffer? treffer = aktien.FirstOrDefault(aktie =>
                aktie.Name != null && aktie.Name.StartsWith(suchbegriff, StringComparison.OrdinalIgnoreCase));

            return (treffer ?? aktien.FirstOrDefault())?.Tickersymbol;
        }

        private Task<DividendenAntwort?> DividendenLaden(string ticker)
        {
            string url = $"{BasisUrl}/v3/reference/dividends?ticker={ticker}&apikey={_apiKey}";
            return AbfrageAusfuehren<DividendenAntwort>(url);
        }

        // Eroeffnungs- und Schlusskurs des letzten abgeschlossenen Handelstags.
        private async Task<Handelstag?> LetzterHandelstagLaden(string ticker)
        {
            string url = $"{BasisUrl}/v2/aggs/ticker/{ticker}/prev?apikey={_apiKey}";
            HandelstagAntwort? ergebnis = await AbfrageAusfuehren<HandelstagAntwort>(url);
            return ergebnis?.Ergebnisse.FirstOrDefault();
        }

        // ---------- Hilfsmethode ----------

        // Fuehrt einen GET-Aufruf aus. Ist die API nicht erreichbar oder das Anfragelimit
        // erreicht (z. B. 429), kommt null zurueck, statt dass die Seite abstuerzt.
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
    }
}
