# Kursdiagramm – Umsetzungsplan

> **Für agentische Umsetzer:** PFLICHT-SKILL: superpowers:subagent-driven-development (empfohlen) oder superpowers:executing-plans, Aufgabe für Aufgabe. Schritte nutzen Checkboxen (`- [ ]`).

**Ziel:** Wiederverwendbares Liniendiagramm für Kursverläufe (1W/1M/6M/1J), zuerst eingebaut im Aktienmarkt.

**Architektur:** `MassiveVerbindung` lädt Tageskurse und baut daraus ein `Kursverlauf`-Objekt. Das Partial `_Kursdiagramm` zeigt jeden `Kursverlauf` an und schreibt die Punkte als JSON an ein `<canvas>`. `site.js` zeichnet alle diese Canvas-Elemente mit Chart.js. Die ganze Logik steckt in den Klassen, die Seiten rufen nur auf.

**Tech Stack:** ASP.NET Core Razor Pages (.NET 10), Chart.js 4.4.1 (cdnjs), xUnit

**Spec:** `AktienMarkplatz/Doku/Kursdiagramm-Design.md`

## Globale Vorgaben

- Einfach halten: keine Interfaces, keine DI-Registrierung. `MassiveVerbindung` wird wie bisher im Seiten-Konstruktor mit `new` erzeugt.
- Kommentare auf Deutsch, Umlaute im Code als `ae/oe/ue` (wie im restlichen Code).
- Zeiträume genau: `1W`, `1M`, `6M`, `1J`. Standard und Ersatz für ungültige Werte: `1M`.
- Massive-Endpoint: `GET {BasisUrl}/v2/aggs/ticker/{ticker}/range/1/day/{von}/{bis}?adjusted=true&sort=asc&apiKey=...`
- Cache: 10 Minuten, Schlüssel `ticker|zeitraum`, leere Verläufe werden nicht gecacht.
- Chart.js von `https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js`
- Farben: `--gain` bei Anstieg, `--loss` bei Verlust (aus `wwwroot/css/site.css`).
- Portfolio-Seite wird **nicht** angefasst.

## Review-Schwerpunkte

1. **Massive-Limit erreicht (5 Anfragen/Min):** API liefert Fehler → leerer Verlauf → Text „Kein Kursverlauf verfügbar“, keine Exception, kein Cache-Eintrag. (Aufgabe 2, manuelle Prüfung in Aufgabe 4)
2. **Leerer Verlauf:** `Hoechst()`, `Tiefst()`, `VeraenderungProzent()` liefern 0 statt Exception. (Test in Aufgabe 1)
3. **Ungültiger Zeitraum in der URL** (`?zeitraum=abc` oder fehlt): wird zu `1M`. (Test in Aufgabe 1)
4. **Erster Kurs ist 0 oder nur ein Punkt:** Veränderung 0 %, keine Division durch 0 / kein `Infinity`. (Test in Aufgabe 1)
5. **Zeitraum-Link behält die Suche:** Klick auf `6M` auf `/Aktienmarkt?symbol=Apple&zeitraum=1M` führt zu `symbol=Apple&zeitraum=6M`. (manuelle Prüfung in Aufgabe 4)

---

### Aufgabe 1: Klassen `Kurspunkt` und `Kursverlauf` mit Tests

**Dateien:**
- Erstellen: `AktienMarkplatz/Classes/Kurspunkt.cs`
- Erstellen: `AktienMarkplatz/Classes/Kursverlauf.cs`
- Erstellen: `AktienMarkplatz.Tests/` (xUnit-Projekt), Test: `AktienMarkplatz.Tests/KursverlaufTests.cs`
- Ändern: `AktienMarkplatz.slnx` (Testprojekt eintragen)

