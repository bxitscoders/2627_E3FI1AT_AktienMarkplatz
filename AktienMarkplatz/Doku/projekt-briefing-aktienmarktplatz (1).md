# Projekt-Briefing: Aktienmarktplatz (ASP.NET Core, SQLite, 9h/3 Personen)

## 1. Vorgaben aus der Lernsituation (Original-Dokument)

- **CRUD-Szenario** – Aktien anlegen/lesen/ändern/löschen.
- **Mehrere Entwickler parallel** – klare Projektgrenzen, Interfaces als Verträge.
- **Automatisch testbar** – Geschäftslogik von UI/API-Layer entkoppelt.
- **DB über REST-API erreichbar** – bei euch keine "Nice-to-have"-Option mehr, sondern zentraler Mechanismus: die Startseite holt Aktien über die eigene API, nicht direkt aus der DB.
- **Datenmodell mehrfach verwendbar** – Models in eigener Class Library.
- **SOLID**, v. a. DIP (Controller/API hängen von Interfaces ab, nicht von konkreten Klassen).
- **Doku (Pflicht):** Projektbeschreibung, Technologieschema, Klassendiagramm, Use-Case-Diagramm, ERM & RM, Struktogramm.
- **SCRUM:** Backlog, Taskboard, Sprints.
- **Technologie-Abweichung von WPF/MVVM** (ihr nutzt ASP.NET Core statt WPF) → laut Aufgabenstellung müsst ihr zusätzlich dokumentieren, wie SOLID-Prinzipien und Patterns in eurer Technologie umgesetzt sind, plus ein Technologieschema und ein UML-Diagramm der wesentlichen Programmabläufe. Das ist Pflicht, nicht optional – bei 9h fest einplanen, sonst fällt es hinten runter.

## 2. Feature-Scope

**MVP (muss laufen):**
1. Login (einfache Anmeldung, kein aufwendiges Rollen-/Rechtekonzept).
2. Startseite: Aktien suchen – die Suche läuft über einen Aufruf der eigenen REST-API, nicht direkt gegen die Datenbank.

**Stretch (nur wenn MVP steht und Zeit übrig ist):**
- Vergleichsfunktion (zwei/mehrere Aktien nebeneinander vergleichen).
- Weitere Features nach demselben Muster: zuerst als Backlog-Eintrag mit "Nice-to-have"-Kennzeichnung, erst umsetzen, wenn MVP fertig und getestet ist.

**Warum diese Reihenfolge:** Bei 9h ist die Versuchung groß, an Zusatzfeatures zu arbeiten, weil sie mehr Spaß machen als Login/CRUD-Basics. Das ist der häufigste Grund, warum Zeitbudget-Projekte am Ende unfertig wirken. Login + Suche zuerst fertig und stabil, alles andere ist Bonus.

## 3. Architektur – 3 Projekte

```
AktienMarktplatz.sln
├── AktienMarktplatz.Core     (Class Library: Model Aktie, Interfaces IAktieRepository/IAktieService)
├── AktienMarktplatz.Data     (Class Library: EF Core + SQLite, Repository-Implementierung, Identity-Tabellen)
└── AktienMarktplatz.Web      (ASP.NET Core: Login via Identity, Razor-Startseite, 
                                eigene API-Controller für die Suche, Service-Implementierung)
```

**Wichtige Entscheidung:** Kein separates Api-Projekt mehr. `Web` enthält sowohl die API-Endpunkte (`/api/aktien?suche=...`) als auch die Razor-Seite mit Login. Die Startseite ruft die API per JavaScript (`fetch`) auf – damit ist "DB über REST erreichbar" echt umgesetzt (nicht nur behauptet), ohne ein zusätzliches Projekt zu koordinieren. Bei 9h zählt jedes gesparte Projekt-Setup.

**Login:** Nutzt ASP.NET Core Identity mit dem Standard-Scaffolding (`dotnet new webapp -au Individual` bzw. entsprechendes MVC-Template), Identity-Tabellen laufen über denselben SQLite-DbContext. Selbst gebaute Auth-Logik (Passwort-Hashing etc.) würde bei 9h unnötig Zeit fressen und ist fehleranfälliger – Identity ist hier die richtige Wahl, nicht "billig" gedacht, sondern Zeit sparen, wo es keinen Bewertungsvorteil bringt, es selbst zu bauen.

## 4. Team-Split (3 Personen)

Erst gemeinsam Interfaces festlegen (Kickoff), danach parallel:

| Person | Bereich | Aufgabe |
|---|---|---|
| **A** | Core + Data | Model `Aktie`, Interfaces, `DbContext` (SQLite, inkl. Identity), Migration, Repository-Implementierung |
| **B** | Web – API-Teil | API-Controller `/api/aktien` (Suche/CRUD), Service-Klasse (Geschäftslogik) implementiert `IAktieService`, DI-Registrierung |
| **C** | Web – UI-Teil | Login-Scaffolding (Identity), Startseite mit Suchfeld + JS-Aufruf gegen die API, einfaches Ergebnis-Layout |

