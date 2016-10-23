using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// Interface exposing some basic functionality specific to a particular
    /// web site supplied by the underlying web server - ASP.NET or our own server.
    /// </summary>
    public interface IWebSiteUtilities
    {
        string MapPath(string url);
    }
}
