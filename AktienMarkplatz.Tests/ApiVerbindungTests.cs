using System.Net;
using System.Net.Sockets;
using System.Text;
using AktienMarkplatz.API;

namespace AktienMarkplatz.Tests
{
    public class ApiVerbindungTests
    {
        // Macht die geschuetzte Methode AbfrageAusfuehren fuer den Test aufrufbar.
        private class TestVerbindung : ApiVerbindung
        {
            public TestVerbindung() : base("")
            {
            }

            public Task<Dictionary<string, string>?> Abfragen(string url)
            {
                return AbfrageAusfuehren<Dictionary<string, string>>(url);
            }
        }

        [Fact]
        public async Task Antwort_ohne_JSON_liefert_null_statt_Exception()
        {
            // Kleiner lokaler Server, der mit Status 200 HTML statt JSON antwortet.
            var server = new TcpListener(IPAddress.Loopback, 0);
            server.Start();
            int port = ((IPEndPoint)server.LocalEndpoint).Port;

            Task serverTask = Task.Run(async () =>
            {
                using TcpClient client = await server.AcceptTcpClientAsync();
                using NetworkStream stream = client.GetStream();
                await stream.ReadAsync(new byte[4096]);

                string body = "<html>";
                string antwort = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n"
                    + $"Content-Length: {body.Length}\r\nConnection: close\r\n\r\n{body}";
                await stream.WriteAsync(Encoding.ASCII.GetBytes(antwort));
            });

            Dictionary<string, string>? ergebnis = await new TestVerbindung().Abfragen($"http://127.0.0.1:{port}/");

            await serverTask;
            server.Stop();
            Assert.Null(ergebnis);
        }
    }
}
