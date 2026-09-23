using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AktienMarkplatz.API
{
    // Antwort vom Endpoint fuer den letzten abgeschlossenen Handelstag einer Aktie.
    public class HandelstagAntwort
    {
        [JsonPropertyName("results")]
        public List<Handelstag> Ergebnisse { get; set; } = new();
    }

    // Eroeffnungs- und Schlusskurs eines einzelnen Handelstags.
    public class Handelstag
    {
        [JsonPropertyName("o")]
        public double Eroeffnung { get; set; }

        [JsonPropertyName("c")]
        public double Schluss { get; set; }
    }
}
