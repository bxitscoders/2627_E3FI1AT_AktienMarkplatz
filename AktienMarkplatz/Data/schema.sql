-- Aktienmarktplatz - SQLite Schema
-- Erzeugt die Datenbank aktienmarktplatz.db
-- Ausfuehren z.B. mit: sqlite3 aktienmarktplatz.db < schema.sql

DROP TABLE IF EXISTS Benutzer;

CREATE TABLE Benutzer (
    Id            INTEGER PRIMARY KEY,
    Name          VARCHAR(255)   NOT NULL UNIQUE,
    PasswortHash  VARCHAR(255)   NOT NULL
);

-- Entwicklungs-Testdaten
-- Testpasswoerter: Test123!, Aktien2026!, DemoPasswort!
INSERT INTO Benutzer (Id, Name, PasswortHash) VALUES
    (1, 'max', 'Baum'),
    (2, 'anna', 'Tree'),
    (3, 'demo', 'Black');
