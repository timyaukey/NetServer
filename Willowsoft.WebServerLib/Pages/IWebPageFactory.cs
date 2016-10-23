using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public interface IWebPageFactory<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        WebPage<TSiteData, TSession> GetInstance(WebContext<TSiteData, TSession> context);
    }
}
