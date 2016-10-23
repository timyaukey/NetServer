using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public class WebResponse
    {
        public static readonly string XHTML10_Doctype = 
            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" " +
            "\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n";
        public static readonly string XHTML10_Namespace = "xmlns=\"http://www.w3.org/1999/xhtml\"";
        public static readonly string XHTML10_Html = "<html " + XHTML10_Namespace + ">\r\n";

        private Connection _Con;
        private NameValueCollection _Headers;
        private HttpCookieCollection _Cookies;
        private HttpStatus _Status;
        private Encoder _Encoder;
        private byte[] _OutputBuffer;
        private int _OutputBufferUsed;
        private bool _HeadersWritten;
        private bool _HeadersOnly;

        public WebResponse(Connection con, NameValueCollection headers, HttpCookieCollection cookies,
            bool headersOnly)
        {
            _Con = con;
            _Status = HttpStatus.Ok;
            _Encoder = Encoding.UTF8.GetEncoder();
            _Headers = headers;
            _Cookies = cookies;
            _OutputBuffer = new byte[1024];
            _OutputBufferUsed = 0;
            _HeadersWritten = false;
            _HeadersOnly = headersOnly;
        }

        public WebResponse(Connection con, bool headersOnly)
            : this(con, new NameValueCollection(), new HttpCookieCollection(), headersOnly)
        {
        }

        public Connection Con { get { return _Con; } }
        public NameValueCollection Headers { get { return _Headers; } }
        public HttpCookieCollection Cookies { get { return _Cookies; } }
        public HttpStatus Status { get { return _Status; } set { _Status = value; } }

        public void SetStdHeaders(bool keepAlive, string contentType, int length)
        {
            SetDateHeader();
            if (keepAlive)
                _Headers.Set(HttpHeaders.Connection, HttpHeaders.Connection_KeepAlive);
            else
                _Headers.Set(HttpHeaders.Connection, HttpHeaders.Connection_Close);
            _Headers.Set(HttpHeaders.ContentType, contentType);
            //_Headers.Set(HttpHeaders.ContentLength, length.ToString());
            _Headers.Set(HttpHeaders.TransferEncoding, HttpHeaders.TransferEncoding_Chunked);
        }

        public void SetDateHeader()
        {
            string rfc1123DateTime = DateTime.Now.ToUniversalTime().ToString("R");
            _Headers.Set(HttpHeaders.Date, rfc1123DateTime);
        }

        public IBodyWriter CreateNotFoundBodyWriter(string requestURI, bool keepAlive)
        {
            string errorContent =
                XHTML10_Doctype +
                XHTML10_Html +
                    StandardErrorHead("Resource Not Found") +
                    "<body><h3>Resource not found: " + HtmlEncode(requestURI) +"</h3></body>\r\n" +
                "</html>";
            SetStdHeaders(keepAlive, ContentTypes.XHTML, errorContent.Length);
            this.Status = HttpStatus.NotFound;
            return new TextBodyWriter(errorContent);
        }

        public static string StandardErrorHead(string title)
        {
            return
                "<head>\r\n" +
                "  <title>" + WebResponse.HtmlEncode(title) + "</title>\r\n" +
                "  <style type=\"" + ContentTypes.CSS + "\">\r\n" +
                "    body, p { font-family: sans-serif; }\r\n" +
                "  </style>\r\n" +
                "</head>\r\n";
        }

        public void Flush()
        {
            _Con.Flush();
        }

        public static string HtmlEncode(string input)
        {
            return HttpUtility.HtmlEncode(input);
        }

        public void WriteHeaderLine(string line)
        {
            _Con.WriteAsciiLine(line);
        }

        public void WriteBody(IWebServerUtilities server, string content)
        {
            char[] chars = content.ToCharArray();
            int byteCount = _Encoder.GetByteCount(chars, 0, content.Length, true);
            byte[] bytes = new byte[byteCount];
            _Encoder.GetBytes(chars, 0, content.Length, bytes, 0, true);
            WriteBody(server, bytes);
        }

        /// <summary>
        /// All writing to the HTTP response body must come through here
        /// to support chunked transfer encoding.
        /// </summary>
        /// <param name="content"></param>
        public void WriteBody(IWebServerUtilities server, byte[] content)
        {
            WriteHeadersIfNeeded(server);
            if (!_HeadersOnly)
            {
                int bytesWritten = 0;
                while (bytesWritten < content.Length)
                {
                    int amountRemaining = content.Length - bytesWritten;
                    int spaceRemaining = _OutputBuffer.Length - _OutputBufferUsed;
                    int copyLength;
                    if (amountRemaining >= spaceRemaining)
                        copyLength = spaceRemaining;
                    else
                        copyLength = amountRemaining;
                    Array.Copy(content, bytesWritten, _OutputBuffer, _OutputBufferUsed, copyLength);
                    _OutputBufferUsed += copyLength;
                    WriteChunk();
                    bytesWritten += copyLength;
                }
            }
        }

        public void FlushBody(IWebServerUtilities server)
        {
            WriteHeadersIfNeeded(server);
            if (!_HeadersOnly)
            {
                if (_OutputBufferUsed > 0)
                {
                    WriteChunk();
                }
                // Write a zero length chunk to end the chunk stream.
                _Con.WriteAsciiLine("0");
                _Con.WriteAsciiEol();
                // And signal an end to the trailing HTTP headers.
                _Con.WriteAsciiEol();
            }
            _Con.Flush();
        }

        private void WriteHeadersIfNeeded(IWebServerUtilities server)
        {
            if (!_HeadersWritten)
            {
                server.WriteResponseHeadData(this);
                _HeadersWritten = true;
            }
        }

        private void WriteChunk()
        {
            string header = _OutputBufferUsed.ToString("X");
            _Con.WriteAsciiLine(header);
            
            _Con.Write(_OutputBuffer, _OutputBufferUsed);
            _Con.WriteAsciiEol();
            _OutputBufferUsed = 0;
        }
    }
}
