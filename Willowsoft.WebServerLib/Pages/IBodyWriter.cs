using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public interface IBodyWriter
    {
        void WriteBody(WebResponse response, IWebServerUtilities server);
    }
}
