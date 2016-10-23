using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public class TextBodyWriter : IBodyWriter
    {
        private string _Content;

        public TextBodyWriter(string content)
        {
            _Content = content;
        }

        public void WriteBody(WebResponse response, IWebServerUtilities server)
        {
            response.WriteBody(server, _Content);
        }
    }
}
