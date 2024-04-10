/* DiscordPlayer - Created by Alex Hester
 * Contains the PlayerSingleton and the public Player accessible from a bot.
 */

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Diagnostics;
using System.Xml;

/*  Things accessible to a bot
 * 
 *      Player.Initialize();
 *      Player.Search();
 *      Player.AddToQueue();
 *      Player.Skip();
 *      Player.Stop();
 *      Player.HasTracks;
 *      Player.Insert();
 *      Player.ViewQueue();
 *      Player.PrintLog;
 *      
 *      Player.Events.TrackAdded
 *      Player.Events.StartedPlaying
 *      Player.Events.FinishedPlaying
 *      Player.Events.Log
 */

namespace DiscordPlayer;

/// <summary>
/// DiscordPlayer
/// </summary>
public static class Player
{
    private static PlayerSingleton PlayerInstance
    { get { return PlayerSingleton.Instance; } }

    private static QueueSingleton Queue
    { get { return QueueSingleton.Instance; } }

    /// <summary>
    /// Contains subscribable events for the Player
    /// </summary>
    public static EventsSingleton Events
    { get { return EventsSingleton.Instance; } }

    /// <summary>
    /// Initializes the player
    ///     <list type="table">
    ///         <item>
    ///             <description>[REQUIRED]</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="DiscordClient"/> <paramref name="discord"/></term>
    ///             <description>Your bot's client</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="DiscordGuild"/> <paramref name="guild"/></term>
    ///             <description>Your bot's guild</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="string"/> <paramref name="youtubeApiKey"/></term>
    ///             <description>Youtube Data API Key from console.cloud.google.com</description>
    ///         </item>
    ///         <item>
    ///             <description>[OPTIONAL]</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="string"/> downloadsPath</term>
    ///             <description>Where to store downloaded Youtube videos, defaults to ./downloads/</description>
    ///         </item>
    ///     </list>
    /// </summary>
    /// <param name="discord"></param>
    /// <param name="guild"></param>
    /// <param name="youtubeApiKey"></param>
    public static async Task Initialize(DiscordClient discord, DiscordGuild guild, string youtubeApiKey)
    {
        string downloadsPath = "./downloads/";
        await Initialize(discord, guild, youtubeApiKey, downloadsPath);
    }
    /// <summary>
    /// Initializes the player
    ///     <list type="table">
    ///         <item>
    ///             <description>[REQUIRED]</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="DiscordClient"/> <paramref name="discord"/></term>
    ///             <description>Your bot's client</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="DiscordGuild"/> <paramref name="guild"/></term>
    ///             <description>Your bot's guild</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="string"/> <paramref name="youtubeApiKey"/></term>
    ///             <description>Youtube Data API Key from console.cloud.google.com</description>
    ///         </item>
    ///         <item>
    ///             <description>[OPTIONAL]</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="string"/> <paramref name="downloadsPath"/></term>
    ///             <description>Where to store downloaded Youtube videos, defaults to ./downloads/</description>
    ///         </item>
    ///     </list>
    /// </summary>
    /// <param name="discord"></param>
    /// <param name="guild"></param>
    /// <param name="downloadsPath"></param>
    /// <param name="youtubeApiKey"></param>
    public static async Task Initialize(DiscordClient discord, DiscordGuild guild, string youtubeApiKey, string downloadsPath)
    {
        await Task.Run(() =>
        {
            Events.Log += Log;
            PlayerInstance.Client = discord;
            PlayerInstance.Guild = guild;
            PlayerInstance.Downloads = downloadsPath;
            PlayerInstance.ApiKey = youtubeApiKey;
            Events.TrackAdded += OnTrackAdded;
            Events.StartedPlaying += StartedPlayingTrack;
            Events.FinishedPlaying += FinishedPlayingTrack;
            LoggingService.LogMessage(Sender.DISCORDPLAYER, Severity.INFO, "Player initialized");
            LoggingService.LogMessage(Sender.DISCORDPLAYER, Severity.INFO, "With DiscordClient: " + discord);
            LoggingService.LogMessage(Sender.DISCORDPLAYER, Severity.INFO, "With DiscordGuild: " + guild);
            LoggingService.LogMessage(Sender.DISCORDPLAYER, Severity.INFO, "With API_KEY: " + youtubeApiKey);
            LoggingService.LogMessage(Sender.DISCORDPLAYER, Severity.INFO, "With Downloads Path: " + downloadsPath);
        });
    }

