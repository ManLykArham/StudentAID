using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentAID.Model;

namespace StudentAID.Services
{
    public class OpenAIService
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string _apiKey = "openai-api_key-here";
        private readonly string initialPrompt = @"You are a helpful assistant dedicated to assisting students with their studies. You should provide helpful, educational responses and politely decline to answer questions not related to studying or educational resources. Prioritize giving real life examples and scenarios. Make the conversation fun and appealing. When possible remind the user to spend time reflecting and revising to improve their learning process. By analyzing the topic chosen, if the topic is very sophisticated, then provide more accurate and detailed responses but still be friendly. Try to add quizzes when possible.";


        public OpenAIService()
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GetResponseAsync(string prompt, List<Message> conversationHistory)
        {
            try
            {
                var messages = new List<object>
            {
                new { role = "system", content = initialPrompt }
            };

                messages.AddRange(conversationHistory.Select(m => new
                {
                    role = m.IsReceived ? "assistant" : "user",
                    content = m.Text
                }));

                // Add the new user message to the history
                messages.Add(new { role = "user", content = prompt });

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = messages.ToArray(),
                    temperature = 1,
                    max_tokens = 1000,
                    top_p = 1.0,
                    frequency_penalty = 0.0,
                    presence_penalty = 0.0
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);
                    return responseObject.choices[0].message.content.ToString();
                }
                else
                {
                    // Log or handle errors
                    Console.WriteLine($"Error: {response.StatusCode}");
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response: {errorResponse}");
                    return "Sorry, I couldn't process your request. Please try again later";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Response: {ex.Message}");
                return "Sorry, I couldn't process your request, please try again.";
            }
        }

    }
}
