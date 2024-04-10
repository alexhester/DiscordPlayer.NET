/* DiscordPlayer - Created by Alex Hester
 * Contains Singleton classes for Events and the Queue.
 */

using System.Collections;

namespace DiscordPlayer;

/// <summary>
/// Contains subscribable events for the Player
/// </summary>
public sealed class EventsSingleton
{
    private EventsSingleton() { }
    private static EventsSingleton? _instance = null;
    private static readonly object _instanceLock = new();

    internal static EventsSingleton Instance
    {
        get
        {
            if (_instance == null)
                lock (_instanceLock)
                    _instance ??= new EventsSingleton();
            return _instance;
        }
    }

    /// <summary>
    /// Fired when a Track is added to the queue
    /// </summary>
    public event EventHandler? TrackAdded;
    /// <summary>
    /// Fired when a Track is finished playing
    /// </summary>
    public event EventHandler? FinishedPlaying;
    /// <summary>
    /// Fired when a Track begins playing
    /// </summary>
    public event EventHandler? StartedPlaying;
    internal event EventHandler<LogMessage>? Log;
    internal void OnTrackAdded() => TrackAdded?.Invoke(null, EventArgs.Empty);
    internal void OnFinishedPlaying() => FinishedPlaying?.Invoke(null, EventArgs.Empty);
    internal void OnStartedPlaying() => StartedPlaying?.Invoke(null, EventArgs.Empty);
    internal void OnLog(LogMessage message) => Log?.Invoke(null, message);
}

/// <summary>
/// A singleton that contains a QueueableList of string filePath, where filePath is the location of a downloaded track
/// </summary>
public sealed class QueueSingleton
{
    internal delegate Task QueueHandler();

    internal QueueSingleton() { }
    private static QueueSingleton? _instance = null;
    private static readonly object _instanceLock = new();

    internal static QueueSingleton Instance
    {
        get
        {
            if (_instance == null)
                lock (_instanceLock)
                    _instance ??= new QueueSingleton();
            return _instance;
        }
    }
    // node contains the filePath to each downloaded video
    internal QueueableList<Track> node = [];

    internal Track currentTrack = new()
    {
        Title = "null",
        Url = "null",
        Author = "null",
        ThumbnailUrl = "null",
        FilePath = "null"
    };
}

/// <summary>
/// A List with added functions: Dequeue(), and Peek()
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueueableList<T> : IList<T>
{
    private readonly List<T> _list = [];

    #region QUEUE

    /// <summary>
    /// Removes and returns the first element of the QueueableList
    /// </summary>
    /// <returns>The first element of the QueueableList</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Dequeue()
    {
        if (_list.Count == 0)
            throw new InvalidOperationException("The collection is empty");

        T item = _list[0];
        _list.RemoveAt(0);
        return item;
    }

    /// <summary>
    /// Returns the first element of the QueueableList, without removing it
    /// </summary>
    /// <returns>The first element of the QueueableList</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Peek()
    {
        if (_list.Count == 0)
            throw new InvalidOperationException("The collection is empty");
        return _list[0];
    }

    #endregion

    #region LIST

    /// <summary>
    /// Gets the number of elements contained in the QueueableList
    /// </summary>
    /// <returns>The number of elements contained in the QueueableList</returns>
    public int Count => _list.Count;

    /// <summary>
    /// Gets a value indicating whether the QueueableList is read-only
    /// </summary>
    /// <returns>true if the QueueableList is read-only; otherwise, false</returns>
    public bool IsReadOnly => ((ICollection<T>)_list).IsReadOnly;

    /// <summary>
    /// Adds an item to the QueueableList
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        _list.Add(item);
        EventsSingleton.Instance.OnTrackAdded();
    }

    /// <summary>
    /// Copies the elements of a QueueableList to an Array, starting at a particular Array index 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Inserts an item to the QueueableList at the specified index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        EventsSingleton.Instance.OnTrackAdded();
    }

    /// <summary>
    /// Determines the index of a specified item in the QueueableList
    /// </summary>
    /// <param name="item"></param>
    /// <returns>The index of item if found in the QueueableList; otherwise, -1</returns>
    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    /// <summary>
    /// Determines whether the QueueableList contains a specific value
    /// </summary>
    /// <param name="item"></param>
    /// <returns>true if an item is found in the QueueableList; otherwise, false</returns>
    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    /// <summary>
    /// Removes the first occurrence of a specfic object from the QueueableList
    /// </summary>
    /// <param name="item"></param>
    /// <returns>true if item was successfully removed from the QueueableList; otherwise, false. This method also returns false if item is not found in the original QueueableList</returns>
    public bool Remove(T item)
    {
        bool result = _list.Remove(item);
        return result;
    }

    /// <summary>
    /// Removes the item at the specified index
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    /// <summary>
    /// Removes all items from the QueueableList
    /// </summary>
    public void Clear()
    {
        _list.Clear();
    }

    /// <summary>
    /// Gets or sets the element at the specified index
    /// </summary>
    /// <param name="index"></param>
    /// <returns>The element at the specified index</returns>
    public T this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    /// <summary>
    /// Returns an enumerator the iterates through the QueueableList
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the QueueableList</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_list).GetEnumerator();
    }

    #endregion
}