    private static void Log(object? sender, LogMessage e)
    {
        if (!PlayerInstance.Log) return;
        // Get Sender Color
        Console.ForegroundColor = e.Sender switch
        {
            Sender.DISCORDPLAYER => ConsoleColor.Yellow,
            Sender.SEARCH => ConsoleColor.DarkRed,
            Sender.DOWNLOAD => ConsoleColor.DarkRed,
            _ => ConsoleColor.White,
        };
        Console.Write($"\n[{e.Sender}]");

        // Get Severity Color
        Console.ForegroundColor = e.Severity switch
        {
            Severity.WARNING => ConsoleColor.Green,
            Severity.ERROR => ConsoleColor.Magenta,
            _ => ConsoleColor.White,
        };
        Console.Write($" {e.Message}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    /// <summary>
    /// Searches for a query, or link to a playlist/video
    /// </summary>
    /// <param name="query"></param>
    /// <returns><see cref="PlayerResult"/></returns>
    public static async Task<PlayerResult> Search(string query)
    {
        LoggingService.LogMessage(Sender.SEARCH, Severity.INFO, "Searching for: " + query);

        return await PlayerInstance.SearchYoutubeAsync(query);
    }

    /// <summary>
    /// Adds a List of Track(s) to the queue and begins playing
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="tracks"></param>
    /// <returns></returns>
    public static async Task AddToQueue(DiscordChannel channel, List<Track> tracks)
    {
        await AddToQueue(channel, tracks, PlayerInstance.Options);
    }

    /// <summary>
    /// Adds a List of Track(s) to the queue and begins playing, and sets the Player NodeOptions
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="tracks"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task AddToQueue(DiscordChannel channel, List<Track> tracks, NodeOptions options)
    {
        PlayerInstance.Channel = channel;
        PlayerInstance.Options = options;

        foreach (Track track in tracks)
        {
            Queue.node.Add(new()
            {
                Title = track.Title,
                Url = track.Url,
                Author = track.Author,
                Content = track.Content,
                ThumbnailUrl = track.ThumbnailUrl,
                UploadDate = track.UploadDate,
                FilePath = await PlayerInstance.DownloadAsync(track.Url)
            });
            LoggingService.LogMessage(Sender.DOWNLOAD, Severity.INFO, $"Downloaded: {track.Title}");
        }
    }

    /// <summary>
    /// Skips to a specified index of the queue
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static async Task Skip(int index)
    {
        for (int i = 0; i < index; i++)
        {
            string filePath = Queue.node.Dequeue().FilePath;
            if (File.Exists(filePath) && PlayerInstance.Options.deleteAfterPlay)
                File.Delete(filePath);
        }
        await PlayerInstance.KillFFmpeg();
    }
    /// <summary>
    /// Skips the currently playing track
    /// </summary>
    public static async Task Skip() => await PlayerInstance.KillFFmpeg();

    /// <summary>
    /// Clears the queue and ends playback
    /// </summary>
    /// <returns></returns>
    public static async Task Stop()
    {
        Queue.node.Clear();
        await PlayerInstance.KillFFmpeg();

        if (PlayerInstance.Options.leaveOnStop)
            PlayerInstance.LeaveChannel();
    }

    /// <summary>
    /// Returns true if the queue contains any tracks or if the player is currently playing a track
    /// </summary>
    /// <returns>Returns true if the queue contains any tracks or if the player is currently playing a track; otherwise, false</returns>
    public static bool HasTracks { get { return Queue.node.Count > 0 || PlayerInstance.IsPlaying; } }

    /// <summary>
    /// Inserts track at a given <paramref name="index"/> in the queue
    ///     <list type="table">
    ///         <item>
    ///             <term><see cref="List{T}"/> <paramref name="tracks"/></term>
    ///             <description>The track(s) to insert</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="int"/> <paramref name="index"/></term>
    ///             <description>The index to insert at</description>
    ///         </item>
    ///     </list>
    /// </summary>
    /// <param name="tracks"></param>
    /// <param name="index"></param>
    public static async Task Insert(List<Track> tracks, int index)
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            Queue.node.Insert(index + i, new()
            {
                Title = tracks[i].Title,
                Url = tracks[i].Url,
                Author = tracks[i].Author,
                Content = tracks[i].Content,
                ThumbnailUrl = tracks[i].ThumbnailUrl,
                UploadDate = tracks[i].UploadDate,
                FilePath = await PlayerInstance.DownloadAsync(tracks[i].Url)
            });
            LoggingService.LogMessage(Sender.DOWNLOAD, Severity.INFO, $"Downloaded: {tracks[i].Title}");
        }
    }

