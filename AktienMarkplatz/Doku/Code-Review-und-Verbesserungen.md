# Code-Review: SOLID, Patterns und Verbesserungen

Stand: 23.09.2026 – Grundlage zur Besprechung im Team.

## 1. SOLID – was schon drin ist

| Prinzip | Stand | Beispiel aus unserem Code |
|---|---|---|
| **S** – Single Responsibility (eine Aufgabe pro Klasse) | 🟡 teilweise | Gut: `FinnhubVerbindung` macht nur Aktien, `MassiveVerbindung` nur News, `Position` nur eine Depot-Position. Schlecht: `Aktie` liefert CSS-Klassen (`VeraenderungCssKlasse`). |
| **O** – Open/Closed (erweitern statt ändern) | ✅ ja | Für eine neue API wird einfach eine neue Unterklasse von `ApiVerbindung` angelegt, die Basisklasse bleibt unverändert. Genauso `Transaktion` → `Kauftransaktion` / `Verkaufstransaktion`. |
| **L** – Liskov Substitution (Unterklasse ersetzt Oberklasse) | 🟡 formal ja | Die Vererbung ist korrekt, aber nirgends wird eine Unterklasse als `ApiVerbindung` benutzt. Erklären lässt es sich, zeigen nicht. |
| **I** – Interface Segregation (kleine Interfaces) | ❌ kaum | Es gibt nur `IEmailSender`, und das kommt vom Framework. Eigene Interfaces haben wir keine. |
| **D** – Dependency Inversion (von Abstraktionen abhängen) | ❌ nein | Die Seiten erzeugen `new FinnhubVerbindung(...)` selbst. Das Briefing nennt DIP aber ausdrücklich („SOLID, v. a. DIP“). |

## 2. Patterns – was schon drin ist

| Pattern | Wo |
|---|---|
| **Razor Pages / PageModel** | Alle Seiten. `PageModel` übernimmt die Rolle, die bei WPF das ViewModel hat. Wichtig für die Doku zur Abweichung von WPF/MVVM. |
| **Vererbung mit abstrakter Basisklasse** | `ApiVerbindung` mit `FinnhubVerbindung` und `MassiveVerbindung` |
| **Dependency Injection** (nur über das Framework) | `DbContext`, Identity, `IEmailSender`, `IConfiguration` in `Program.cs` |
| **DTO** (Data Transfer Object) | `FinnhubKursAntwort`, `FinnhubSucheAntwort`, `MassiveNewsAntwort`: reine Datenklassen für das JSON der APIs |
| **Cache** | 10-Minuten-Cache für Kurse in `FinnhubVerbindung` |
| **Singleton** (im Ansatz) | statischer `HttpClient` in `ApiVerbindung` |
| **Repository** | ❌ nicht drin, obwohl im Briefing geplant (`IAktieRepository`) |

## 3. Entscheidung: Dependency Inversion (DIP)

| Variante | Aufwand | Vorteil | Nachteil |
|---|---|---|---|
| **A: In der Doku begründen** | keiner | Code bleibt einfach | Punktabzug möglich, wenn DIP bewertet wird |
| **B: Minimal nachrüsten**: ein Interface für `FinnhubVerbindung` und Registrierung per DI in `Program.cs` | ca. 15 Zeilen | DIP ist echt umgesetzt und zeigbar | etwas mehr Code |

➡️ **Empfehlung: B**, wenn DIP bewertet wird.

## 4. Was wir verbessern sollten

### 🔴 Wichtig

1. **Die Seiten sind nicht geschützt.** Nur `/Verrechnungskonto` hat `[Authorize]`. Wer z. B. `/Aktienmarkt` direkt aufruft, kommt ohne Login rein. Der Login ist damit nur eine Umleitung auf der Startseite.
2. **Zugangsdaten im Repo:** Die API-Keys und das Admin-Passwort stehen in `appsettings.json`, Test-Passwörter in `TestBenutzer.cs` und in `Data/schema.sql`. Private Zugangsdaten gehören nicht in Dateien im Repo.
3. **Vorgaben aus dem Briefing fehlen:**
   - kein CRUD für Aktien (Anlegen, Ändern, Löschen)
   - keine eigene REST-API (`/api/aktien`)
   - keine Aktien in der Datenbank, die kommen nur von Finnhub
   - keine Tests. „Automatisch testbar“ und xUnit-Tests sind vorgegeben. `Aktie`, `Position` und `Verrechnungskonto` wären leicht zu testen.

### 🟡 Code-Qualität

4. **`Transaktion.Datum` hat den Typ `DataSetDateTime`.** Das ist eine Aufzählung aus `System.Data`, kein Datum. Richtig wäre `DateTime`.
5. **`Settings.cshtml.cs` enthält eine Klasse namens `LoginModel`.** Der Name ist falsch und verwechselbar mit dem Identity-Login.
6. **Doppelte Benutzer-Logik:** `Benutzer.cs` und die Tabelle `Benutzer` in `schema.sql` werden nicht genutzt. Den Login macht Identity mit `ApplicationUser`.
7. **Geldbeträge sind uneinheitlich:** Kurse sind `double`, Kontostände `decimal`. Für Geld sollte man durchgehend `decimal` nehmen.
8. **`Einstellungen` hat kleingeschriebene Properties** (`newEmail`, `newPasswort`) und Englisch statt Deutsch.

### ⚪ Offen, weil noch in Arbeit

- `Depot`, `Kauftransaktion`, `Verkaufstransaktion` und die Seite `Portfolio` sind noch leer.

## 5. Vorschlag für die Reihenfolge

1. `[Authorize]` auf die Seiten setzen (eine Zeile pro Seite).
2. Entscheiden: DIP nachrüsten (B) oder in der Doku begründen (A).
3. Zugangsdaten aus dem Repo entfernen.
4. Kleine Fehler beheben (Punkte 4–6, 8).
5. Tests für `Aktie`, `Position` und `Verrechnungskonto` schreiben.
6. Klären, ob CRUD und eine eigene REST-API noch umgesetzt werden oder ob die Abweichung in der Doku begründet wird.

## 6. Zu klären im Team

- [ ] Wird DIP bewertet? → Variante A oder B?
- [ ] Brauchen wir CRUD und eine eigene REST-API noch, oder begründen wir die Abweichung?
- [ ] Wer übernimmt welchen Punkt?
