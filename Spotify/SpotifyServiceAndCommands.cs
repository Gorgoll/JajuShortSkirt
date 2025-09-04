using Discord;
using Discord.Interactions;
using DotNetEnv;
using SpotifyAPI.Web;
using Timer = System.Timers.Timer;

namespace JajuShortSkirt.Spotify;

public class SpotifyServiceAndCommands
{
    public class SpotifyService
    {
        private static SpotifyService _instance;
        private static readonly object _lock = new object();

        private Timer _spotifyTokenRefreshTimer;
        private SpotifyClient _spotifyClient;

        public static SpotifyService Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new SpotifyService();
                }
            }
        }

        public SpotifyClient SpotifyClient => _spotifyClient;

        public async Task Initialize()
        {
            await UpdateSpotifyClient();
        }

        private void StartSpotifyTokenRefreshTimer()
        {
            _spotifyTokenRefreshTimer = new Timer(1803 * 1000);
            _spotifyTokenRefreshTimer.Elapsed += async (sender, e) => { await UpdateSpotifyClient(); };
            _spotifyTokenRefreshTimer.Start();
        }

        private async Task<SpotifyClient> CreateSpotifyClient()
        {
            var config = SpotifyClientConfig.CreateDefault();
            Env.TraversePath().Load();

            var request = new ClientCredentialsRequest(
                Env.GetString("SPOTIFYCLIENTID"),
                Env.GetString("SPOTIFYCLIENTSECRET"));

            var tokenResponse = await new OAuthClient(config).RequestToken(request);
            return new SpotifyClient(config.WithToken(tokenResponse.AccessToken));
        }

        public async Task UpdateSpotifyClient()
        {
            _spotifyClient = await CreateSpotifyClient();
            StartSpotifyTokenRefreshTimer();
        }
    }


    public class SpotifyCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("search", "Search for a Spotify track and get its details")]
        public async Task SearchTrack(string trackName)
        {
            var spotifyService = SpotifyService.Instance;

            if (spotifyService.SpotifyClient == null)
            {
                await spotifyService.Initialize();
            }

            var spotify = spotifyService.SpotifyClient;
            string responseMessage;
            try
            {
                Console.WriteLine($"Received request to search for track: '{trackName}'");
                var searchRequest = new SearchRequest(SearchRequest.Types.Track, trackName);
                var searchResult = await spotify.Search.Item(searchRequest);

                if (searchResult.Tracks.Items != null && searchResult.Tracks.Items.Count == 0)
                {
                    responseMessage = $"Couldn't find a track named '{trackName}' on Spotify.";
                    Console.WriteLine(responseMessage);
                    await RespondAsync(responseMessage, ephemeral: true);
                    return;
                }

                var spotifyTrack = searchResult.Tracks.Items!.First();
                Console.WriteLine(
                    $"Found track: {spotifyTrack.Name} by {spotifyTrack.Artists.FirstOrDefault()?.Name}");

                string trackUrl = spotifyTrack.ExternalUrls["spotify"];
                string trackNameInfo = spotifyTrack.Name;
                string artistName = spotifyTrack.Artists.FirstOrDefault()?.Name!;
                string albumName = spotifyTrack.Album.Name;
                string trackIconUrl = spotifyTrack.Album.Images.FirstOrDefault()?.Url!;
                var embed = new EmbedBuilder()
                    .WithTitle($"Track: {trackNameInfo}")
                    .WithDescription($"Artist: {artistName}\nAlbum: {albumName}\n[Listen on Spotify]({trackUrl})")
                    .WithThumbnailUrl(trackIconUrl)
                    .WithColor(Color.Green)
                    .Build();

                await RespondAsync(embed: embed);
            }
            catch (Exception ex)
            {
                responseMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine($"Error: {ex.Message}");
                await RespondAsync(responseMessage, ephemeral: true);
            }
        }
    }
}