    /// <summary>
    /// Returns a list of all tracks in the queue
    /// </summary>
    /// <returns>Returns List{Tracks}</returns>
    public static async Task<List<Track>> ViewQueue()
    {
        return await Task.Run(() =>
        {
            List<Track> tracks = [];
            if (Queue.currentTrack.Title != "null" && Queue.currentTrack.Url != "null" && Queue.currentTrack.Author != "null" && Queue.currentTrack.ThumbnailUrl != "null" && Queue.currentTrack.FilePath != "null")
            {
                tracks.Add(new()
                {
                    Title = Queue.currentTrack.Title,
                    Url = Queue.currentTrack.Url,
                    Author = Queue.currentTrack.Author,
                    Content = Queue.currentTrack.Content,
                    ThumbnailUrl = Queue.currentTrack.ThumbnailUrl,
                    UploadDate = Queue.currentTrack.UploadDate,
                    FilePath = Queue.currentTrack.FilePath,
                });
            }
            foreach (Track track in Queue.node)
            {
                tracks.Add(new()
                {
                    Title = track.Title,
                    Url = track.Url,
                    Author = track.Author,
                    Content = track.Content,
                    ThumbnailUrl = track.ThumbnailUrl,
                    UploadDate = track.UploadDate,
                    FilePath = track.FilePath,
                });
            }
            return tracks;
        });
    } // TODO Gotta test

    /// <summary>
    /// Controls the Player's internal logging. Enabled by default.
    /// </summary>
    /// <returns>true if enabled; false if disabled</returns>
    public static bool PrintLog
    { get { return PlayerInstance.Log; } set { PlayerInstance.Log = PrintLog; } }

    private static async void OnTrackAdded(object? sender, EventArgs e)
    {
        if (!PlayerInstance.IsPlaying)
            await PlayerInstance.SendAsync(Queue.node.Dequeue());
    }
    private static void StartedPlayingTrack(object? sender, EventArgs e)
    {

    }
    private static async void FinishedPlayingTrack(object? sender, EventArgs e)
    {
        if (!PlayerInstance.IsPlaying)
        {
            if (Queue.node.Count > 0)
            {
                await PlayerInstance.SendAsync(Queue.node.Dequeue());
            }
            else if (PlayerInstance.Options.leaveOnEnd)
                PlayerInstance.LeaveChannel();
        }
    }
}

sealed class PlayerSingleton
{
    internal DiscordClient? Client { get; set; }
    internal DiscordGuild? Guild { get; set; }
    internal DiscordChannel? Channel { get; set; }
    internal string? Downloads { get; set; }
    internal string? ApiKey { get; set; }
    internal bool Log { get; set; } = true;
    internal NodeOptions Options { get; set; } = new()
    {
        leaveOnEmpty = true, // TODO not implemented
        leaveOnEnd = false,
        leaveOnStop = true,
        deleteAfterPlay = true
    };
    internal bool IsPlaying { get; private set; }
    internal bool InVoiceChannel { get; private set; }

    private PlayerSingleton() { }
    private static PlayerSingleton? _instance = null;
    private static readonly object _instanceLock = new();
    internal static PlayerSingleton Instance
    {
        get
        {
            if (_instance == null)
                lock (_instanceLock)
                    _instance ??= new PlayerSingleton();
            return _instance;
        }
    }