**Schnittstellen:**
- Liefert:
  - `new Kursverlauf(string titel, string? zeitraum)`
  - `string Titel`, `string Zeitraum`, `List<Kurspunkt> Punkte`
  - `void PunktHinzufuegen(DateTime datum, double wert)`
  - `bool IstLeer()`, `double Hoechst()`, `double Tiefst()`, `double VeraenderungProzent()`, `bool Steigt()`
  - `string VeraenderungText()` → z. B. `"8.4 %"` bzw. `"8,4 %"` je nach Kultur
  - `string VeraenderungCssKlasse()` → `"veraenderung--gewinn"` / `"veraenderung--verlust"`
  - `static string[] Zeitraeume` = `{ "1W", "1M", "6M", "1J" }`
  - `static string ZeitraumPruefen(string? zeitraum)`
  - `static DateTime StartDatum(string zeitraum, DateTime heute)`
  - `Kurspunkt` mit `DateTime Datum`, `double Wert`

- [ ] **Schritt 1: Testprojekt anlegen**

```bash
cd C:/Coding/Berufschule/mainProjectsB/AktienMarkplatz
dotnet new xunit -n AktienMarkplatz.Tests -o AktienMarkplatz.Tests -f net10.0
dotnet add AktienMarkplatz.Tests/AktienMarkplatz.Tests.csproj reference AktienMarkplatz/AktienMarkplatz.csproj
dotnet sln AktienMarkplatz.slnx add AktienMarkplatz.Tests/AktienMarkplatz.Tests.csproj
rm AktienMarkplatz.Tests/UnitTest1.cs
```

- [ ] **Schritt 2: Fehlschlagende Tests schreiben** – `AktienMarkplatz.Tests/KursverlaufTests.cs`

```csharp
using AktienMarkplatz.Classes;

namespace AktienMarkplatz.Tests
{
    public class KursverlaufTests
    {
        private static Kursverlauf VerlaufMit(params double[] werte)
        {
            var verlauf = new Kursverlauf("Test", "1M");
            DateTime datum = new DateTime(2026, 9, 1);
            foreach (double wert in werte)
            {
                verlauf.PunktHinzufuegen(datum, wert);
                datum = datum.AddDays(1);
            }
            return verlauf;
        }

        [Fact]
        public void Hoechst_und_Tiefst_werden_gefunden()
        {
            Kursverlauf verlauf = VerlaufMit(100, 120, 90, 110);

            Assert.Equal(120, verlauf.Hoechst());
            Assert.Equal(90, verlauf.Tiefst());
        }

        [Fact]
        public void Veraenderung_vom_ersten_zum_letzten_Punkt()
        {
            Kursverlauf verlauf = VerlaufMit(100, 90, 110);

            Assert.Equal(10, verlauf.VeraenderungProzent(), 6);
            Assert.True(verlauf.Steigt());
            Assert.Equal("veraenderung--gewinn", verlauf.VeraenderungCssKlasse());
        }

        [Fact]
        public void Fallender_Verlauf_ist_Verlust()
        {
            Kursverlauf verlauf = VerlaufMit(200, 150);

            Assert.Equal(-25, verlauf.VeraenderungProzent(), 6);
            Assert.False(verlauf.Steigt());
            Assert.Equal("veraenderung--verlust", verlauf.VeraenderungCssKlasse());
        }

        [Fact]
        public void Leerer_Verlauf_liefert_ueberall_0()
        {
            Kursverlauf verlauf = VerlaufMit();

            Assert.True(verlauf.IstLeer());
            Assert.Equal(0, verlauf.Hoechst());
            Assert.Equal(0, verlauf.Tiefst());
            Assert.Equal(0, verlauf.VeraenderungProzent());
        }

        [Fact]
        public void Ein_Punkt_hat_keine_Veraenderung()
        {
            Assert.Equal(0, VerlaufMit(100).VeraenderungProzent());
        }

        [Fact]
        public void Erster_Wert_0_teilt_nicht_durch_0()
        {
            Assert.Equal(0, VerlaufMit(0, 50).VeraenderungProzent());
        }

        [Theory]
        [InlineData("1W", "1W")]
        [InlineData("6M", "6M")]
        [InlineData("abc", "1M")]
        [InlineData("", "1M")]
        [InlineData(null, "1M")]
        public void Ungueltiger_Zeitraum_wird_1M(string? eingabe, string erwartet)
        {
            Assert.Equal(erwartet, Kursverlauf.ZeitraumPruefen(eingabe));
            Assert.Equal(erwartet, new Kursverlauf("Test", eingabe).Zeitraum);
        }

        [Theory]
        [InlineData("1W", 2026, 9, 17)]
        [InlineData("1M", 2026, 8, 24)]
        [InlineData("6M", 2026, 3, 24)]
        [InlineData("1J", 2025, 9, 24)]
        public void StartDatum_pro_Zeitraum(string zeitraum, int jahr, int monat, int tag)
        {
            DateTime heute = new DateTime(2026, 9, 24);

            Assert.Equal(new DateTime(jahr, monat, tag), Kursverlauf.StartDatum(zeitraum, heute));
        }
    }
}
```

