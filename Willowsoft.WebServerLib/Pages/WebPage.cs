using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// Base class of all page handlers.
    /// </summary>
    public abstract class WebPage<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        /// <summary>
        /// Produce the output for the web page.
        /// Must set HTTP response headers and HTTP status, generally by calling 
        /// context.Response.SetStdHeaders(). Then write the HTTP response body,
        /// generally by creating and using an IBodyWriter though you may directly
        /// call context.Response.WriteBody() if you like. All response headers
        /// MUST be set before the first data is written to the response body,
        /// because the response headers will be written to the response stream
        /// the first time data is written to the response body.
        /// </summary>
        /// <param name="context"></param>
        public abstract void Process(WebContext<TSiteData, TSession> context);
    }
}
