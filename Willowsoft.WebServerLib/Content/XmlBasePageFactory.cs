using System;
using System.Collections.Generic;
using System.Xml;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    abstract public class XmlBasePageFactory<TSiteData, TSession, TXmlDocument> : IWebPageFactory<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
        where TXmlDocument : CachedXmlDocument, new()
    {
        private static CachingXmlFactory<TXmlDocument> _XmlDocuments;

        public XmlBasePageFactory(long cacheSizeLimit)
        {
            _XmlDocuments = new CachingXmlFactory<TXmlDocument>(cacheSizeLimit);
        }

        public WebPage<TSiteData, TSession> GetInstance(WebContext<TSiteData, TSession> context)
        {
            string path = context.WebSiteInfo.MapPath(context.Request.AbsoluteUriPath);
            TXmlDocument xmlData;
            try
            {
                xmlData = _XmlDocuments.Get(path);
            }
            catch (System.IO.FileNotFoundException)
            {
                throw new WebExceptions.ResourceNotFound(context.Request.RequestURI);
            }
            XmlBasePage<TSiteData, TSession, TXmlDocument> page = GetXmlPageInstance(context, xmlData);
            page.SetXmlDocument(xmlData);
            return page;
        }

        /// <summary>
        /// Return the XmlBasePage to use. For example, extract a type identifier
        /// from xmlData and create an instance of that type.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        abstract public XmlBasePage<TSiteData, TSession, TXmlDocument> GetXmlPageInstance(WebContext<TSiteData, TSession> context, TXmlDocument xmlData);
    }
}