- [ ] **Schritt 3: Tests laufen lassen, sie müssen fehlschlagen**

Ausführen: `dotnet test AktienMarkplatz.Tests`
Erwartet: Build-Fehler „Der Typ- oder Namespacename "Kursverlauf" wurde nicht gefunden“

- [ ] **Schritt 4: `Classes/Kurspunkt.cs` schreiben**

```csharp
using System;

namespace AktienMarkplatz.Classes
{
    // Ein Punkt im Kursverlauf: an welchem Tag welcher Wert.
    public class Kurspunkt
    {
        public DateTime Datum { get; }
        public double Wert { get; }

        public Kurspunkt(DateTime datum, double wert)
        {
            Datum = datum;
            Wert = wert;
        }
    }
}
```

- [ ] **Schritt 5: `Classes/Kursverlauf.cs` schreiben**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace AktienMarkplatz.Classes
{
    // Werte ueber einen Zeitraum, z. B. die Tageskurse einer Aktie oder spaeter der Depotwert.
    // Wird vom Partial _Kursdiagramm als Liniendiagramm angezeigt.
    public class Kursverlauf
    {
        public static readonly string[] Zeitraeume = { "1W", "1M", "6M", "1J" };

        public string Titel { get; }
        public string Zeitraum { get; }
        public List<Kurspunkt> Punkte { get; } = new();

        public Kursverlauf(string titel, string? zeitraum)
        {
            Titel = titel;
            Zeitraum = ZeitraumPruefen(zeitraum);
        }

        public void PunktHinzufuegen(DateTime datum, double wert)
        {
            Punkte.Add(new Kurspunkt(datum, wert));
        }

        public bool IstLeer()
        {
            return Punkte.Count == 0;
        }

        public double Hoechst()
        {
            if (IstLeer()) return 0;
            return Punkte.Max(punkt => punkt.Wert);
        }

        public double Tiefst()
        {
            if (IstLeer()) return 0;
            return Punkte.Min(punkt => punkt.Wert);
        }

        // Veraenderung vom ersten zum letzten Punkt in Prozent.
        public double VeraenderungProzent()
        {
            if (Punkte.Count < 2) return 0;

            double erster = Punkte[0].Wert;
            double letzter = Punkte[^1].Wert;
            if (erster == 0) return 0;

            return (letzter - erster) / erster * 100;
        }

        public bool Steigt()
        {
            return VeraenderungProzent() >= 0;
        }

        public string VeraenderungText()
        {
            return Math.Abs(VeraenderungProzent()).ToString("0.0") + " %";
        }

        public string VeraenderungCssKlasse()
        {
            if (Steigt())
                return "veraenderung--gewinn";

            return "veraenderung--verlust";
        }

        // Unbekannte Zeitraeume (z. B. aus der URL) werden zu "1M".
        public static string ZeitraumPruefen(string? zeitraum)
        {
            if (zeitraum != null && Zeitraeume.Contains(zeitraum))
                return zeitraum;

            return "1M";
        }

        // Ab welchem Tag die Kurse geladen werden.
        public static DateTime StartDatum(string zeitraum, DateTime heute)
        {
            switch (ZeitraumPruefen(zeitraum))
            {
                case "1W": return heute.AddDays(-7);
                case "6M": return heute.AddMonths(-6);
                case "1J": return heute.AddYears(-1);
                default: return heute.AddMonths(-1);
            }
        }
    }
}
```

- [ ] **Schritt 6: Tests laufen lassen, alle müssen grün sein**

Ausführen: `dotnet test AktienMarkplatz.Tests`
Erwartet: `Bestanden! Fehler: 0, erfolgreich: 15`

- [ ] **Schritt 7: Commit**

```bash
git add AktienMarkplatz.slnx AktienMarkplatz.Tests AktienMarkplatz/Classes/Kurspunkt.cs AktienMarkplatz/Classes/Kursverlauf.cs
git commit -m "Kursverlauf-Klasse mit Tests"
```

---

### Aufgabe 2: Kursverlauf von Massive laden

**Dateien:**
- Erstellen: `AktienMarkplatz/API/Massive/MassiveKursverlaufAntwort.cs`
- Ändern: `AktienMarkplatz/API/Massive/MassiveVerbindung.cs`

**Schnittstellen:**
- Nutzt: `Kursverlauf`, `Kursverlauf.StartDatum`, `PunktHinzufuegen`, `IstLeer` aus Aufgabe 1
- Liefert: `Task<Kursverlauf> MassiveVerbindung.KursverlaufLaden(string ticker, string? zeitraum)`
  - Titel des Verlaufs: `"Kursverlauf " + ticker`
  - Bei Fehler: leerer Verlauf (nie `null`)

- [ ] **Schritt 1: `API/Massive/MassiveKursverlaufAntwort.cs` schreiben**

```csharp
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
```

- [ ] **Schritt 2: `KursverlaufLaden` in `MassiveVerbindung.cs` ergänzen**

Oben ergänzen:

```csharp
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using AktienMarkplatz.Classes;
```

Kommentar der Klasse ändern auf:

```csharp
    // Verbindung zur massive.com-API. Liefert die News und den Kursverlauf von Aktien.
    // Suche und aktuelle Kurse laufen ueber Finnhub (siehe FinnhubVerbindung).
    // Kostenloser Tarif: 5 Anfragen pro Minute.