    private readonly SemaphoreSlim semaphore = new(1, 1);

    private Process? FFmpeg { get; set; }

    /// <summary>
    /// Leaves channel
    /// </summary>
    internal void LeaveChannel()
    {
        try
        {
            Client.GetVoiceNext().GetConnection(Guild).Disconnect();
            InVoiceChannel = false;
            LoggingService.LogMessage(Sender.DISCORDPLAYER, Severity.INFO, "Leaving voice channel");
            Channel = null;
        }
        catch (Exception ex) { LoggingService.LogMessage(Sender.DISCORDPLAYER, Severity.ERROR, ex.ToString()); }
    }

    /// <summary>
    /// Searches for a query on YouTube and returns a PlayerResult.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Returns a PlayerResult</returns>
    internal async Task<PlayerResult> SearchYoutubeAsync(string query)
    {
        // Start YoutubeService
        var youtube = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = ApiKey,
            ApplicationName = "DiscordPlayer",
        });

        bool isPlaylist = query.Contains("https://www.youtube.com/playlist?list=");

        if (isPlaylist)
        {
            // Search for playlist
            var playlist = await Task.Run(async () =>
            {
                var playlistRequest = youtube.Playlists.List("snippet");
                playlistRequest.Id = query.Replace("https://www.youtube.com/playlist?list=", "");
                playlistRequest.MaxResults = 1;
                var playlistResponse = await playlistRequest.ExecuteAsync();

                LoggingService.LogMessage(Sender.SEARCH, Severity.INFO, "Found Playlist: " + playlistResponse.Items.First().Snippet.Title);

                return playlistResponse;
            });

            // If no results found, return
            if (playlist.Items.Count == 0)
            {
                LoggingService.LogMessage(Sender.SEARCH, Severity.WARNING, "No results found for query: " + query);
                return new PlayerResult(); // This is fine because the bot can check playerResult.HasTracks()
            }

            // Search for videos in the playlist
            var playlistItem = await Task.Run(async () =>
            {
                var playlistItemsRequest = youtube.PlaylistItems.List("snippet");
                playlistItemsRequest.PlaylistId = query.Replace("https://www.youtube.com/playlist?list=", "");
                playlistItemsRequest.MaxResults = 50;
                return await playlistItemsRequest.ExecuteAsync();
            });

            // If no results found, return
            if (playlistItem.Items.Count == 0)
            {
                LoggingService.LogMessage(Sender.SEARCH, Severity.WARNING, "No results found for query: " + query);
                return new PlayerResult(); // This is fine because the bot can check playerResult.HasTracks()
            }

            // Create a temporary list so we can make sure we dont return any null tracks
            List<Track> tracks = [];

            // Loop through each video in the playlist
            foreach (var result in playlistItem.Items)
            {
                // Put all the information into a Track
                tracks.Add(await Task.Run(async () =>
                {
                    // Get details about a video
                    var video = youtube.Videos.List("snippet,contentDetails,statistics");
                    video.Id = result.Snippet.ResourceId.VideoId;
                    video.MaxResults = 1;
                    var videoResponse = await video.ExecuteAsync();
                    if (videoResponse.Items.Count == 0)
                    {
                        LoggingService.LogMessage(Sender.SEARCH, Severity.INFO, "Couldn't find video");
                        return new Track();
                    }
                    var snippet = videoResponse.Items[0].Snippet;
                    var contentDetails = videoResponse.Items[0].ContentDetails;
                    var statistics = videoResponse.Items[0].Statistics;

                    LoggingService.LogMessage(Sender.SEARCH, Severity.INFO, "Found Video: " + snippet.Title);

                    if (contentDetails != null && contentDetails.Duration != null)
                        contentDetails.Duration = await ConvertDuration(contentDetails.Duration);

                    return new Track()
                    {
                        Title = snippet.Title,
                        Author = snippet.ChannelTitle,
                        Url = string.Format("https://www.youtube.com/watch?v={0}", videoResponse.Items.First().Id),
                        ThumbnailUrl = snippet.Thumbnails.Default__.Url,
                        Content = new YoutubeContent()
                        {
                            Caption = contentDetails?.Caption ?? "null",
                            CommentCount = statistics.CommentCount ?? 0,
                            Definition = contentDetails?.Definition ?? "null",
                            DislikeCount = statistics.DislikeCount ?? 0,
                            Duration = contentDetails?.Duration ?? "00:00:00",
                            FavoriteCount = statistics.FavoriteCount ?? 0,
                            LicensedContent = contentDetails?.LicensedContent ?? false,
                            LikeCount = statistics.LikeCount ?? 0,
                            Projection = contentDetails?.Projection ?? "null",
                            ViewCount = statistics.ViewCount ?? 0
                        },
                        UploadDate = snippet.PublishedAtDateTimeOffset,
                    };
                }));
            }

            // Create PlayerResult to add non-null Tracks to
            PlayerResult playerResult = new()
            {
                IsPlaylist = isPlaylist,
                PlaylistTitle = playlist.Items.First().Snippet.Title,
                PlaylistUrl = query,
                PlaylistThumbnailUrl = playlist.Items.First().Snippet.Thumbnails.Default__.Url,
                Tracks = []
            };

            // Add non-null Tracks to PlayerResult
            foreach (Track track in tracks)
                if (!track.HasNullInfo())
                    playerResult.Tracks.Add(track);

            return playerResult;
        }
        else
        {
            // Search for query
            var search = await Task.Run(async () =>
            {
                var searchRequest = youtube.Search.List("snippet");
                searchRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
                searchRequest.Q = query;
                searchRequest.MaxResults = 1;
                return await searchRequest.ExecuteAsync();
            });

            // If no results found, return
            if (search.Items.Count == 0)
            {
                LoggingService.LogMessage(Sender.SEARCH, Severity.WARNING, "No results found for query: " + query);
                return new PlayerResult(); // This is fine because the bot can check playerResult.HasTracks()
            }

            // Put all the information into a PlayerResult
            PlayerResult playerResult = new()
            {
                IsPlaylist = false,
                PlaylistTitle = string.Empty,
                PlaylistUrl = string.Empty,
                PlaylistThumbnailUrl = string.Empty,
                Tracks = []
            };

            if (search.Items[0].Id.Kind == "youtube#video")
            {
                // Get details about a video
                var videosRequest = youtube.Videos.List("snippet,contentDetails,statistics");
                videosRequest.Id = search.Items[0].Id.VideoId;
                videosRequest.MaxResults = 1;
                var videoResponse = await videosRequest.ExecuteAsync();
                if (videoResponse.Items.Count == 0)
                {
                    LoggingService.LogMessage(Sender.SEARCH, Severity.INFO, "Couldn't find video");
                    return new PlayerResult(); // This is fine because the bot can check playerResult.HasTracks()
                }
                var snippet = videoResponse.Items.First().Snippet;
                var contentDetails = videoResponse.Items.First().ContentDetails;
                var statistics = videoResponse.Items.First().Statistics;

                LoggingService.LogMessage(Sender.SEARCH, Severity.INFO, "Found Video: " + snippet.Title);

                if (contentDetails != null && contentDetails.Duration != null)
                    contentDetails.Duration = await ConvertDuration(contentDetails.Duration);

                playerResult.Tracks.Add(new()
                {
                    Title = snippet.Title,
                    Author = snippet.ChannelTitle,
                    Url = "https://www.youtube.com/watch?v=" + videoResponse.Items.First().Id,
                    ThumbnailUrl = snippet.Thumbnails.Default__.Url,
                    Content = new YoutubeContent()
                    {
                        Caption = contentDetails?.Caption ?? "null",
                        CommentCount = statistics.CommentCount ?? 0,
                        Definition = contentDetails?.Definition ?? "null",
                        DislikeCount = statistics.DislikeCount ?? 0,
                        Duration = contentDetails?.Duration ?? "00:00:00",
                        FavoriteCount = statistics.FavoriteCount ?? 0,
                        LicensedContent = contentDetails?.LicensedContent ?? false,
                        LikeCount = statistics.LikeCount ?? 0,
                        Projection = contentDetails?.Projection ?? "null",
                        ViewCount = statistics.ViewCount ?? 0
                    },
                    UploadDate = snippet.PublishedAtDateTimeOffset,
                });
            }
            return playerResult;
        }
    }

    private static async Task<string> ConvertDuration(string isoDuration)
    {
        return await Task.Run(() =>
        {
            TimeSpan duration = XmlConvert.ToTimeSpan(isoDuration);

            return $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        });
    }

    /// <summary>
    /// Downloads a YouTube video from a URL and returns the path to the downloaded video.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    internal async Task<string> DownloadAsync(string url)
    {
        return await Task.Run(async () =>
        {
            string filePath;
            if (Downloads != null && Downloads.EndsWith('/'))
                filePath = $"{Downloads}{url.Replace("https://www.youtube.com/watch?v=", "")}.wav";
            else
                filePath = $"{Downloads}/{url.Replace("https://www.youtube.com/watch?v=", "")}.wav";

            // If file already exists, skip the download
            if (File.Exists(filePath))
            {
                LoggingService.LogMessage(Sender.DOWNLOAD, Severity.INFO, $"{filePath} already exists.");
                return filePath;
            }
            YouTube youtube = new()
            {
                Output = filePath,
                ExtractAudio = true,
                VideoURL = url
            };

            await youtube.DownloadAsync();

            return filePath;
        });
    }

    internal async Task SendAsync(Track track)
    {
        IsPlaying = true;
        EventsSingleton.Instance.OnStartedPlaying();
        await semaphore.WaitAsync();

        QueueSingleton.Instance.currentTrack = track;

        if (!File.Exists(track.FilePath))
        {
            Console.WriteLine($"{track.FilePath} does not exist");
            return;
        }

        VoiceNextConnection connection;

        if (InVoiceChannel)
        {
            connection = Client.GetVoiceNext().GetConnection(Guild);
        }
        else
        {
            if (Channel != null)
                connection = await Channel.ConnectAsync();
            else
                throw new InvalidOperationException("Channel is null");
            InVoiceChannel = true;
        }

        if (FFmpeg != null) await KillFFmpeg();

        FFmpeg = Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{track.FilePath}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });

        if (FFmpeg == null)
        {
            Console.WriteLine("Failed to start ffmpeg.exe");
            return;
        }
        Stream pcm = FFmpeg.StandardOutput.BaseStream;

        VoiceTransmitSink transmit = connection.GetTransmitSink();

        // Start streaming file
        try
        {
            await pcm.CopyToAsync(transmit);
        }
        // If cancelled/failed
        catch (Exception ex)
        {
            LoggingService.LogMessage(Sender.DISCORDPLAYER, Severity.INFO, "Playback was cancelled: " + ex);
        }
        // After streaming
        finally
        {
            await pcm.FlushAsync();
            if (Options.deleteAfterPlay) // Optionally delete file
            {
                await KillFFmpeg(); // Make sure ffmpeg is not using the file before deletion

                File.Delete(track.FilePath);
            }
            semaphore.Release();
            IsPlaying = false;
            EventsSingleton.Instance.OnFinishedPlaying();
        }
    }

    internal async Task KillFFmpeg()
    {
        await Task.Run(() =>
        {
            FFmpeg?.WaitForExit();
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (FFmpeg != null && !FFmpeg.HasExited && stopwatch.ElapsedMilliseconds < 5000) // Kill FFmpeg if it hasn't exited after 5 seconds
            {
                Thread.Sleep(100);
            }
            FFmpeg?.Kill(true);
            FFmpeg = null;
        });
    }
}

/// <summary>
/// Options for the Player. Default values: leaveOnEnd = false, leaveOnEmpty = true, leaveOnStop = true, deleteAfterPlay = true
/// </summary>
public struct NodeOptions
{
    /// <summary>
    /// Leave when the queue is empty
    /// </summary>
    public bool leaveOnEnd;
    /// <summary>
    /// Leave when the channel is empty
    /// </summary>
    public bool leaveOnEmpty;
    /// <summary>
    /// Leave on the Stop command
    /// </summary>
    public bool leaveOnStop;
    /// <summary>
    /// Delete the file after playing
    /// </summary>
    public bool deleteAfterPlay;
}