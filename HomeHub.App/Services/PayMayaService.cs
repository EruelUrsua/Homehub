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

        /*public async Task<string> PayOnline(decimal amount, string orderRef)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                string base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(_publicKey));
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64Auth);

                var requestBody = new
                {
                    totalAmount = new { value = amount, currency = "PHP" },
                    requestReferenceNumber = orderRef,
                    redirectUrl = new
                    {
                        //replace these
                        success = "https://yourwebsite.com/success",
                        failure = "https://yourwebsite.com/failure",
                        cancel = "https://yourwebsite.com/cancel"
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // If successful, read the response content (e.g., QR code URL)
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return responseJson;
                }
                else
                {
                    // If request fails, log or handle error
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error: " + errorResponse);
                    return null;
                }
            }
        }*/

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
                            value = totalAmount, // Calculate total price
                            currency = currency
                        };
                    }
                }

                var checkoutRequest = new PayMayaCheckoutRequest
                {
                    //totalAmount = new PayMayaAmount { value = totalAmount, currency = currency },
                    totalAmount = new PayMayaAmount
                    {
                        value = totalAmount,  // ✅ Use totalAmount inside PayMayaAmount correctly
                        currency = currency
                    },
                    items = items, // List of items passed dynamically
                    //requestReferenceNumber = "ORDER-" + logId // Dynamic Log ID
                    requestReferenceNumber = logId
                };

                /*
                var checkoutRequest = new PayMayaVM.PayMayaCheckoutRequest
                {
                    //totalAmount = new PayMayaVM.PayMayaAmount { totalAmount = totalAmount, currency = currency },
                    //items = items,
                    //requestReferenceNumber = "ORDER-" + logId

                    totalAmount = new PayMayaVM.PayMayaAmount
                    {
                        value = totalAmount,  // ✅ Fix: Ensure Value property is set
                        currency = currency
                    },
                    items = items ?? throw new ArgumentException("Items list cannot be null."),
                    requestReferenceNumber = "ORDER-" + logId
                };

                for multiple order
                foreach (var item in checkoutRequest.items)
                {
                    if (item.totalAmount == null)
                    {
                        item.totalAmount = new PayMayaVM.PayMayaAmount
                        {
                            Value = item.amount.Value * item.quantity,  // ✅ Calculate total price
                            Currency = currency
                        };
                    }
                }*/

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
                    return responseContent; // Returns JSON response from PayMaya
                }
                else
                {
                    throw new Exception("Failed to create PayMaya checkout: " + await response.Content.ReadAsStringAsync());
                }
            }
        }
    }
}