Tests und Doku sind **kein Einzelposten für eine Person**, sondern laufen im letzten Zeitblock gemeinsam (siehe Zeitplan) – bei 9h ist "eine Person macht die ganze Doku allein" ein Risiko, falls diese Person nicht fertig wird.

## 5. Zeitplan (9h)

1. **0:00–0:45 – Kickoff:** Model `Aktie`, Interfaces `IAktieRepository`/`IAktieService`, Backlog/Taskboard mit MVP-Tickets (Login, Suche) + Stretch-Ticket (Vergleich).
2. **0:45–4:30 – Parallelarbeit:** A baut Data, B baut API+Service (gegen Interface, notfalls mit Mock-Daten), C baut Login + Startseite (UI erstmal mit Fake-Daten, bevor API steht).
3. **4:30–5:30 – Integration MVP:** Echte Repository-Implementierung einbinden, API + Login zusammenführen, End-to-End-Test: Login → Startseite → Suche liefert echte Daten aus SQLite.
4. **5:30–6:30 – Puffer für Stretch-Feature:** Nur starten, wenn MVP steht und stabil läuft. Sonst direkt zu Punkt 5.
5. **6:30–8:00 – Doku-Block (alle drei):** Technologieschema, UML-Sequenzdiagramm (Login-Flow und Such-Flow), ERM/RM, SOLID-Abweichungs-Doku, ein paar xUnit-Tests gegen `IAktieService`/`IAktieRepository` mit Moq.
6. **8:00–9:00 – Review/Puffer:** Gemeinsamer Durchlauf, letzte Fehler, Abgabe vorbereiten.

## 6. Annahmen

- Login ohne komplexe Rollen (nur "eingeloggt ja/nein" reicht für MVP).
- Kursdaten sind statisch/manuell eingepflegt, kein Live-Feed.
- Vergleichsfunktion ist bewusst nicht architektonisch vorgeplant – wird erst konkretisiert, wenn MVP steht, damit ihr keine Zeit in eine Funktion steckt, die am Ende evtl. gar nicht umgesetzt wird.

## 7. Doku-Checkliste (Pflicht laut Aufgabenstellung)

- [ ] Projektbeschreibung
- [ ] Klassendiagramm (Core-Modelle)
- [ ] Use-Case-Diagramm (Login, Aktie suchen, ggf. Aktien vergleichen)
- [ ] ERM + relationales Modell (Aktie, Identity-Tabellen)
- [ ] Technologieschema (Core/Data/Web-Layer, inkl. API-Aufruf von der Startseite)
- [ ] UML-Sequenzdiagramm mind. eines Ablaufs (Login-Flow oder Such-Flow)
- [ ] Struktogramm für einen Algorithmus (z. B. Such-/Filterlogik im Service)
- [ ] SOLID/Pattern-Doku wegen Technologie-Abweichung von WPF/MVVM
- [ ] Backlog + Taskboard mit MVP- und Stretch-Tickets

## 8. Prompt für Claude Code

```
Ich baue in 9h mit einem 3er-Team ein CRUD-Projekt "Aktienmarktplatz" in ASP.NET Core mit SQLite.
Lege eine Solution mit 3 Projekten an:

- AktienMarktplatz.Core (Class Library):
  Model Aktie (Symbol, Name, Kurs, Stückzahl), 
  Interfaces IAktieRepository und IAktieService (Suchmethode nach Symbol/Name).

- AktienMarktplatz.Data (Class Library):
  EF Core DbContext mit SQLite, kombiniert mit ASP.NET Core Identity (IdentityDbContext),
  AktieRepository implementiert IAktieRepository, erste Migration.

- AktienMarktplatz.Web (ASP.NET Core, Template mit Identity-Scaffolding "Individual Accounts"):
  - Login/Registrierung über ASP.NET Core Identity (Standard-Scaffolding, nicht selbst bauen)
  - API-Controller /api/aktien mit Such-Endpunkt (GET, Query-Parameter für Symbol/Name)
  - AktieService implementiert IAktieService, nutzt IAktieRepository per DI
  - Startseite (Razor) mit Suchfeld, ruft /api/aktien per fetch() aus JavaScript auf 
    und rendert Ergebnisse in einer Tabelle

Starte mit Core: Model + Interfaces, keine Implementierung.
Danach Data: DbContext (inkl. Identity), Migration, Repository-Implementierung.
Danach Web: zuerst Identity-Scaffolding für Login, danach API-Controller, danach Startseite.
```
