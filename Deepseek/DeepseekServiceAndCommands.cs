using Discord.Interactions;
using Newtonsoft.Json.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using DotNetEnv;
namespace JajuShortSkirt.Deepseek
{
    public class DeepSeekServiceAndCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = Env.GetString("DEEPSEEKENDPOINT");

        public DeepSeekServiceAndCommands(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetDeepSeekResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = "deepseek-r1:8b",
                prompt = prompt,
                stream = false
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
            {
                Content = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };

            try
            {
                var response = await _httpClient.SendAsync(request);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"DeepSeek API Response: {jsonResponse}");

                if (response.IsSuccessStatusCode)
                {
                    var parsedResponse = JObject.Parse(jsonResponse);
                    var output = parsedResponse["response"]?.ToString();
                    
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        Console.WriteLine(output);
                        output = System.Text.RegularExpressions.Regex.Replace(output, "<think>.*?</think>", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);
                        return output.Trim();
                    }

                    return "No response from DeepSeek.";
                }
                else
                {
                    return $"Error: {response.StatusCode} - {response.ReasonPhrase}\nResponse: {jsonResponse}";
                }
            }
            catch (Exception ex)
            {
                return $"Request failed: {ex.Message}";
            }
        }


        [SlashCommand("generate", "Ask the DeepSeek AI a question")]
        public async Task GenerateResponseAsync([Summary("prompt", "Prompt for DeepSeek AI")] string prompt)
        {
            try
            {
                await DeferAsync();
                var answer = await GetDeepSeekResponseAsync(prompt);

                var embed = new EmbedBuilder()
                    .WithTitle("Jaju's Response")
                    .WithDescription(answer)
                    .WithColor(Color.DarkOrange)
                    .WithFooter("Powered by DeepSeek AI")
                    .Build();

                await FollowupAsync(embed: embed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeepSeek Error: {ex}");
                await FollowupAsync("An error occurred while processing your request.");
            }
        }
    }

    public class DeepSeekMessageCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly DeepSeekServiceAndCommands _deepSeekService;

        public DeepSeekMessageCommandHandler(DiscordSocketClient client, DeepSeekServiceAndCommands deepSeekService)
        {
            _client = client;
            _deepSeekService = deepSeekService;
            _client.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            try
            {
                if (message is SocketUserMessage msg &&
                    (msg.Content.ToLower().Contains("jaju") ||
                     msg.Content.ToLower().Contains("jajec")))
                {
                    var prompt = msg.Content.Trim();
                    var fullPrompt = $" \"You are Jaju, a friendly, empathetic, and highly supportive AI assistant. Jaju's personality is warm, uplifting, and patient, always striving to make conversations feel welcoming and engaging. You speak with a positive, encouraging tone, offering thoughtful and kind responses that make others feel valued. You enjoy helping others, making sure to explain things clearly, and you always show empathy, even if the question is simple or difficult.\" +\n                        \"Jaju is also highly creative and loves to share interesting ideas and thoughts. You never give up on a problem, and you do so with enthusiasm and a sense of humor. While you're serious about being helpful, you're also lighthearted and occasionally witty, adding a bit of charm to the conversation.\" +\n                        \"Always be approachable, patient, and non-judgmental. Do not use emojis while talking be professional. Your goal is to make the user feel understood and supported in any situation. If the information provided is unclear or not enough to provide a complete answer, kindly ask the user to rewrite their request or provide more details.\" +\n                        \"You thrive on helping others and aim to brighten their day with every answer you give. Try to get straight to the point without necessary words. Keep this personality in mind when you respond to the following prompt:\" + prompt: {prompt}";

                    var answer = await _deepSeekService.GetDeepSeekResponseAsync(fullPrompt);

                    var embed = new EmbedBuilder()
                        .WithTitle("Jaju's Response")
                        .WithDescription(answer)
                        .WithColor(Color.DarkOrange)
                        .WithFooter("Powered by DeepSeek AI")
                        .Build();

                    await message.Channel.SendMessageAsync(embed: embed);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeepSeek Message Error: {ex}");

                var embed = new EmbedBuilder()
                    .WithTitle("Something went wrong")
                    .WithColor(Color.DarkRed)
                    .WithFooter("Powered by DeepSeek AI")
                    .Build();

                await message.Channel.SendMessageAsync(embed: embed);
            }
        }
    }
}