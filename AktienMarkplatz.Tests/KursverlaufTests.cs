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
