using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AktienMarkplatz.API
{
    public class DividendApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("results")]
        public List<Dividend> Results { get; set; } = new();
    }

    public class Dividend
    {
        [JsonPropertyName("ticker")]
        public string? Ticker { get; set; }

        [JsonPropertyName("cash_amount")]
        public double CashAmount { get; set; }

        [JsonPropertyName("declaration_date")]
        public string? DeclarationDate { get; set; }

        [JsonPropertyName("ex_dividend_date")]
        public string? ExDividendDate { get; set; }

        [JsonPropertyName("pay_date")]
        public string? PayDate { get; set; }

        [JsonPropertyName("record_date")]
        public string? RecordDate { get; set; }

        [JsonPropertyName("frequency")]
        public int Frequency { get; set; }
    }
}
