using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using TechMove.GLMS.Interfaces;

namespace TechMove.GLMS.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _http;

        public CurrencyService(HttpClient http)
        {
            _http = http;
        }

        public virtual async Task<decimal> ConvertUsdToZarAsync(decimal usdAmount)
        {
            var response = await _http.GetAsync("https://api.exchangerate-api.com/v4/latest/USD");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<ExchangeRateResponse>(json);

            if (data == null || !data.Rates.ContainsKey("ZAR"))
                throw new Exception("ZAR rate not found.");

            decimal rate = data.Rates["ZAR"];
            return usdAmount * rate;
        }
    }

    public class ExchangeRateResponse
    {
        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
