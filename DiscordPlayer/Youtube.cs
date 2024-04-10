/* DiscordPlayer - Created by Alex Hester
 * Contains a youtube-dl wrapper and the Extractors available (currently just YouTube).
 */

using System.Diagnostics;

namespace DiscordPlayer;

/// <summary>
/// Wrapper for youtube-dl
/// </summary>
internal class YouTube
{
    internal string? VideoURL { get; set; }

    internal string? Output { get; set; }

    internal bool ExtractAudio { get; set; }

    internal string YouTubeDLPath { get; set; } = new FileInfo("youtube-dl").FullName;

    internal async Task DownloadAsync()
    {
        await Task.Run(() =>
        {
            using Process process = new();
            process.StartInfo.FileName = YouTubeDLPath;

            process.StartInfo.Arguments = $"-x --audio-format wav -o \"{Output}\" {VideoURL}"; // -f bestaudio --extract-audio --audio-quality 0

            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += Process_OutputDataReceived;

            process.ErrorDataReceived += Process_ErrorDataReceived;

            process.Start();

            process.BeginOutputReadLine();

            process.BeginErrorReadLine();

            process.WaitForExit();
        });
    }

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data)) LoggingService.LogDownload(Sender.DOWNLOAD, e.Data);
    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data)) LoggingService.LogMessage(Sender.DOWNLOAD, Severity.ERROR, e.Data);
    }
}

/// <summary>
/// Contains content details and statistics of a Youtube video
/// </summary>
public struct YoutubeContent
{
    /// <summary>
    /// The value of captions, indicated whether the video has captions or not.
    /// </summary>
    public string Caption { get; internal set; }
    /// <summary>
    /// The number of comments for the video.
    /// </summary>
    public ulong CommentCount { get; internal set; }
    /// <summary>
    /// The value of definition indicated whether the video is available in high definition or only in standard definition.
    /// </summary>
    public string Definition { get; internal set; }
    /// <summary>
    /// The number of users who have indicated that they disliked the video by giving it a negative rating.
    /// </summary>
    public ulong DislikeCount { get; internal set; }
    /// <summary>
    /// The link of the video. Formatted as HH:MM:SS.
    /// </summary>
    public string Duration { get; internal set; }
    /// <summary>
    /// The number of users who currently have the video marked as a favorite video.
    /// </summary>
    public ulong FavoriteCount { get; internal set; }
    /// <summary>
    /// Indicates whether the video is licensed content.
    /// </summary>
    public bool LicensedContent { get; internal set; }
    /// <summary>
    /// The number of users who have indicated that they liked the video by giving it a positive rating.
    /// </summary>
    public ulong LikeCount { get; internal set; }
    /// <summary>
    /// Specifies the projection format of the video/
    /// </summary>
    public string Projection { get; internal set; }
    /// <summary>
    /// The number of times the video has been viewed.
    /// </summary>
    public ulong ViewCount { get; internal set; }
}