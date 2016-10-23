using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Willowsoft.ServerLib
{
    /// <summary>
    /// A CachingFactory that returns a CachedXmlDocument object for an XML file.
    /// The cache entry size is size of the XML file in bytes.
    /// </summary>
    public class CachingXmlFactory<TData> : CachingFactory<TData, string>
        where TData : CachedXmlDocument, new()
    {
        public CachingXmlFactory(long cacheSizeLimit)
            : base(cacheSizeLimit)
        {
        }

        protected override CacheEntry MakeNew(string path)
        {
            XmlDocument doc = new XmlDocument();
            FileInfo file = new FileInfo(path);
            doc.Load(path);
            TData entry = new TData();
            entry.Init(doc);
            return new CacheEntry(entry, path, file.Length);
        }
    }

    public class CachedXmlDocument
    {
        private XmlDocument _Doc;

        public CachedXmlDocument()
        {
            _Doc = null;
        }

        public virtual void Init(XmlDocument doc)
        {
            _Doc = doc;
        }

        public XmlDocument Doc { get { return _Doc; } }
    }
}
