using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DoAnTeam12.Services
{
    public class AI
    {
        private static readonly string apiKey = System.Configuration.ConfigurationManager.AppSettings["OpenAIApiKey"];
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        public async Task<string> GetGPTResponse(string prompt)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    return result.choices[0].message.content.ToString();
                }

                return "Lỗi: " + responseString;
            }
        }
    }
}
