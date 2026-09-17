using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AktienMarkplatz.API
{
    // Antwort vom Dividenden-Endpoint der API.
    public class DividendenAntwort
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("results")]
        public List<Dividende> Ergebnisse { get; set; } = new();
    }

    // Eine einzelne Dividendenausschüttung einer Aktie.
    public class Dividende
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("ticker")]
        public string? Tickersymbol { get; set; }

        [JsonPropertyName("cash_amount")]
        public double Betrag { get; set; }

        [JsonPropertyName("currency")]
        public string? Waehrung { get; set; }

        [JsonPropertyName("dividend_type")]
        public string? Art { get; set; }

        [JsonPropertyName("declaration_date")]
        public string? Ankuendigungsdatum { get; set; }

        [JsonPropertyName("ex_dividend_date")]
        public string? ExDatum { get; set; }

        [JsonPropertyName("pay_date")]
        public string? Auszahlungsdatum { get; set; }

        [JsonPropertyName("record_date")]
        public string? Stichtag { get; set; }

        [JsonPropertyName("frequency")]
        public int Haeufigkeit { get; set; }
    }
}
