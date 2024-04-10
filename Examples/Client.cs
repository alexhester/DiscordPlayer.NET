using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Text.Json;
using DiscordPlayer;

namespace ExampleBot;

class Client
{
    internal struct Information
    {
        internal string token;
        internal string clientId;
        internal ulong guildId;
        internal string youtubeApiKey;
    }

    internal static Information Read(string path)
    {
        using var json = File.OpenRead(path);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        string? token = string.Empty;
        string? clientId = string.Empty;
        ulong guildId = 0;
        string? youtubeApiKey = string.Empty;

        token = root.GetProperty("token").GetString();
        if (token == null)
            throw new Exception("Couldn't find token in config.json");

        clientId = root.GetProperty("clientId").GetString();
        if (clientId == null)
            throw new Exception("Couldn't find clientId in config.json");

        guildId = Convert.ToUInt64(root.GetProperty("guildId").GetString());
        if (guildId == 0)
            throw new Exception("Couldn't find guildId in config.json");

        youtubeApiKey = root.GetProperty("youtubeApiKey").GetString();
        if (youtubeApiKey == null)
            throw new Exception("Couldn't find youtubeApiKey in config.json");

        Information info = new()
        {
            token = token,
            clientId = clientId,
            guildId = guildId,
            youtubeApiKey = youtubeApiKey,
        };

        return info;
    }

    static async Task Main()
    {
        Information information = Read("./config.json");

        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = information.token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        });

        var voice = discord.UseVoiceNext();
        var slash = discord.UseSlashCommands();

        slash.RegisterCommands<SlashCommands>(information.guildId);

        await discord.ConnectAsync();
        await Player.Initialize(discord, await discord.GetGuildAsync(information.guildId), information.youtubeApiKey);
        await Task.Delay(-1);
    }
}

