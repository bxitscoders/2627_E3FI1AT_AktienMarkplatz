using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AktienMarkplatz.Classes;

namespace AktienMarkplatz.API.Finnhub
{
    // Verbindung zur Finnhub-API (finnhub.io). Liefert Aktiensuche und Kurse.
    // Kostenloser Tarif: 60 Anfragen pro Minute.
    public class FinnhubVerbindung : ApiVerbindung
    {
        private const string BasisUrl = "https://finnhub.io/api/v1";

        // Geladene Kurse werden eine Weile wiederverwendet, damit nicht bei jedem
        // Seitenaufruf das Anfragelimit der API aufgebraucht wird.
        private static readonly ConcurrentDictionary<string, (Aktie Aktie, DateTime GeladenAm)> _aktienCache = new();
        private static readonly TimeSpan _cacheDauer = TimeSpan.FromMinutes(10);

        public FinnhubVerbindung(string apiKey) : base(apiKey)
        {
        }

        // Sucht zu einem Firmennamen oder Tickersymbol die passende Aktie samt Kurs,
        // z. B. "ServiceNow" -> NOW. Wird nichts gefunden, kommt null zurueck.
        public async Task<Aktie?> AktieSuchen(string suchbegriff)
        {
            FinnhubSuchTreffer? treffer = await TickerSuchen(suchbegriff);

            if (treffer?.Tickersymbol is null)
            {
                return null;
            }

            return await AktieLaden(treffer.Tickersymbol, treffer.Name ?? treffer.Tickersymbol);
        }

        // Sucht zu einem Firmennamen oder Tickersymbol den passenden Suchtreffer.
        private async Task<FinnhubSuchTreffer?> TickerSuchen(string suchbegriff)
        {
            string url = $"{BasisUrl}/search?q={WebUtility.UrlEncode(suchbegriff)}&token={_apiKey}";
            FinnhubSucheAntwort? ergebnis = await AbfrageAusfuehren<FinnhubSucheAntwort>(url);

            if (ergebnis is null)
            {
                return null;
            }

            // Nur Aktien von US-Boersen (Ticker ohne Punkt, z. B. "SAP" statt "SAP.DE"),
            // weil der kostenlose Tarif nur fuer diese Kurse liefert. ADRs sind auslaendische
            // Firmen an der US-Boerse, z. B. SAP.
            List<FinnhubSuchTreffer> aktien = ergebnis.Ergebnisse
                .Where(treffer => treffer.Tickersymbol != null && !treffer.Tickersymbol.Contains('.'))
                .Where(treffer => treffer.Typ == "Common Stock" || treffer.Typ == "ADR")
                .ToList();

            // Am liebsten die Aktie, deren Name mit dem Suchbegriff anfängt,
            // sonst einfach die erste passende.
            FinnhubSuchTreffer? treffer = aktien.FirstOrDefault(aktie =>
                aktie.Name != null && aktie.Name.StartsWith(suchbegriff, StringComparison.OrdinalIgnoreCase));

            return treffer ?? aktien.FirstOrDefault();
        }

        // Laedt den aktuellen Kurs zu einem bekannten Ticker. Nur erfolgreich geladene Aktien
        // kommen in den Cache, sonst wird beim naechsten Seitenaufruf erneut geladen.
        public async Task<Aktie> AktieLaden(string ticker, string name)
        {
            if (_aktienCache.TryGetValue(ticker, out var eintrag) && DateTime.UtcNow - eintrag.GeladenAm < _cacheDauer)
            {
                return eintrag.Aktie;
            }

            var aktie = new Aktie(ticker, name, 1);

            string url = $"{BasisUrl}/quote?symbol={ticker}&token={_apiKey}";
            FinnhubKursAntwort? kurs = await AbfrageAusfuehren<FinnhubKursAntwort>(url);

            // Bei unbekanntem Ticker liefert Finnhub keinen Fehler, sondern ueberall 0.
            if (kurs is not null && kurs.Eroeffnung > 0 && kurs.Aktuell > 0)
            {
                aktie.HandelstagFestlegen(kurs.Eroeffnung, kurs.Aktuell);
                _aktienCache[ticker] = (aktie, DateTime.UtcNow);
            }

            return aktie;
        }
    }
}
