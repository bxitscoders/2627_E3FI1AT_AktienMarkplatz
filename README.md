# AktienMarkplatz

## Tech-Status

**Status:** Planung / Projektstart steht noch aus

Das Projekt wurde bisher noch nicht umgesetzt. Die folgenden Punkte beschreiben die geplante Zielsetzung und den Umfang der ersten Version.

### Geplanter Tech-Stack

- **Backend:** C# mit ASP.NET Core
- **Framework:** .NET 10
- **Weboberfläche:** Razor Pages, HTML und CSS
- **Datenbank:** relationale Datenbank für Benutzerkonten, zum Beispiel SQLite
- **Aktienkurse:** externe Finanz-API
- **Authentifizierung:** sichere Anmeldung mit Session- oder ASP.NET-Identity-Unterstützung

## Zielsetzung

AktienMarkplatz soll eine Webanwendung werden, auf der sich Benutzer anmelden können und aktuelle Aktienkurse angezeigt bekommen. Die Kursdaten werden über eine externe API abgerufen und übersichtlich auf der Website dargestellt.

## Grundlegende Funktionen

### Benutzerverwaltung

- Benutzer können sich mit ihren Zugangsdaten einloggen.
- Login-Daten werden dauerhaft in einer Datenbank gespeichert.
- Passwörter werden aus Sicherheitsgründen nicht im Klartext gespeichert, sondern gehasht.
- Angemeldete Benutzer können sich wieder ausloggen.
- Geschützte Bereiche sind nur nach erfolgreicher Anmeldung erreichbar.

### Aktienkurse


### Tabs und persönliche Bereiche

- **Startseite:** Übersicht über aktuelle Aktienkurse und Marktdaten.
- **Profil:** Anzeige und spätere Verwaltung der persönlichen Benutzerdaten.
- **Kollektion:** Persönliche Sammlung gespeicherter Aktienkurse.
- Benutzer können Aktien markieren und zu ihrer Kollektion hinzufügen.
- Gespeicherte Aktien werden dauerhaft dem jeweiligen Benutzer zugeordnet.
- Markierte Aktien können in der Kollektion angezeigt und wieder entfernt werden.

## Geplanter MVP

6. Tabs für Startseite, Profil und Kollektion
7. Aktien markieren und in der persönlichen Kollektion speichern
8. Einfache, übersichtliche und responsive Benutzeroberfläche

1. Login-Seite
2. Speicherung und Prüfung der Benutzerdaten
7. Profil- und Kollektion-Tabs erstellen
8. Datenmodell für gespeicherte Aktien anlegen
9. Markieren, Anzeigen und Entfernen von Aktien implementieren
10. Fehlerbehandlung und Funktionstests durchführen
4. Geschütztes Benutzer-Dashboard
5. Abruf und Anzeige von Aktienkursen über eine API
6. Einfache, übersichtliche und responsive Benutzeroberfläche
## Geplanter Ablauf

1. Datenmodell für Benutzer und Login erstellen
2. Datenbank anbinden
3. Login und Logout implementieren
4. Zugriffsschutz für angemeldete Benutzer einrichten
5. Geeignete Aktien-API auswählen und anbinden
- Tabelle für Nutzer und Login-Daten
- Tabelle für gespeicherte Aktien beziehungsweise Kollektionen
- Verknüpfung zwischen Benutzer und gespeicherten Aktien
7. Fehlerbehandlung und Funktionstests durchführen

## Mögliche Erweiterungen

- Registrierung neuer Benutzer
- Suchfunktion für Aktien
- Watchlist beziehungsweise Favoriten
- Kursverläufe als Diagramme
- Persönliches Aktienportfolio
- Weitere Markt- und Unternehmensinformationen

## Kurzbeschreibung

AktienMarkplatz ist ein geplantes Aktienkurs- und Benutzerportal. Der aktuelle Stand ist die Konzept- und Planungsphase. Die Umsetzung beginnt mit den Kernfunktionen Login, sichere Speicherung der Benutzerdaten, Logout und Anzeige von Aktienkursen aus einer externen API.

