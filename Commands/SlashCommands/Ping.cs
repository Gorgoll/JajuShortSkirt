using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace JajuShortSkirt.Commands.SlashCommands;

public class PingSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;
        
    public PingSlashCommand(DiscordSocketClient client)
    {
        _client = client;
    }

    [SlashCommand("ping", "Check your latency.")]
    public async Task Ping()
    {
        float latency = _client.Latency;

        var embed = new EmbedBuilder()
            .WithTitle("Pong!")
            .WithDescription($"Latency: {latency} ms")
            .WithColor(Color.Blue)
            .WithAuthor(Context.User)
            .Build();
        await RespondAsync(embed: embed);
    }
}