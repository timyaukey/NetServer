using System;
using System.Collections.Generic;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public static class PageTypes<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        public static IEnumerable<IWebPageHandler<TSiteData, TSession>> StandardPageHandlers()
        {
            List<IWebPageHandler<TSiteData, TSession>> handlers = new List<IWebPageHandler<TSiteData, TSession>>();
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.HTML, ".html"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.HTML, ".htm"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.XHTML, ".xhtml"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.JPEG, ".jpg"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.GIF, ".gif"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.PNG, ".png"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.TIFF, ".tif"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.TIFF, ".tiff"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.ICON, ".ico"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.CSS, ".css"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.JAVASCRIPT, ".js"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.XML, ".xml"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.PDF, ".pdf"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.FLV, ".flv"));
            handlers.Add(StaticFilePageFactory<TSiteData, TSession>.EndsWithHandler(ContentTypes.MP4, ".mp4"));
            handlers.Add(new WebPageEndsWithHandler<TSiteData, TSession>(new DebugPageFactory<TSiteData, TSession>(), ".debug"));
            return handlers;
        }
    }
}
