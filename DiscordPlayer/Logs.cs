/* DiscordPlayer - Created by Alex Hester
 * Contains all the data for Logging.
 */

namespace DiscordPlayer;

/// <summary>
/// Struct that contains Severity and Message
/// </summary>
public struct LogMessage
{
    /// <summary>
    /// Sender of the message
    /// </summary>
    public Sender Sender { get; set; }
    /// <summary>
    /// Severity of the message
    /// </summary>
    public Severity Severity { get; set; }
    /// <summary>
    /// LogMessage Message
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// What time the message is created
    /// </summary>
    public DateTime TimeStamp { get; set; }
}

/// <summary>
/// Severity of the message
/// </summary>
public enum Severity
{
    /// <summary>
    /// Information
    /// </summary>
    INFO,
    /// <summary>
    /// A warning
    /// </summary>
    WARNING,
    /// <summary>
    /// An error
    /// </summary>
    ERROR
}

/// <summary>
/// Sender of the message
/// </summary>
public enum Sender
{
    /// <summary>
    /// Downloads
    /// </summary>
    DOWNLOAD,
    /// <summary>
    /// Searches
    /// </summary>
    SEARCH,
    /// <summary>
    /// DiscordPlayer.NET
    /// </summary>
    DISCORDPLAYER
}

/// <summary>
/// LoggingService class
/// </summary>
internal class LoggingService
{
    /// <summary>
    /// Logs a message to the console
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="severity"></param>
    /// <param name="message"></param>
    internal static void LogMessage(Sender sender, Severity severity, string message)
    {
        if (message.Contains("unable to obtain file audio codec with ffprobe")) return;
        LogMessage logMessage = new()
        {
            Sender = sender,
            Severity = severity,
            Message = message,
            TimeStamp = DateTime.Now
        };
        EventsSingleton.Instance.OnLog(logMessage);
    }

    internal static void LogDownload(Sender sender, string message)
    {
        if (!PlayerSingleton.Instance.Log) return;

        Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");

        // Set Sender Color
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("\r{0}", $"[{sender}]");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($" {message}");
    }
}