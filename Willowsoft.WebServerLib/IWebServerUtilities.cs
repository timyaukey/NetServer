using System;
using System.Collections.Generic;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// Interface exposing some basic web server functionality not
    /// specific to a particular web site.
    /// </summary>
    public interface IWebServerUtilities : IGenericServerUtilities
    {
        void WriteResponseHeadData(WebResponse response);
    }
}
