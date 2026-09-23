using System;
using System.Threading.Tasks;

namespace AktienMarkplatz.API.Massive
{
    // Verbindung zur News-API (massive.com). Liefert nur die News.
    // Alles rund um Aktien (Suche und Kurse) laeuft ueber Finnhub (siehe FinnhubVerbindung).
    public class MassiveVerbindung : ApiVerbindung
    {
        private const string BasisUrl = "https://api.massive.com";

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
    }
}
