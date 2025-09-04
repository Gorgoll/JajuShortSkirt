
using Discord;
using Discord.Interactions;
using JajuShortSkirt.Database;
using Microsoft.EntityFrameworkCore;

namespace JajuShortSkirt.SlashCommands.Economy;

public class EconomyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ApplicationDbContext _dbContext;

    public EconomyModule(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [SlashCommand("work", "Earn eggs by working.")]
    public async Task Work()
    {
        ulong userId = Context.User.Id;
        Random random = new Random();
        int coinsEarned = random.Next(200, 801);
        TimeSpan cooldown = TimeSpan.FromHours(8);
        var embed = new EmbedBuilder();
            
        try
        {
            var userCoinBalance = _dbContext.CoinBalance.FirstOrDefault(cb => cb.DiscordUserId == userId);
        
            if (userCoinBalance != null)
            {
                if (DateTime.UtcNow - userCoinBalance.LastWorked < cooldown)
                {
                    TimeSpan remainingTime = cooldown - (DateTime.UtcNow - userCoinBalance.LastWorked);
                    
                    embed
                        .WithDescription($"{Context.User.Mention}, you can only work once every 8 hours. Please wait {remainingTime.TotalMinutes:F0} minutes before working again.")
                        .WithColor(Color.LighterGrey);

                    await RespondAsync(embed: embed.Build());
                    return;
                }
                
                userCoinBalance.Balance += coinsEarned;
                userCoinBalance.LastWorked = DateTime.UtcNow;
            }
            else
            {
                userCoinBalance = new CoinBalance
                {
                    DiscordUserId = userId,
                    Balance = coinsEarned,
                    LastWorked = DateTime.UtcNow
                };
                _dbContext.CoinBalance.Add(userCoinBalance);
            }

            await _dbContext.SaveChangesAsync();
            embed
                .WithDescription($"{Context.User.Mention}, you worked hard and earned {coinsEarned} eggs! Your total balance is now {userCoinBalance.Balance} eggs.")
                .WithColor(Color.LightOrange)
                .Build();

            await RespondAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await RespondAsync("An error occurred while processing your request.");
            Console.WriteLine(ex.Message);
        }
    }


    [SlashCommand("balance", "Check your total egg balance.")]
    public async Task Balance()
    {
        ulong userId = Context.User.Id;
        var embed = new EmbedBuilder();
        
        try
        {
            var userCoinBalance = _dbContext.CoinBalance.FirstOrDefault(cb => cb.DiscordUserId == userId);
            
            if (userCoinBalance != null)
            {
                embed
                    .WithDescription($"{Context.User.Mention}, you have a total of {userCoinBalance.Balance} eggs.")
                    .WithColor(Color.Gold);
                
                await RespondAsync(embed: embed.Build());
            }
            else
            {
                embed
                    .WithDescription($"{Context.User.Mention}, you currently have no eggs.")
                    .WithColor(Color.LightOrange);
                
                await RespondAsync(embed: embed.Build());
            }
        }
        catch (Exception ex)
        {
            await RespondAsync("An error occurred while retrieving your balance.");
            Console.WriteLine(ex.Message);
        }
    }
    [SlashCommand("leaderboard", "Display the top 10 users with the highest egg balance.")]
    public async Task Leaderboard()
    {
        try
        {
            var topUsers = _dbContext.CoinBalance
                .OrderByDescending(cb => cb.Balance)
                .Take(10)
                .ToList();

            if (topUsers.Count > 0)
            {
                var embedBuilder = new EmbedBuilder()
                    .WithTitle("🥚 Top 10 Users with the Highest Egg Balance")
                    .WithColor(Color.Gold)
                    .WithTimestamp(DateTimeOffset.UtcNow);

                foreach (var user in topUsers)
                {
                    var discordUser = await Context.Client.GetUserAsync(user.DiscordUserId);
                    string userMention = discordUser != null ? discordUser.Mention : "Unknown User";
                    
                    embedBuilder.AddField($"{discordUser?.Username ?? " Unknown User"}", $"{userMention} - {user.Balance} eggs", inline: false);
                }
                await RespondAsync(embed: embedBuilder.Build());
            }
            else
            {
                await RespondAsync("There are no users on the leaderboard yet.");
            }
        }
        catch (Exception ex)
        {
            await RespondAsync("An error occurred while retrieving the leaderboard.");
            Console.WriteLine(ex.Message);
        }
    }
    
    [SlashCommand("flip", "Flip the coin and bet some eggs!")]
    public async Task Flip([Summary("bet", "number of eggs you bet")] int betAmount = 0)
    {
        var userId = Context.User.Id;
        var coinBalance = await _dbContext.CoinBalance.FirstOrDefaultAsync(u => u.DiscordUserId== userId);
        var embed = new EmbedBuilder();
            
        if (coinBalance == null)
        {
            embed
                .WithTitle("You don't have any eggs to bet.")
                .WithColor(Color.Blue);
                
            await RespondAsync(embed: embed.Build());
            return;
        }
            
        if (coinBalance.Balance < betAmount)
        {
            embed
                .WithTitle("You're too poor! You don't have enough eggs to bet.")
                .WithColor(Color.Blue);
                
            await RespondAsync(embed: embed.Build());
            return;
        }

        string result = new Random().Next(0, 2) == 0 ? "Heads" : "Tails";
        bool wonBet = result == "Heads"; 

        if (betAmount > 0)
        {
            if (wonBet)
            {
                coinBalance.Balance += betAmount;

                embed
                    .WithTitle($"The coin landed on **{result}**! You won {betAmount*2} and now have {coinBalance.Balance} eggs.")
                    .WithColor(result == "Heads" ? Color.DarkGreen : Color.Gold);
                    
                await RespondAsync(embed: embed.Build());
            }
            else
            {
                coinBalance.Balance -= betAmount;

                embed
                    .WithTitle($"The coin landed on **{result}**. You lost {betAmount} and now have {coinBalance.Balance} eggs.")
                    .WithColor(result == "Heads" ? Color.DarkGreen : Color.DarkRed);

                await RespondAsync(embed: embed.Build());
            }
                
            _dbContext.CoinBalance.Update(coinBalance);
            await _dbContext.SaveChangesAsync();
        }
        else
        {

            embed
                .WithTitle($"The coin landed on **{result}**!")
                .WithColor(result == "Heads" ? Color.DarkGreen : Color.Green);

            await RespondAsync(embed: embed.Build());
        }
    }
}