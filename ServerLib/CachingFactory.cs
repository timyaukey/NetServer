using System;
using System.Collections.Generic;

namespace Willowsoft.ServerLib
{
    /// <summary>
    /// A TData factory that may return the same TData instance from multiple calls
    /// to Get() when the same TKey value is passed to those calls.
    /// Caches objects returned by Get(), removing the objects least recently
    /// returned by Get() as needed to make room for new objects.
    /// All public methods of this class are threadsafe.
    /// </summary>
    abstract public class CachingFactory<TData, TKey>
    {
        /// <summary>
        /// A dictionary of objects previously returned by Get() that have not yet
        /// expired, keyed by the "key" value passed to Get(). Is a dictionary of
        /// LinkedListNode<CacheEntry> instead of CacheEntry to make it efficient
        /// to find a dictionary entry in _ExpirationOrderList.
        /// </summary>
        private Dictionary<TKey, LinkedListNode<CacheEntry>> _Cache;
        /// <summary>
        /// A linked list with all the entries in _Cache, ordered by expiration
        /// priority. The first entry to expire is always at the end.
        /// </summary>
        private LinkedList<CacheEntry> _ExpirationOrderList;
        /// <summary>
        /// The sum of all CacheEntry.Size values of all entries in _Cache.
        /// </summary>
        private long _TotalCacheSize;
        /// <summary>
        /// The lowest priority cache entries are removed as needed to keep
        /// _TotalCacheSize from exceeding this value.
        /// </summary>
        private long _CacheSizeLimit;
        private object _Lock;

        public CachingFactory(long cacheSizeLimit)
        {
            _Cache = new Dictionary<TKey, LinkedListNode<CacheEntry>>();
            _ExpirationOrderList = new LinkedList<CacheEntry>();
            _TotalCacheSize = 0;
            _CacheSizeLimit = cacheSizeLimit;
            _Lock = new object();
        }

        /// <summary>
        /// Return a TData uniquely described by a TKey, constructed by MakeNew(TKey).
        /// For example, may load and return the XmlDocument object for the XML file at some path.
        /// Will return the TData instance returned by a previous call to Get()
        /// if that instance has not expired and been removed from the cache.
        /// Subclasses may throw an exception if MakeNew() cannot create a TData for the TKey,
        /// for example the named XML file does not exist.
        /// </summary>
        /// <param name="key">A TKey that uniquely describes a TData, for example the path to a file.</param>
        /// <returns></returns>
        public TData Get(TKey key)
        {
            lock (_Lock)
            {
                LinkedListNode<CacheEntry> cachedNode;
                if (_Cache.TryGetValue(key, out cachedNode))
                {
                    _ExpirationOrderList.Remove(cachedNode);
                    _ExpirationOrderList.AddFirst(cachedNode);
                }
                else
                {
                    cachedNode = new LinkedListNode<CacheEntry>(MakeNew(key));
                    _Cache.Add(cachedNode.Value.Key, cachedNode);
                    _ExpirationOrderList.AddFirst(cachedNode);
                    _TotalCacheSize += cachedNode.Value.Size;
                    if (_TotalCacheSize > _CacheSizeLimit)
                    {
                        LinkedListNode<CacheEntry> expiredNode = _ExpirationOrderList.Last;
                        _ExpirationOrderList.Remove(expiredNode);
                        _Cache.Remove(expiredNode.Value.Key);
                        _TotalCacheSize -= expiredNode.Value.Size;
                    }
                }
                return cachedNode.Value.Data;
            }
        }

        /// <summary>
        /// Construct and return a new CacheEntry object for "key".
        /// For example, treat "key" as a file path, load an XmlDocument
        /// from that path, and return a new CacheEntry with that XmlDocument.
        /// </summary>
        /// <param name="key">The value passed to Get().</param>
        /// <returns></returns>
        abstract protected CacheEntry MakeNew(TKey key);

        protected class CacheEntry
        {
            public readonly TData Data;
            public readonly TKey Key;
            public readonly long Size;

            public CacheEntry(TData data, TKey key, long size)
            {
                Data = data;
                Key = key;
                Size = size;
            }
        }
    }
}
