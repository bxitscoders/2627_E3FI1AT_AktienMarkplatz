-- Aktienmarktplatz - SQLite Schema
-- Erzeugt die Datenbank aktienmarktplatz.db
-- Ausfuehren z.B. mit: sqlite3 aktienmarktplatz.db < schema.sql

DROP TABLE IF EXISTS Test;

CREATE TABLE Test (
    Id          INTEGER PRIMARY KEY,
    Name        TEXT    NOT NULL
);

INSERT INTO Test ( id, Name) VALUES
    (1, 'Testeintrag 1'),
    (2, 'Testeintrag 2'),
    (3, 'Testeintrag 3');