```

In der Klasse unter `BasisUrl` ergänzen:

```csharp
        // Geladene Verlaeufe werden eine Weile wiederverwendet, weil der kostenlose Tarif
        // nur 5 Anfragen pro Minute erlaubt.
        private static readonly ConcurrentDictionary<string, (Kursverlauf Verlauf, DateTime GeladenAm)> _verlaufCache = new();
        private static readonly TimeSpan _cacheDauer = TimeSpan.FromMinutes(10);
```

Nach `NewsLaden` ergänzen:

```csharp
        // Laedt die Tageskurse einer Aktie fuer einen Zeitraum ("1W", "1M", "6M", "1J").
        // Klappt das nicht, kommt ein leerer Verlauf zurueck, der nicht gecacht wird.
        public async Task<Kursverlauf> KursverlaufLaden(string ticker, string? zeitraum)
        {
            var verlauf = new Kursverlauf("Kursverlauf " + ticker, zeitraum);
            string schluessel = ticker + "|" + verlauf.Zeitraum;

            if (_verlaufCache.TryGetValue(schluessel, out var eintrag) && DateTime.UtcNow - eintrag.GeladenAm < _cacheDauer)
            {
                return eintrag.Verlauf;
            }

            string von = Kursverlauf.StartDatum(verlauf.Zeitraum, DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string bis = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string url = $"{BasisUrl}/v2/aggs/ticker/{WebUtility.UrlEncode(ticker)}/range/1/day/{von}/{bis}?adjusted=true&sort=asc&apiKey={_apiKey}";

            MassiveKursverlaufAntwort? antwort = await AbfrageAusfuehren<MassiveKursverlaufAntwort>(url);
            if (antwort?.Ergebnisse is null)
            {
                return verlauf;
            }

            foreach (MassiveTageskurs tag in antwort.Ergebnisse)
            {
                DateTime datum = DateTimeOffset.FromUnixTimeMilliseconds(tag.Zeitpunkt).UtcDateTime.Date;
                verlauf.PunktHinzufuegen(datum, tag.Schluss);
            }

            if (!verlauf.IstLeer())
            {
                _verlaufCache[schluessel] = (verlauf, DateTime.UtcNow);
            }

            return verlauf;
        }
```

- [ ] **Schritt 3: Bauen**

Ausführen: `dotnet build AktienMarkplatz.slnx`
Erwartet: `0 Fehler`

- [ ] **Schritt 4: Tests laufen noch**

Ausführen: `dotnet test AktienMarkplatz.Tests`
Erwartet: `Fehler: 0, erfolgreich: 15`

- [ ] **Schritt 5: Commit**

```bash
git add AktienMarkplatz/API/Massive
git commit -m "Kursverlauf ueber Massive laden"
```

---

### Aufgabe 3: Partial `_Kursdiagramm` mit Chart.js

**Dateien:**
- Erstellen: `AktienMarkplatz/Pages/Shared/_Kursdiagramm.cshtml`
- Erstellen: `AktienMarkplatz/Pages/Shared/_Kursdiagramm.cshtml.css`
- Ändern: `AktienMarkplatz/wwwroot/js/site.js`
- Ändern: `AktienMarkplatz/Pages/Shared/_Layout.cshtml:45-47` (Chart.js vor `site.js`)

**Schnittstellen:**
- Nutzt: `Kursverlauf` (alle Methoden aus Aufgabe 1)
- Liefert: `<partial name="_Kursdiagramm" model="..." />` mit Model vom Typ `Kursverlauf`
- HTML-Vertrag zwischen Partial und `site.js`: `<canvas class="kursdiagramm__flaeche" data-steigt="true|false" data-punkte='[{"datum":"24.09.26","wert":227.48}, ...]'>`

- [ ] **Schritt 1: `Pages/Shared/_Kursdiagramm.cshtml` schreiben**

```cshtml
@model AktienMarkplatz.Classes.Kursverlauf
@using AktienMarkplatz.Classes
@using Microsoft.AspNetCore.Http.Extensions

@* Wiederverwendbares Kursdiagramm. Einbinden mit:
   <partial name="_Kursdiagramm" model="Model.Verlauf" />
   Gezeichnet wird es in wwwroot/js/site.js. *@

@{
    // Link fuer einen Zeitraum-Button: aktuelle URL, nur "zeitraum" wird ersetzt.
    // So funktioniert das Partial auf jeder Seite.
    string ZeitraumLink(string zeitraum)
    {
        var parameter = new QueryBuilder(Context.Request.Query
            .Where(eintrag => eintrag.Key != "zeitraum")
            .SelectMany(eintrag => eintrag.Value.Select(wert => new KeyValuePair<string, string>(eintrag.Key, wert ?? ""))));
        parameter.Add("zeitraum", zeitraum);
        return Context.Request.Path + parameter.ToQueryString().ToString();
    }

    string punkteJson = System.Text.Json.JsonSerializer.Serialize(
        Model.Punkte.Select(punkt => new { datum = punkt.Datum.ToString("dd.MM.yy"), wert = punkt.Wert }));
}

<section class="kursdiagramm">
    <div class="kursdiagramm__kopf">
        <h2 class="kursdiagramm__titel">@Model.Titel</h2>
        <nav class="kursdiagramm__zeitraeume" aria-label="Zeitraum">
            @foreach (string zeitraum in Kursverlauf.Zeitraeume)
            {
                <a href="@ZeitraumLink(zeitraum)"
                   class="kursdiagramm__zeitraum @(zeitraum == Model.Zeitraum ? "kursdiagramm__zeitraum--aktiv" : "")">@zeitraum</a>
            }
        </nav>
    </div>

    @if (Model.IstLeer())
    {
        <p class="kursdiagramm__leer">Kein Kursverlauf verfügbar. Bitte in einer Minute nochmal versuchen.</p>
    }
    else
    {
        <div class="kursdiagramm__rahmen">
            <canvas class="kursdiagramm__flaeche"
                    data-steigt="@(Model.Steigt() ? "true" : "false")"
                    data-punkte="@punkteJson"></canvas>
        </div>

        <div class="kursdiagramm__kennzahlen">
            <span>Hoch @Model.Hoechst().ToString("0.00") $</span>
            <span>Tief @Model.Tiefst().ToString("0.00") $</span>
            <span class="@Model.VeraenderungCssKlasse()">Zeitraum @(Model.Steigt() ? "▲" : "▼") @Model.VeraenderungText()</span>
        </div>
    }
</section>
```

- [ ] **Schritt 2: `Pages/Shared/_Kursdiagramm.cshtml.css` schreiben**

```css
.kursdiagramm {
  margin: 1.5rem 0;
}

.kursdiagramm__kopf {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.kursdiagramm__titel {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.kursdiagramm__zeitraeume {
  display: flex;
  gap: 0.25rem;
}

.kursdiagramm__zeitraum {
  padding: 0.3rem 0.7rem;
  border-radius: var(--radius-s);
  color: var(--ink-muted);
  font-family: var(--font-mono);
  font-size: 0.85rem;
  text-decoration: none;
}

.kursdiagramm__zeitraum:hover {
  background: var(--panel);
  color: var(--ink);
}

.kursdiagramm__zeitraum--aktiv {
  background: var(--akzent-tint);
  color: var(--akzent);
  font-weight: 600;
}

.kursdiagramm__rahmen {
  position: relative;
  height: 260px;
  padding: 0.75rem;
  border: 1px solid var(--line);
  border-radius: var(--radius-s);
  background: var(--bg);
}

.kursdiagramm__kennzahlen {
  display: flex;
  flex-wrap: wrap;
  gap: 1.25rem;
  margin-top: 0.75rem;
  color: var(--ink-muted);
  font-family: var(--font-mono);
  font-size: 0.9rem;
}

.kursdiagramm__kennzahlen .veraenderung--gewinn {
  color: var(--gain);
}

.kursdiagramm__kennzahlen .veraenderung--verlust {
  color: var(--loss);
}

.kursdiagramm__leer {
  color: var(--ink-muted);
}
```

- [ ] **Schritt 3: Chart.js im Layout einbinden** – in `Pages/Shared/_Layout.cshtml` direkt **vor** der Zeile mit `~/js/site.js`:

```cshtml
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js"
            integrity="sha384-bs/nf9FbdNouRbMiFcrcZfLXYPKiPaGVGplVbv7dLGECccEXDW+S3zjqSKR5ZEaD"
            crossorigin="anonymous"></script>
```

- [ ] **Schritt 4: Zeichnen in `wwwroot/js/site.js`** – den Inhalt ersetzen durch:

```js
// Zeichnet alle Kursdiagramme auf der Seite (siehe Pages/Shared/_Kursdiagramm.cshtml).
// Jedes <canvas class="kursdiagramm__flaeche"> bringt seine Punkte als JSON in data-punkte mit.
document.querySelectorAll("canvas.kursdiagramm__flaeche").forEach(function (canvas) {
    if (typeof Chart === "undefined") return;

    const punkte = JSON.parse(canvas.dataset.punkte);
    const farbName = canvas.dataset.steigt === "true" ? "--gain" : "--loss";
    const farbe = getComputedStyle(document.documentElement).getPropertyValue(farbName).trim();

    new Chart(canvas, {
        type: "line",
        data: {
            labels: punkte.map(function (punkt) { return punkt.datum; }),
            datasets: [{
                data: punkte.map(function (punkt) { return punkt.wert; }),
                borderColor: farbe,
                borderWidth: 2,
                pointRadius: 0,
                tension: 0.2
            }]
        },
        options: {
            maintainAspectRatio: false,
            interaction: { mode: "index", intersect: false },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (eintrag) {
                            return eintrag.parsed.y.toFixed(2).replace(".", ",") + " $";
                        }
                    }
                }
            },
            scales: {
                x: { grid: { display: false }, ticks: { maxTicksLimit: 6 } }
            }
        }
    });
});
```

- [ ] **Schritt 5: Bauen**

Ausführen: `dotnet build AktienMarkplatz.slnx`
Erwartet: `0 Fehler`

- [ ] **Schritt 6: Commit**

```bash
git add AktienMarkplatz/Pages/Shared AktienMarkplatz/wwwroot/js/site.js
git commit -m "Wiederverwendbares Kursdiagramm als Partial"
```

---

### Aufgabe 4: Im Aktienmarkt einbauen und im Browser prüfen

**Dateien:**
- Ändern: `AktienMarkplatz/Pages/Aktienmarkt.cshtml.cs`
- Ändern: `AktienMarkplatz/Pages/Aktienmarkt.cshtml` (im `else`-Zweig nach der `</table>` der gefundenen Aktie)

**Schnittstellen:**
- Nutzt: `MassiveVerbindung.KursverlaufLaden(string ticker, string? zeitraum)` aus Aufgabe 2, Partial `_Kursdiagramm` aus Aufgabe 3

- [ ] **Schritt 1: `Aktienmarkt.cshtml.cs` erweitern**

Oben ergänzen:

```csharp
using AktienMarkplatz.API.Massive;
```

Feld und Konstruktor:

```csharp
        private readonly FinnhubVerbindung _finnhub;
        private readonly MassiveVerbindung _massive;

        public AktienmarktModel(IConfiguration configuration)
        {
            string finnhubApiKey = configuration["FinnhubApi:ApiKey"] ?? string.Empty;
            _finnhub = new FinnhubVerbindung(finnhubApiKey);

            string massiveApiKey = configuration["MassiveApi:ApiKey"] ?? string.Empty;
            _massive = new MassiveVerbindung(massiveApiKey);
        }
