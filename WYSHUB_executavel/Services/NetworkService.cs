using System.Net.Http;
using System.Threading.Tasks;

namespace SystemWM.Services
{
    public class NetworkService
    {
        public async Task<string> ObterIpPublicoAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = System.TimeSpan.FromSeconds(4);
                var ip = await client.GetStringAsync("https://api.ipify.org");
                return ip.Trim();
            }
            catch
            {
                return "Indisponível";
            }
        }
    }
}
