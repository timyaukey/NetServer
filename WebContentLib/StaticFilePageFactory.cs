using System;

using Willowsoft.WebServerLib;

namespace Willowsoft.WebContentLib
{
    public class StaticFilePageFactory<TSiteData, TSession> : IWebPageFactory<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        private string _ContentType;

        public StaticFilePageFactory(string contentType)
        {
            _ContentType = contentType;
        }

        public static IWebPageHandler<TSiteData, TSession> EndsWithHandler(string contentType, string endsWith)
        {
            return new WebPageEndsWithHandler<TSiteData, TSession>(new StaticFilePageFactory<TSiteData, TSession>(contentType), endsWith);
        }

        public WebPage<TSiteData, TSession> GetInstance(WebContext<TSiteData, TSession> context)
        {
            return new StaticFilePage<TSiteData, TSession>(_ContentType);
        }
    }
}
