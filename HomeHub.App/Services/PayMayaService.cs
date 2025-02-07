using System.Text.Json;
using System.Text;

namespace HomeHub.App.Services
{
    public class PayMayaService
    {
        private readonly string _apiUrl = "https://pg-sandbox.paymaya.com/payments/v1/qr/payments";
        private readonly string _publicKey = "pk-MOfNKu3FmHMVHtjyjG7vhr7vFevRkWxmxYL1Yq6iFk5";

        public async Task<string> PayOnline(decimal amount, string orderRef)
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
        }
    }
}
