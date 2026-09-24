using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AktienMarkplatz.API.Finnhub
{
    // Antwort von der Ticker-Suche der Finnhub-API. Damit finden wir zu einem Firmennamen
    // (z. B. "ServiceNow") das passende Tickersymbol (z. B. "NOW").
    public class FinnhubSucheAntwort
    {
        [JsonPropertyName("result")]
        public List<FinnhubSuchTreffer> Ergebnisse { get; set; } = new();
    }

    public class FinnhubSuchTreffer
    {
        [JsonPropertyName("symbol")]
        public string? Tickersymbol { get; set; }

        [JsonPropertyName("description")]
        public string? Name { get; set; }

        // "Common Stock" = normale Aktie, "ADR" = auslaendische Aktie an der US-Boerse, sonst z. B. "ETP"
        [JsonPropertyName("type")]
        public string? Typ { get; set; }
    }
}
