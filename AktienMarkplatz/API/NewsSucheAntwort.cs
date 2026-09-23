using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AktienMarkplatz.API
{
    // Modell der Antwort für die News-Suche (entspricht dem Beispiel-JSON der API).
    public class NewsSucheAntwort
    {
        [JsonPropertyName("count")]
        public int? Count { get; set; }

        [JsonPropertyName("next_url")]
        public string? NextUrl { get; set; }

        [JsonPropertyName("request_id")]
        public string? RequestId { get; set; }

        [JsonPropertyName("results")]
        public List<NewsSucheEintrag>? Results { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class NewsSucheEintrag
    {
        [JsonPropertyName("amp_url")]
        public string? AmpUrl { get; set; }

        [JsonPropertyName("article_url")]
        public string? ArticleUrl { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("insights")]
        public List<Insight>? Insights { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }

        [JsonPropertyName("published_utc")]
        public DateTime? PublishedUtc { get; set; }

        [JsonPropertyName("publisher")]
        public Publisher? Publisher { get; set; }

        [JsonPropertyName("tickers")]
        public List<string>? Tickers { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }

    public class Insight
    {
        [JsonPropertyName("sentiment")]
        public string? Sentiment { get; set; }

        [JsonPropertyName("sentiment_reasoning")]
        public string? SentimentReasoning { get; set; }

        [JsonPropertyName("ticker")]
        public string? Ticker { get; set; }
    }

    public class Publisher
    {
        [JsonPropertyName("favicon_url")]
        public string? FaviconUrl { get; set; }

        [JsonPropertyName("homepage_url")]
        public string? HomepageUrl { get; set; }

        [JsonPropertyName("logo_url")]
        public string? LogoUrl { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
