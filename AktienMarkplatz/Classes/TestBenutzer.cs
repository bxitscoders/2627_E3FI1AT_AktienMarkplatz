using System.ComponentModel;

namespace AktienMarkplatz.Classes
{
    public class TestBenutzer
    {
        public string mail;
        enum testBenutzer
        {
            [Description("SchuleTest123@123")]
           mail,
            [Description("!SchuleTest123")] 
          pw
            
        }
        enum testBenutzer2
        {
            [Description("test@123")]
            mail,
            [Description("test12")]
            pw

        }
    }
}
