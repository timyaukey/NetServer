using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public class BytesBodyWriter : IBodyWriter
    {
        private byte[] _Content;

        public BytesBodyWriter(byte[] content)
        {
            _Content = content;
        }

        public void WriteBody(WebResponse response, IWebServerUtilities server)
        {
            response.WriteBody(server, _Content);
        }
    }
}
