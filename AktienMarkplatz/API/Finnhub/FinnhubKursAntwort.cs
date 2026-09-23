using System.Text.Json.Serialization;

namespace AktienMarkplatz.API.Finnhub
{
    // Antwort vom Kurs-Endpoint (quote) der Finnhub-API.
    public class FinnhubKursAntwort
    {
        [JsonPropertyName("c")]
        public double Aktuell { get; set; }

        [JsonPropertyName("o")]
        public double Eroeffnung { get; set; }
    }
}
