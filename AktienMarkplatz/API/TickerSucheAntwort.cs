using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AktienMarkplatz.API
{
    // Antwort von der Ticker-Suche. Damit finden wir zu einem Firmennamen
    // (z. B. "ServiceNow") das passende Tickersymbol (z. B. "NOW").
    public class TickerSucheAntwort
    {
        [JsonPropertyName("results")]
        public List<TickerTreffer> Ergebnisse { get; set; } = new();
    }

    public class TickerTreffer
    {
        [JsonPropertyName("ticker")]
        public string? Tickersymbol { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        // "stocks" = normale Börse, "otc" = meistens Kleinstfirmen/Pennystocks
        [JsonPropertyName("market")]
        public string? Markt { get; set; }

        // "CS" = Common Stock (normale Aktie), sonst z. B. "ETF"
        [JsonPropertyName("type")]
        public string? Typ { get; set; }
    }
}
