/* DiscordPlayer - Created by Alex Hester
 * Contains all the classes, structs, and interfaces use for PlayerResults and Tracks.
 */

using System.Xml;

namespace DiscordPlayer;

/// <summary>
/// Contains Playlist and Track information
/// </summary>
public struct PlayerResult
{
    /// <summary>
    /// Whether a this result contains a playlist or not
    /// </summary>
    public bool IsPlaylist { get; internal set; }
    /// <summary>
    /// The title of the contained playlist
    /// </summary>
    public string PlaylistTitle { get; internal set; }
    /// <summary>
    /// The url of the contained playlist
    /// </summary>
    public string PlaylistUrl { get; internal set; }
    /// <summary>
    /// The thumbnail image of the contained playlist
    /// </summary>
    public string PlaylistThumbnailUrl { get; internal set; }
    /// <summary>
    /// The Tracks contained in this result
    /// </summary>
    public List<Track> Tracks { get; internal set; }
    /// <summary>
    /// Whether this result contains Tracks or not
    /// </summary>
    public readonly bool HasTracks
    { get { return Tracks != null && Tracks.Count > 0; } }
}

/// <summary>
/// Contains Track information
/// </summary>
public struct Track
{
    /// <summary>
    /// The title of a Track
    /// </summary>
    public string Title { get; internal set; }
    /// <summary>
    /// The url of a Track
    /// </summary>
    public string Url { get; internal set; }
    /// <summary>
    /// The author of a Track
    /// </summary>
    public string Author { get; internal set; }
    /// <summary>
    /// Contains details about a Track. Contains things like duration.
    /// </summary>
    public YoutubeContent Content { get; internal set; }
    /// <summary>
    /// The thumbnail of a Track
    /// </summary>
    public string ThumbnailUrl { get; internal set; }
    /// <summary>
    /// The upload date of a Track
    /// </summary>
    public DateTimeOffset? UploadDate { get; internal set; }

    internal readonly bool HasNullInfo() => Title?.Length == 0 || Url?.Length == 0 || Author?.Length == 0 || ThumbnailUrl?.Length == 0 || UploadDate == null;
    internal string FilePath { get; set; }
}

internal static class TimeConverter
{
    /// <summary>
    /// Converts a duration from (string)ISO format to (string)HH:MM:SS
    /// </summary>
    /// <param name="iso"></param>
    /// <returns></returns>
    internal static async Task<string> ConvertISO(string iso)
    {
        return await Task.Run(() =>
        {
            TimeSpan duration = XmlConvert.ToTimeSpan(iso);

            return $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        });
    }
    /// <summary>
    /// Converts a duration from (int)Milliseconds to (string)HH:MM:SS
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    internal static async Task<string> ConvertMS(int ms)
    {
        return await Task.Run(() =>
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);

            return $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        });
    }
}