using System.Net.Http.Json;

namespace JwtTechTask.Service
{
    public class ExchangeRateService
    {
        private readonly HttpClient _httpClient;

        public ExchangeRateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
                return amount;

            var url = $"https://open.er-api.com/v6/latest/{fromCurrency}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<ErApiResponse>(url);
                if (response?.Rates is not null &&
                response.Rates.TryGetValue(toCurrency.ToUpperInvariant(), out var rate))
                {
                    return amount * rate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Currency conversion error: {ex.Message}");
            }

            return 0;
        }
    }
}