```

Neue Eigenschaft unter `GefundeneAktie`:

```csharp
        // Kursverlauf der gesuchten Aktie, null wenn keine Aktie gefunden wurde.
        public Kursverlauf? Verlauf { get; private set; }
```

`OnGetAsync` bekommt den Zeitraum und lädt den Verlauf:

```csharp
        public async Task OnGetAsync(string symbol, string? zeitraum)
        {
            Symbol = symbol;

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                GefundeneAktie = await _finnhub.AktieSuchen(symbol);

                if (GefundeneAktie is not null)
                {
                    Verlauf = await _massive.KursverlaufLaden(GefundeneAktie.Symbol, zeitraum);
                }
            }

            // ... Rest (BeispielAktien laden) bleibt unveraendert
```

- [ ] **Schritt 2: Partial in `Aktienmarkt.cshtml` einbinden** – direkt nach dem `</table>` im `else`-Zweig:

```cshtml
            @if (Model.Verlauf is not null)
            {
                <partial name="_Kursdiagramm" model="Model.Verlauf" />
            }
```

- [ ] **Schritt 3: Bauen und Tests**

Ausführen: `dotnet build AktienMarkplatz.slnx; dotnet test AktienMarkplatz.Tests`
Erwartet: `0 Fehler`, `erfolgreich: 15`

- [ ] **Schritt 4: Im Browser prüfen**

Ausführen: `dotnet run --project AktienMarkplatz`, dann nacheinander öffnen:

1. `/Aktienmarkt?symbol=Apple` → Diagramm „Kursverlauf AAPL“, `1M` hervorgehoben, Linie grün oder rot, Hoch/Tief/Zeitraum darunter, Tooltip beim Drüberfahren zeigt z. B. `310,34 $`
2. Auf `6M` klicken → URL ist `/Aktienmarkt?symbol=Apple&zeitraum=6M`, Diagramm zeigt ca. 125 Tage
3. `/Aktienmarkt?symbol=Apple&zeitraum=abc` → wie `1M`
4. `/Aktienmarkt?symbol=gibtsnicht` → kein Diagramm, nur „Keine Aktie gefunden“
5. Schnell 6× verschiedene Aktien/Zeiträume öffnen (Limit 5/Min) → „Kein Kursverlauf verfügbar“ statt Absturz, nach einer Minute klappt es wieder
6. Browserfenster auf Handybreite ziehen → Diagramm passt sich an, keine horizontale Scrollleiste

- [ ] **Schritt 5: Commit**

```bash
git add AktienMarkplatz/Pages/Aktienmarkt.cshtml AktienMarkplatz/Pages/Aktienmarkt.cshtml.cs
git commit -m "Kursdiagramm im Aktienmarkt"
```

---

## Später: Einbau im Portfolio (nicht Teil dieses Plans)

Wer die Portfolio-Seite baut, erzeugt einen eigenen Verlauf und bindet das Partial ein:

```csharp
var verlauf = new Kursverlauf("Depotwert", zeitraum);
verlauf.PunktHinzufuegen(datum, wert); // fuer jeden Tag
```

```cshtml
<partial name="_Kursdiagramm" model="Model.Verlauf" />
```
