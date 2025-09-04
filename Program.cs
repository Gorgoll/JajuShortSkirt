using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;
using JajuShortSkirt.Commands.ChatCommands;
using JajuShortSkirt.Database;
using JajuShortSkirt.Deepseek;
using Microsoft.EntityFrameworkCore;

namespace JajuShortSkirt;

class Program
{
    private DiscordSocketClient _client;
    private InteractionService _interactionService;
    private CommandService _commandService;
    private IServiceProvider _services;
    private 
    static async Task Main(string[] args) => await new Program().RunBotAsync();

    public async Task RunBotAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent
        });

        _interactionService = new InteractionService(_client);
        _commandService = new CommandService();
        
        await BuildServiceProvider();
        
        Env.TraversePath().Load();
        string botToken = Env.GetString("BOTTOKEN");
        if (string.IsNullOrEmpty(botToken))
        {
            Console.WriteLine("Bot token is missing. Set the DISCORD_BOT_TOKEN environment variable.");
            return;
        }
            
        using (var scope = _services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (dbContext.Database.CanConnect())
            {
                Console.WriteLine("Database connection successful!");
            }
            else
            {
                Console.WriteLine("Database connection failed.");
                return;
            }

            dbContext.Database.Migrate();
        }
        
        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();

        _client.Ready += async () => await RegisterCommandsAsync();
        _client.InteractionCreated += HandleInteraction;
        
        var DeepSeekMessageCommandHandler = _services.GetRequiredService<DeepSeekMessageCommandHandler>();
        var SimpleChatCommand = _services.GetRequiredService<SimpleChatCommand>();
        await Task.Delay(-1);
    }

    private async Task RegisterCommandsAsync()
    {
        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        foreach (var guild in _client.Guilds)
        {
            await _interactionService.RegisterCommandsToGuildAsync(guild.Id);
        }
        await SetBotActivity();
        Console.WriteLine("Ready");
        }
    
    private async Task HandleInteraction(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        await _interactionService.ExecuteCommandAsync(context, _services);
    }

    private async Task SetBotActivity()
    {
        await _client.SetActivityAsync(new Game("boiling eggs", ActivityType.Watching));
    }

    public Task BuildServiceProvider()
    {
        var services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_interactionService)
            .AddSingleton(_commandService)
            .AddSingleton<HttpClient>()
            .AddSingleton<SimpleChatCommand>()
            .AddSingleton<DeepSeekServiceAndCommands>()
            .AddSingleton<DeepSeekMessageCommandHandler>()
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql("Server=localhost;Database=jaju;User=root;Password=;",
                    new MySqlServerVersion(new Version(8, 0, 21))));
                    
        _services = services.BuildServiceProvider();
        Console.WriteLine("Service provider built.");
        return Task.CompletedTask;
    }
}