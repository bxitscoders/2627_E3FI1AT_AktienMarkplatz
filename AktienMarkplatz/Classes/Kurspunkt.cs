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
