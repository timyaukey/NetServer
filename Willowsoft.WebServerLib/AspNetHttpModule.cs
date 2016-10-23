using System;
using System.Web;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// An ASP.NET IHttpModule that creates and stores the WebSiteData<> shared by one or
    /// more AspNetHttpHandler<> subclasses registered as IHttpHandler objects on an
    /// ASP.NET web site.
    /// </summary>
    /// <typeparam name="TSiteData"></typeparam>
    /// <typeparam name="TSession"></typeparam>
    public class AspNetHttpModule<TSiteData, TSession>
        : IHttpModule
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        public const string SiteDataKey = "WebSiteData";

        public void Init(HttpApplication context)
        {
            TSiteData siteData = new TSiteData();
            context.Application[SiteDataKey] = siteData;
        }

        public void Dispose()
        {
        }
    }
}
