using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// Interface for WebSite<TApplication, TSession>.ExecuteRequest(),
    /// for places that only need that or don't know TApplication and TSession.
    /// </summary>
    public interface IWebSiteExecutor
    {
        void ExecuteRequest(WebRequest request, WebResponse response, WebServer server);
    }
}
