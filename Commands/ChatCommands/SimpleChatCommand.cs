using Discord.WebSocket;

namespace JajuShortSkirt.Commands.ChatCommands;

public class SimpleChatCommand
{
    private readonly DiscordSocketClient _client;
    private bool _cancelCountdown;

    public SimpleChatCommand(DiscordSocketClient client)
    {
        _client = client;
        _client.MessageReceived += OnMessageReceivedAsync;
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot)
        {
            return;
        }

        Console.WriteLine($"Send by: {message.Author} on {message.Channel} content: {message}");
            
        if (message.ToString().ToLower().Contains("gong"))
        {
            await message.Channel.SendMessageAsync("https://tenor.com/view/labul-gong-gif-25780334");
        }
    }
}