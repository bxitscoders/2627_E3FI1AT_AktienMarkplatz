using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AktienMarkplatz.API.Massive
{
    // Antwort vom Aggregates-Endpoint der massive.com-API (Tageskurse).
    public class MassiveKursverlaufAntwort
    {
        [JsonPropertyName("results")]
        public List<MassiveTageskurs>? Ergebnisse { get; set; }
    }

    public class MassiveTageskurs
    {
        // Zeitpunkt in Millisekunden seit 1970 (UTC).
        [JsonPropertyName("t")]
        public long Zeitpunkt { get; set; }

        // Schlusskurs des Tages.
        [JsonPropertyName("c")]
        public double Schluss { get; set; }
    }
}
