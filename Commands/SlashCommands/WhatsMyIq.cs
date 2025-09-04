using Discord;
using Discord.Interactions;

namespace JajuShortSkirt.Commands.SlashCommands;

public class WhatsMyIq : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("whats_my_iq", "Check your IQ!")]
    public async Task RandomNumberAsync([Summary("user", "Check the IQ of another user")] IUser? user= null)
    {
        user ??= Context.User;
        string result = new Random().Next(50, 151).ToString();
        
        var embed = new EmbedBuilder()
            .WithDescription($"**{user.Mention}, your IQ is: {result}!**")
            .WithColor(Color.Green)
            .Build();
        await RespondAsync(embed: embed);
            
    }
}