public class SlashCommands : ApplicationCommandModule
{
    [SlashCommand("play", "Play a song")]
    public static async Task Play(InteractionContext interaction, [Option("query", "song to play")] string query)
    {
        DiscordChannel? channel = interaction.Member.VoiceState?.Channel;

        if (channel == null)
        {
            await ErrorEmbed.SendErrorMessage(interaction, "Join a voice channel.");
            return;
        }

        PlayerResult result;

        try
        {
            result = await Player.Search(query);
        }
        catch (Exception ex)
        { await ErrorEmbed.SendErrorMessage(interaction, ex.Message); return; }

        if (!result.HasTracks)
        {
            await ErrorEmbed.SendErrorMessage(interaction, "Couldn't find tracks for query: " + query);
            return;
        }
        if (result.IsPlaylist)
        {
            try
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithColor(0x00950)
                    .WithTitle("Playlist: " + result.PlaylistTitle)
                    .WithUrl(result.PlaylistUrl)
                    .WithThumbnail(result.PlaylistThumbnailUrl)
                    .AddField(name: "Tracks:", value: result.Tracks.Count.ToString(), inline: true);

                await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
            }
            catch (Exception ex)
            { await ErrorEmbed.SendErrorMessage(interaction, ex.Message); return; }
        }
        else
        {
            try
            {
                var content = result.Tracks.First().Content;
                if (content is YoutubeContent youtubeContent)
                {
                    var duration = youtubeContent.Duration;
                    var viewCount = youtubeContent.ViewCount.ToString();
                    var author = result.Tracks.First().Author;

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithColor(0x00950)
                    .WithTitle("YouTube: " + result.Tracks.First().Title)
                    .WithUrl(result.Tracks.First().Url)
                    .WithThumbnail(result.Tracks.First().ThumbnailUrl)
                    .AddField(name: "Duration:", value: duration, inline: true)
                    .AddField(name: "Views:", value: viewCount, inline: true)
                    .AddField(name: "Uploaded:", value: author, inline: true);

                    await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
                }
            }
            catch (Exception ex)
            { await ErrorEmbed.SendErrorMessage(interaction, ex.Message); return; }
        }
        try
        {
            await Player.AddToQueue(channel, result.Tracks);
            return;
        }
        catch (Exception ex)
        { await ErrorEmbed.SendErrorMessage(interaction, ex.Message); return; }
    }

    [SlashCommand("playnow", "Play a song immediately")]
    public static async Task PlayNow(InteractionContext interaction, [Option("query", "song to play")] string query)
    {
        DiscordChannel? channel = interaction.Member.VoiceState?.Channel;

        if (channel == null)
        {
            await ErrorEmbed.SendErrorMessage(interaction, "Join a voice channel.");
            return;
        }

        PlayerResult result = await Player.Search(query);

        if (!result.HasTracks)
        {
            await ErrorEmbed.SendErrorMessage(interaction, "Couldn't find tracks for query: " + query);
            return;
        }

        if (result.IsPlaylist)
        {
            try
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithColor(0x00950)
                    .WithTitle("Playlist: " + result.PlaylistTitle)
                    .WithUrl(result.PlaylistUrl)
                    .WithThumbnail(result.PlaylistThumbnailUrl)
                    .AddField(name: "Tracks:", value: result.Tracks.Count.ToString(), inline: true);

                await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
            }
            catch (Exception ex)
            { await ErrorEmbed.SendErrorMessage(interaction, ex.Message); }
        }
        else
        {
            try
            {
                var content = result.Tracks.First().Content;
                if (content is YoutubeContent youtubeContent)
                {
                    var duration = youtubeContent.Duration;
                    var viewCount = youtubeContent.ViewCount.ToString();
                    var author = result.Tracks.First().Author;

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithColor(0x00950)
                    .WithTitle("YouTube: " + result.Tracks.First().Title)
                    .WithUrl(result.Tracks.First().Url)
                    .WithThumbnail(result.Tracks.First().ThumbnailUrl)
                    .AddField(name: "Duration:", value: duration, inline: true)
                    .AddField(name: "Views:", value: viewCount, inline: true)
                    .AddField(name: "Uploaded:", value: author, inline: true);

                    await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
                }
            }
            catch (Exception ex)
            { await ErrorEmbed.SendErrorMessage(interaction, ex.Message); }
        }
        try
        {
            if (Player.HasTracks)
                await Player.Insert(result.Tracks, 0);

            else
            {
                await Player.AddToQueue(channel, result.Tracks);
                return;
            }
            await Player.Skip();
        }
        catch (Exception ex)
        { await ErrorEmbed.SendErrorMessage(interaction, ex.Message); }
    }

    [SlashCommand("skip", "Skip the currently playing song")]
    public static async Task Skip(InteractionContext interaction)
    {
        if (!Player.HasTracks)
        {
            await ErrorEmbed.SendErrorMessage(interaction, "Nothing to skip");
            return;
        }

        try
        {
            await Player.Skip();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(0x00950)
                .WithTitle("Skipping track");

            await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
        }
        catch (Exception ex)
        { await ErrorEmbed.SendErrorMessage(interaction, ex.Message); }
    }

    [SlashCommand("stop", "Clear the queue and stop playing")]
    public static async Task Stop(InteractionContext interaction)
    {
        if (!Player.HasTracks)
        {
            await ErrorEmbed.SendErrorMessage(interaction, "Nothing is playing");
            return;
        }

        try
        {
            await Player.Stop();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(0x00950)
                .WithTitle("Stopping playback");

            await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
        }
        catch (Exception ex)
        { await ErrorEmbed.SendErrorMessage(interaction, ex.Message); }
    }
}

internal class ErrorEmbed
{
    internal static async Task SendErrorMessage(InteractionContext interaction, string description)
    {
        Console.WriteLine(description);

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(0xFF0000)
                .WithTitle("Something went wrong")
                .WithDescription(description);

        await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
    }
}