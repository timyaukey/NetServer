using System;
using System.Collections.Generic;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// I don't believe this is actually used anywhere.
    /// </summary>
    /// <typeparam name="TXmlDocument"></typeparam>
    abstract public class XmlBasePage<TSiteData, TSession, TXmlDocument> : WebPage<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
        where TXmlDocument : CachedXmlDocument
    {
        private TXmlDocument _XmlDocument;

        override public void Process(WebContext<TSiteData, TSession> context)
        {
            byte[] content = GetContent();
            context.Response.SetStdHeaders(context.Request.KeepAlive, ContentTypes.XHTML, content.Length);
            IBodyWriter bodyWriter = new BytesBodyWriter(content);
            bodyWriter.WriteBody(context.Response, context.WebServerUtilities);
        }

        public virtual void SetXmlDocument(TXmlDocument cachedXmlDoc)
        {
            _XmlDocument = cachedXmlDoc;
        }

        public abstract byte[] GetContent();

        protected TXmlDocument XmlDocument { get { return _XmlDocument; } }
    }
}
