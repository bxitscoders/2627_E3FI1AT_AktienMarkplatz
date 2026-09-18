using System.Data;

namespace AktienMarkplatz.Classes
{
    public class Transaktion
    {
        public Guid Id { get; set; }
        public DataSetDateTime Datum { get; set; }
        public Aktie Aktie { get; set; }
        public int Anzahl { get; set; }
        public double PreisPro { get; set; }

    }
}
