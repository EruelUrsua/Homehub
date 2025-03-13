using System.Text.Json;
using System.Text;
using HomeHub.App.Models;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static HomeHub.App.Models.PayMayaVM;
using HomeHub.DataModel;

namespace HomeHub.App.Services
{
    public class PayMayaService
    {
        private readonly string _apiUrl = "https://pg-sandbox.paymaya.com/checkout/v1/checkouts";
        private readonly string _publicKey = "pk-Z0OSzLvIcOI2UIvDhdTGVVfRSSeiGStnceqwUE7n0Ah";
        private readonly string _secretKey = "sk-X8qolYjy62kIzEbr0QRK1h4b4KDVHaNcwMYk39jInSl";

        public async Task<string> CreateCheckoutAsync(decimal totalAmount, string currency, List<PayMayaVM.PayMayaItem> items, string logId)
        {
            using (var httpClient = new HttpClient())
            {
                // Set headers
                httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_publicKey)));

                foreach (var item in items)
                {
                    if (item.totalAmount == null)
                    {
                        item.totalAmount = new PayMayaAmount
                        {
                            value = totalAmount,
                            currency = currency
                        };
                    }
                }

                var checkoutRequest = new PayMayaCheckoutRequest
                {
                    totalAmount = new PayMayaAmount
                    {
                        value = totalAmount,
                        currency = currency
                    },
                    items = items,
                    requestReferenceNumber = logId,

                    redirectUrl = new PayMayaRedirectUrl
                    {
                        success = $"https://localhost:44302/Customer/PaymentSuccess?requestReferenceNumber={logId}",
                        failure = $"https://localhost:44302/Customer/PaymentFailure?requestReferenceNumber={logId}",
                        cancel = $"https://localhost:44302/Customer/PaymentCancel?requestReferenceNumber={logId}"
                    }
                };

                var jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Newtonsoft.Json.Formatting.Indented
                };
                string json = JsonConvert.SerializeObject(checkoutRequest, jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
                else
                {
                    throw new Exception("Failed to create PayMaya checkout: " + await response.Content.ReadAsStringAsync());
                }
            }
        }

        /*
        public async Task RegisterWebhooksAsync()
        {
            var client = new HttpClient();
            string[] webhookEvents = { "CHECKOUT_SUCCESS", "PAYMENT_SUCCESS", "PAYMENT_FAILED", "PAYMENT_EXPIRED" };

            foreach (var webhookEvent in webhookEvents)
            {
                var requestData = new
                {
                    name = webhookEvent,
                    callbackUrl = "https://yourdomain.com/api/paymaya/webhook"
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://pg-sandbox.paymaya.com/checkout/v1/webhooks")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_secretKey)));

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to register {webhookEvent}: {error}");
                }
            }
        */


        public async Task RegisterWebhookAsync()
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://pg-sandbox.paymaya.com/checkout/v1/webhooks");

                var requestData = new[]
                {
                    new { name = "CHECKOUT_SUCCESS", callbackUrl = "https://randomstring.ngrok.io/api/paymaya/webhook" },
                    new { name = "CHECKOUT_FAILURE", callbackUrl = "https://randomstring.ngrok.io/api/paymaya/webhook" },
                    new { name = "CHECKOUT_DROPOUT", callbackUrl = "https://randomstring.ngrok.io/api/paymaya/webhook" }
                };

                request.Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_secretKey)));

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to register webhook: {responseBody}");
                }
            }
        }

    }

}

