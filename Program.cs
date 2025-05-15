using JwtTechTask.Models;
using JwtTechTask.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JwtTechTask
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string jwt = "eyJhbGciOiJub25lIn0.eyJkYXRhIjpbeyJ1c2VySWQiOiIxMjM0NSIsInRyYW5zYWN0aW9ucyI6W3siaWQiOiIxIiwiYW1vdW50Ijo1MCwiY3VycmVuY3kiOiJVQUgiLCJtZXRhIjp7InNvdXJjZSI6IkNBQiIsImNvbmZpcm1lZCI6dHJ1ZX0sInN0YXR1cyI6IkNvbXBsZXRlZCJ9LHsiaWQiOiIyIiwiYW1vdW50IjozMC41LCJjdXJyZW5jeSI6IlVBSCIsIm1ldGEiOnsic291cmNlIjoiQUNCIiwiY29uZmlybWVkIjpmYWxzZX0sInN0YXR1cyI6IkluUHJvZ3Jlc3MifSx7ImlkIjoiMyIsImFtb3VudCI6ODkuOTksImN1cnJlbmN5IjoiVUFIIiwibWV0YSI6eyJzb3VyY2UiOiJDQUIiLCJjb25maXJtZWQiOnRydWV9LCJzdGF0dXMiOiJDb21wbGV0ZWQifV19LHsidXNlcklkIjoidTEyMyIsInRyYW5zYWN0aW9ucyI6W3siaWQiOiIxIiwiYW1vdW50Ijo0NDM0LCJjdXJyZW5jeSI6IkVVUiIsIm1ldGEiOnsic291cmNlIjoiQ0FCIiwiY29uZmlybWVkIjp0cnVlfSwic3RhdHVzIjoiQ29tcGxldGVkIn0seyJpZCI6IjIiLCJhbW91bnQiOjU2LjUzLCJjdXJyZW5jeSI6IlVBSCIsIm1ldGEiOnsic291cmNlIjoiQUNCIiwiY29uZmlybWVkIjpmYWxzZX0sInN0YXR1cyI6Mn1dfV19.";
            //string jwt = "eyJhbGciOiJub25lIn0.eyJkYXRhIjpbeyJ1c2VySWQiOiIxMjM0NSIsInRyYW5zYWN0aW9ucyI6W3siaWQiOiIxIiwiYW1vdW50Ijo1MCwiY3VycmVuY3kiOiJVU0QiLCJtZXRhIjp7InNvdXJjZSI6IkNBQiIsImNvbmZpcm1lZCI6dHJ1ZX0sInN0YXR1cyI6IkNvbXBsZXRlZCJ9LHsiaWQiOiIyIiwiYW1vdW50IjozMC41LCJjdXJyZW5jeSI6IkVVUiIsIm1ldGEiOnsic291cmNlIjoiQUNCIiwiY29uZmlybWVkIjp0cnVlfSwic3RhdHVzIjoiSW5Qcm9ncmVzcyJ9LHsiaWQiOiIzIiwiYW1vdW50Ijo4OS45OSwiY3VycmVuY3kiOiJVQUgiLCJtZXRhIjp7InNvdXJjZSI6IkNBQiIsImNvbmZpcm1lZCI6dHJ1ZX0sInN0YXR1cyI6IkNvbXBsZXRlZCJ9XX0seyJ1c2VySWQiOiJ1MTIzIiwidHJhbnNhY3Rpb25zIjpbeyJpZCI6IjEiLCJhbW91bnQiOjQ0MzQsImN1cnJlbmN5IjoiRVVSIiwibWV0YSI6eyJzb3VyY2UiOiJDQUIiLCJjb25maXJtZWQiOnRydWV9LCJzdGF0dXMiOiJDb21wbGV0ZWQifSx7ImlkIjoiMiIsImFtb3VudCI6NTYuNTMsImN1cnJlbmN5IjoiVUFIIiwibWV0YSI6eyJzb3VyY2UiOiJBQ0IiLCJjb25maXJtZWQiOmZhbHNlfSwic3RhdHVzIjoyfV19XX0.";

            // TODO: Step 1: Decode the JWT and extract the payload

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var payloadJson = JsonSerializer.Serialize(token.Payload);
            Console.WriteLine("Payload JSON: " + payloadJson);

            // TODO: Step 2: Deserialize the payload into C# objects

            Users? payloadModel;
            try
            {
                payloadModel = JsonSerializer.Deserialize<Users>(payloadJson, new JsonSerializerOptions()
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Error deserializing payload: " + ex.Message);
                return;
            }

            if (payloadModel?.Data == null || !payloadModel.Data.Any())
            {
                Console.WriteLine("No users found in payload.");
                return;
            }

            // TODO: Step 3: Print user ID, transaction count, total confirmed amount

            var httpClient = new HttpClient();
            var exchangeService = new ExchangeRateService(httpClient);

            foreach (var user in payloadModel.Data)
            {
                var transactions = user.Transactions ?? new List<Transaction>();
                var confirmedTransactions = transactions
                    .Where(t => t.Meta?.Confirmed == true && t.Amount.HasValue && !string.IsNullOrWhiteSpace(t.Currency))
                    .ToList();

                Console.WriteLine($"\nUser ID: {user.UserId}");

                transactions.ForEach(t =>
                    Console.WriteLine($"  - Tx ID: {t.Id}, Amount: {t.Amount}, Currency: {t.Currency}, Confirmed: {t.Meta.Confirmed}, Status: {t.Status}")
                );

                Console.WriteLine($"Transaction Count: {transactions.Count}");

                var distinctCurrencies = confirmedTransactions
                    .Select(t => t.Currency)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var results = new List<string>();

                foreach (var targetCurrency in distinctCurrencies)
                {
                    decimal total = 0;

                    foreach (var tx in confirmedTransactions)
                    {
                        var amount = tx.Amount!.Value;
                        var fromCurrency = tx.Currency;

                        if (fromCurrency != targetCurrency)
                        {
                            amount = await exchangeService.ConvertAsync(amount, fromCurrency, targetCurrency);
                        }

                        total += amount;
                    }

                    results.Add($"{total:F2} {targetCurrency}");
                }

                Console.WriteLine("Total Confirmed Amount: " + string.Join(" / ", results));
            }
         
        }
    }
}
