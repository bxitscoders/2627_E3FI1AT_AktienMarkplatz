# Kursdiagramm – Design

Stand: 24.09.2026

## Ziel

Zu einer Aktie wird der Kursverlauf als Liniendiagramm angezeigt. Das Diagramm ist
wiederverwendbar:

- **verschiedene Zeiträume:** 1W, 1M, 6M, 1J
- **verschiedene Seiten:** zuerst im Aktienmarkt, später z. B. im Portfolio (Depotwert)

## Datenquelle

Finnhub liefert im kostenlosen Tarif keine historischen Kurse (`/stock/candle` ist kostenpflichtig).
Deshalb kommen die Tageskurse von **Massive** (bereits als `MassiveVerbindung` vorhanden):

```
GET https://api.massive.com/v2/aggs/ticker/{ticker}/range/1/day/{von}/{bis}?adjusted=true&sort=asc&apiKey=...
```

Antwort (gekürzt): `{ "results": [ { "t": 1787544000000, "c": 310.34, ... } ] }`
- `t` = Zeitpunkt in Millisekunden seit 1970 (UTC), `c` = Schlusskurs

Kostenloser Tarif: 5 Anfragen pro Minute, 2 Jahre Historie. Geladene Verläufe werden
10 Minuten gecacht (wie bei Finnhub), Schlüssel: Ticker + Zeitraum.

## Bausteine

| Datei | Aufgabe |
|---|---|
| `Classes/Kurspunkt.cs` | Ein Punkt: `Datum`, `Wert` |
| `Classes/Kursverlauf.cs` | `Titel` + Liste von Kurspunkten. Methoden: `Hoechst()`, `Tiefst()`, `VeraenderungProzent()`, `Steigt()`, `VeraenderungCssKlasse()`, `StartDatum(zeitraum)` |
| `API/Massive/MassiveKursverlaufAntwort.cs` | JSON-Antwort von Massive |
| `API/Massive/MassiveVerbindung.cs` | neue Methode `KursverlaufLaden(ticker, zeitraum)` → `Kursverlauf` |
| `Pages/Shared/_Kursdiagramm.cshtml` | zeichnet **jeden** `Kursverlauf`: Kopf mit Zeitraum-Links, `<canvas>`, Hoch/Tief/Veränderung |
| `wwwroot/js/site.js` | sucht alle `canvas.kursdiagramm__flaeche` und zeichnet sie mit Chart.js |
| `Pages/Shared/_Layout.cshtml` | bindet Chart.js einmal ein (cdnjs) |
| `Pages/Aktienmarkt.cshtml(.cs)` | lädt den Verlauf der gesuchten Aktie und bindet das Partial ein |

Das Diagramm kennt nur `Kursverlauf`. Woher die Punkte kommen (Aktie, Depotwert, …),
ist ihm egal. Einbinden auf einer Seite:

```cshtml
<partial name="_Kursdiagramm" model="Model.Verlauf" />
```

## Ablauf

1. Suche „Apple“ → `/Aktienmarkt?symbol=Apple`
2. Seite findet über Finnhub `AAPL` und lädt über Massive den Verlauf für `zeitraum` (Standard `1M`)
3. Partial schreibt die Punkte als JSON in `data-punkte` am `<canvas>`
4. `site.js` liest das JSON und zeichnet die Linie (grün bei Anstieg, rot bei Verlust)
5. Klick auf `[6M]` → `/Aktienmarkt?symbol=Apple&zeitraum=6M` → Seite lädt neu

Zeitraum-Links hängen nur `zeitraum=...` an die aktuelle URL an. So funktioniert
das Partial auf jeder Seite, ohne die Seite zu kennen.

## Mockup

```
AAPL  Apple Inc.
227,48 $   ▲ 1,2 % heute

Kursverlauf              [1W] [1M] [6M] [1J]
┌────────────────────────────────────────────┐
│ 230 ┤                         ╭──╮    ╭──  │
│ 220 ┤       ╭──╮      ╭──╮ ╭──╯  ╰─╮╭─╯    │
│ 210 ┤ ─╮ ╭─╯  ╰──╮ ╭─╯  ╰─╯        ╰╯      │
│     └┬───────┬───────┬───────┬───────┬     │
│    26.08.  02.09.  09.09.  16.09.  23.09.  │
└────────────────────────────────────────────┘
Hoch 231,10 $   Tief 208,40 $   Zeitraum ▲ 8,4 %
```

## Fehlerfälle

- Massive nicht erreichbar / Limit erreicht → `KursverlaufLaden` liefert einen leeren Verlauf,
  das Partial zeigt „Kein Kursverlauf verfügbar“. Leere Verläufe werden nicht gecacht.
- Unbekannter Zeitraum in der URL → wird wie `1M` behandelt.

## Tests

Neues xUnit-Projekt `AktienMarkplatz.Tests` mit Tests für `Kursverlauf`
(Hoch, Tief, Veränderung, leerer Verlauf, Startdatum pro Zeitraum).
Die API selbst wird nicht automatisch getestet, sondern im Browser geprüft.

## Nicht Teil dieser Aufgabe

- Einbau im Portfolio (Seite gehört einem Teammitglied, Partial ist dafür vorbereitet)
- mehrere Aktien in einem Diagramm, Intraday-Kurse
