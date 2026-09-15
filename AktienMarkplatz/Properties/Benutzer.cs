

public class Benutzer
{
    public string Benutzername { get; set; }
    public string Passwort { get; private set; }

    public Benutzer(string benutzername, string passwort)
    {
        Benutzername = benutzername;
        Passwort = passwort;
    }
}