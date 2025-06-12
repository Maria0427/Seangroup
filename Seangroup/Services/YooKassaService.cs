using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;


namespace Seangroup.Services
{
    public class YooKassaService
    {
        
            private readonly HttpClient _httpClient;
            private readonly YooKassaOptions _options;

            public YooKassaService(HttpClient httpClient, IOptions<YooKassaOptions> options)
            {
                _httpClient = httpClient;
                _options = options.Value;
            }

            public async Task RefundPaymentAsync(string idempotenceKey, string paymentId, decimal amount)
            {
                var url = "https://api.yookassa.ru/v3/refunds";

                var requestBody = new
                {
                    amount = new
                    {
                        value = amount.ToString("F2", CultureInfo.InvariantCulture),
                        currency = "RUB"
                    },
                    payment_id = paymentId
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                requestMessage.Headers.Add("Idempotence-Key", idempotenceKey);
                var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ShopId}:{_options.SecretKey}"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                var response = await _httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка возврата: {error}");
                }
            }
        }
}
