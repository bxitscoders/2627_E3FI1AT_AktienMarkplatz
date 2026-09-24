using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using AktienMarkplatz.Classes;

namespace AktienMarkplatz.API.Massive
{
    // Verbindung zur massive.com-API. Liefert die News und den Kursverlauf von Aktien.
    // Suche und aktuelle Kurse laufen ueber Finnhub (siehe FinnhubVerbindung).
    // Kostenloser Tarif: 5 Anfragen pro Minute.
    public class MassiveVerbindung : ApiVerbindung
    {
        private const string BasisUrl = "https://api.massive.com";

        // Geladene Verlaeufe werden eine Weile wiederverwendet, weil der kostenlose Tarif
        // nur 5 Anfragen pro Minute erlaubt.
        private static readonly ConcurrentDictionary<string, (Kursverlauf Verlauf, DateTime GeladenAm)> _verlaufCache = new();
        private static readonly TimeSpan _cacheDauer = TimeSpan.FromMinutes(10);

        public MassiveVerbindung(string apiKey) : base(apiKey)
        {
        }

        // Holt die aktuellen News. order: "desc" = neueste zuerst, "asc" = aelteste zuerst.
        public Task<MassiveNewsAntwort?> NewsLaden(int limit = 10, string order = "desc")
        {
            limit = Math.Clamp(limit, 1, 100);

            if (order != "asc")
            {
                order = "desc";
            }

            string url = $"{BasisUrl}/v2/reference/news?order={order}&limit={limit}&sort=published_utc&apikey={_apiKey}";
            return AbfrageAusfuehren<MassiveNewsAntwort>(url);
        }

        // Laedt die Tageskurse einer Aktie fuer einen Zeitraum ("1W", "1M", "6M", "1J").
        // Klappt das nicht, kommt ein leerer Verlauf zurueck, der nicht gecacht wird.
        public async Task<Kursverlauf> KursverlaufLaden(string ticker, string? zeitraum)
        {
            var verlauf = new Kursverlauf("Kursverlauf " + ticker, zeitraum);
            string schluessel = ticker + "|" + verlauf.Zeitraum;

            if (_verlaufCache.TryGetValue(schluessel, out var eintrag) && DateTime.UtcNow - eintrag.GeladenAm < _cacheDauer)
            {
                return eintrag.Verlauf;
            }

            string von = Kursverlauf.StartDatum(verlauf.Zeitraum, DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string bis = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string url = $"{BasisUrl}/v2/aggs/ticker/{WebUtility.UrlEncode(ticker)}/range/1/day/{von}/{bis}?adjusted=true&sort=asc&apiKey={_apiKey}";

            MassiveKursverlaufAntwort? antwort = await AbfrageAusfuehren<MassiveKursverlaufAntwort>(url);
            if (antwort?.Ergebnisse is null)
            {
                return verlauf;
            }

            foreach (MassiveTageskurs tag in antwort.Ergebnisse)
            {
                DateTime datum = DateTimeOffset.FromUnixTimeMilliseconds(tag.Zeitpunkt).UtcDateTime.Date;
                verlauf.PunktHinzufuegen(datum, tag.Schluss);
            }

            if (!verlauf.IstLeer())
            {
                _verlaufCache[schluessel] = (verlauf, DateTime.UtcNow);
            }

            return verlauf;
        }
    }
}
