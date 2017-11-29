using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// The portion of our web server that listens on a single port.
    /// Not used by pages hosted in ASP.NET.
    /// Contains a collection of IWebSiteExecutor objects for individual
    /// web sites handled by the server. Mostly responsible for reading and
    /// parsing requests and passing them to the appropriate IWebSiteExecutor.
    /// </summary>
    public class WebPortListener : GenericServer, IWebServerUtilities
    {
        private Dictionary<string, IWebSiteExecutor> _Sites;

        public WebPortListener(int port)
            :base(port)
        {
            _Sites = new Dictionary<string, IWebSiteExecutor>();
        }

        public void Add(string hostName, IWebSiteExecutor site)
        {
            _Sites.Add(hostName, site);
        }
        
        protected override void UseConnection(Connection con)
        {
            if (_Sites.Count == 0)
                throw new InvalidDataException("No WebSite objects have been added to the HttpServer");
            for (; ; )
            {
                bool keepAlive = ExecuteRequest(con);
                if (!keepAlive)
                    break;
            }
        }

        /// <summary>
        /// Execute the HTTP request.
        /// Also catches all exceptions thrown internally, and reports the appropriate
        /// HTTP status and accompanying error page depending on the exception type.
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        private bool ExecuteRequest(Connection con)
        {
            bool headersOnly = false;
            try
            {
                string verb;
                string absoluteUriPath;
                string requestUri;
                NameValueCollection queryArgs;
                NameValueCollection requestHeaders;
                //WriteDiagMessage("ReadFirstLineStart socket=" + con.SocketHandle.ToString());
                string commandLine = con.ReadAsciiLine();
                //WriteDiagMessage("ReadFirstLineEnd");
                if (commandLine == null)
                {
                    WriteDiagMessage("ReadFirstLineEOF");
                    throw new WebExceptions.BadRequest("Missing initial request line");
                }
                requestHeaders = ReadAllHeaders(con, DiagOutput);
                string[] commandLineTokens = commandLine.Split(' ');
                if (commandLineTokens.Length < 2)
                {
                    throw new WebExceptions.BadRequest("Initial request line has too few parts");
                }
                verb = commandLineTokens[0].ToUpper();
                headersOnly = (verb == HttpVerbs.Head);
                requestUri = commandLineTokens[1];
                ParseRequestUri(requestUri, out absoluteUriPath, out queryArgs);
                WebRequest request = new WebRequest(verb, requestHeaders, requestUri, absoluteUriPath,
                    queryArgs, con.RemoteAddress);
                WebResponse response = new WebResponse(con, headersOnly);
                if (request.Verb == HttpVerbs.Post)
                {
                    Stream bodyStream;
                    string transferEncoding = requestHeaders.Get(HttpHeaders.TransferEncoding);
                    if (transferEncoding == HttpHeaders.TransferEncoding_Identity || transferEncoding == null)
                    {
                        string contentLength = requestHeaders.Get(HttpHeaders.ContentLength);
                        bodyStream = new ContentLengthStream(con.Stream, int.Parse(contentLength));
                    }
                    else
                    {
                        throw new WebExceptions.BadRequest(HttpHeaders.TransferEncoding + ":" + transferEncoding + " is not supported");
                    }
                    string contentType = requestHeaders.Get(HttpHeaders.ContentType);
                    if (contentType == HttpHeaders.ContentType_WwwFormUrlEncoded)
                    {
                        string body = con.ReadAsciiUntilEnd(bodyStream);
                        if (body == null)
                        {
                            throw new WebExceptions.BadRequest("Missing POST body");
                        }
                        request.PostVars = ParseNameValuePairs(body);
                    }
                    else if (contentType.StartsWith(HttpHeaders.ContentType_MultiPartFormData))
                    {
                        ReadMultiPartFormData(con, request, contentType, bodyStream);
                    }
                    else
                    {
                        throw new WebExceptions.BadRequest(HttpHeaders.ContentType + ":" + contentType + " is not supported");
                    }
                }
                IWebSiteExecutor requestedSite;
                if (!_Sites.TryGetValue(request.HostName, out requestedSite))
                {
                    throw new WebExceptions.ResourceNotFound(request.RequestURI);
                }
                requestedSite.ExecuteRequest(request, response, this);
                con.Flush();
                if (con.ConnectionBroken)
                    request.KeepAlive = false;
                return request.KeepAlive;
            }
            catch (Exception ex)
            {
                try
                {
                    if (!con.ConnectionBroken)
                    {
                        WebResponse response = new WebResponse(con,headersOnly);
                        WebExceptions.HttpStatusException wex = ex as WebExceptions.HttpStatusException;
                        if (wex != null)
                            WriteHttpError(response, wex.Status, ex);
                        else
                            WriteHttpError(response, HttpStatus.InternalError, ex);
                        con.Flush();
                    }
                }
                catch
                {
                    // Eat all exceptions thrown while processing exceptions.
                }
                return false;
            }
        }

        private void ParseRequestUri(string requestUri, out string absoluteUriPath, out NameValueCollection queryArgs)
        {
            string rawQueryString;
            string argRemaining;
            int fragmentOffset = requestUri.IndexOf('#');
            if (fragmentOffset >= 0)
            {
                argRemaining = requestUri.Substring(0, fragmentOffset);
            }
            else
            {
                argRemaining = requestUri;
            }
            int queryOffset = argRemaining.IndexOf('?');
            if (queryOffset >= 0)
            {
                rawQueryString = argRemaining.Substring(queryOffset + 1);
                absoluteUriPath = UnescapeUriPart(argRemaining.Substring(0, queryOffset));
            }
            else
            {
                rawQueryString = string.Empty;
                absoluteUriPath = UnescapeUriPart(argRemaining);
            }
            queryArgs = ParseNameValuePairs(rawQueryString);
        }

        private static string UnescapeUriPart(string input)
        {
            return Uri.UnescapeDataString(input).Replace('+', ' ');
        }

        private static NameValueCollection ParseNameValuePairs(string input)
        {
            NameValueCollection result = new NameValueCollection();
            if (!string.IsNullOrEmpty(input))
            {
                string[] pairs = input.Split('&');
                foreach (string pair in pairs)
                {
                    int equalOffset = pair.IndexOf('=');
                    if (equalOffset > 0)
                    {
                        string name = pair.Substring(0, equalOffset);
                        name = UnescapeUriPart(name);
                        string value = pair.Substring(equalOffset + 1);
                        value = UnescapeUriPart(value);
                        AddNameValuePair(result, name, value);
                    }
                    else
                    {
                        string pair2 = UnescapeUriPart(pair);
                        AddNameValuePair(result, pair2, string.Empty);
                    }
                }
            }
            return result;
        }

        private static void ReadMultiPartFormData(Connection con, WebRequest request, string contentType, Stream bodyStream)
        {
            request.PostVars = new NameValueCollection();
            int boundaryOffset = contentType.IndexOf("boundary=");
            if (boundaryOffset < 0)
                return;
            string boundary = contentType.Substring(boundaryOffset + 9);
            LookAheadStream lookAheadStream = new LookAheadStream(bodyStream, 200);

            // Read and discard the preamble.
            MultiPartMIMEStream multiPartStream = new MultiPartMIMEStream(lookAheadStream, "--" + boundary);
            for (; ; )
            {
                byte[] discardBuffer = new byte[100];
                if (multiPartStream.Read(discardBuffer, 0, discardBuffer.Length) == 0)
                    break;
            }
            multiPartStream.SkipBoundary();

            // Read and save any following parts.
            for (; ; )
            {
                if (multiPartStream.LastPartRead || multiPartStream.TotalBytesRead == 0)
                    break;
                multiPartStream = new MultiPartMIMEStream(lookAheadStream, "\r\n--" + boundary);
                bool connectionBroken;
                NameValueCollection headers = ReadAllHeaders(multiPartStream, out connectionBroken);
                if (connectionBroken)
                    con.ConnectionBroken = true;
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (BufferedStream partBody = new BufferedStream(memStream, 1024))
                    {
                        multiPartStream.CopyTo(partBody, 1024);
                        multiPartStream.SkipBoundary();
                        partBody.Flush();
                        partBody.Seek(0, SeekOrigin.Begin);
                        string contentDisposition = headers.Get(HttpHeaders.ContentDisposition);
                        if (contentDisposition != null)
                        {
                            // save uploaded files in data structures compatible with the way ASP.NET exposes them
                            string[] parts = contentDisposition.Split(';');
                            bool formDataFound = false;
                            string name = null;
                            string filename = null;
                            foreach (string part in parts)
                            {
                                string trimmedPart = part.Trim();
                                if (trimmedPart == "form-data")
                                    formDataFound = true;
                                else if (trimmedPart.StartsWith("name=\""))
                                {
                                    name = trimmedPart.Substring(6, trimmedPart.Length - 7);
                                }
                                else if (trimmedPart.StartsWith("filename=\""))
                                {
                                    filename = trimmedPart.Substring(10, trimmedPart.Length - 11);
                                }
                            }
                            if (!formDataFound)
                                throw new InvalidDataException();
                            if (filename != null)
                            {
                                AddNameValuePair(request.PostVars, name, filename);
                                partBody.Seek(0, SeekOrigin.Begin);
                                string fileContentType = headers.Get(HttpHeaders.ContentType);
                                if (string.IsNullOrEmpty(fileContentType))
                                    fileContentType = HttpHeaders.ContentType_TextPlain;
                                WebUploadedFile file = new WebUploadedFile(filename, partBody, partBody.Length, fileContentType);
                                request.UploadedFiles.Add(name, file);
                            }
                            else
                            {
                                string value;
                                bool ignoreConnectionBroken;
                                Connection.ReadAsciiUntilEnd(partBody, out value, out ignoreConnectionBroken);
                                AddNameValuePair(request.PostVars, name, value);
                            }
                        }
                    }
                }
            }

            // Discard the epilogue
            for (; ; )
            {
                byte[] discardBuffer = new byte[100];
                if (lookAheadStream.Read(discardBuffer, 0, discardBuffer.Length) == 0)
                    break;
            }
        }

        private static void AddNameValuePair(NameValueCollection col, string name, string value)
        {
            col.Add(name, value);
        }

        private static NameValueCollection ReadAllHeaders(Connection con, IDiagOutput diagOutput)
        {
            bool connectionBroken;
            //diagOutput.WriteMessage("ReadAllHeadersStart socket=" + con.SocketHandle.ToString());
            NameValueCollection headers = ReadAllHeaders(con.Stream, out connectionBroken);
            if (connectionBroken)
                con.ConnectionBroken = true;
            //diagOutput.WriteMessage("ReadAllHeadersEnd");
            return headers;
        }

        private static NameValueCollection ReadAllHeaders(Stream stream, out bool connectionBroken)
        {
            NameValueCollection headers = new NameValueCollection();
            string completeHeader = string.Empty;
            for (; ; )
            {
                string headerLine;
                Connection.ReadAsciiLine(stream, out headerLine, out connectionBroken);
                if (headerLine == null)
                {
                    // Should never happen, because headers should be terminated
                    // by a blank line, but we'll act as if we saw a blank line.
                    AddHeader(headers, completeHeader);
                    break;
                }
                if (headerLine.Length == 0)
                {
                    // This is the expected way to leave.
                    AddHeader(headers, completeHeader);
                    break;
                }
                if (headerLine.StartsWith(" ") || headerLine.StartsWith("\t"))
                {
                    // This line continues the header from the previous line.
                    completeHeader += headerLine;
                }
                else
                {
                    // Since this line does not start with a blank or tab it is the start
                    // of a new header, so add the preceding header to the list.
                    AddHeader(headers, completeHeader);
                    completeHeader = headerLine;
                }
            }
            return headers;
        }

        private static void AddHeader(NameValueCollection headers, string completeHeader)
        {
            if (string.IsNullOrEmpty(completeHeader))
                return;
            int colonPos = completeHeader.IndexOf(':');
            if (colonPos < 1)
                return;
            string headerName = completeHeader.Substring(0, colonPos);
            string headerValue = completeHeader.Substring(colonPos + 1).TrimStart(' ');
            headers.Add(headerName, headerValue);
            //System.Console.Out.WriteLine("Header:{0} Value:{1}", headerName, headerValue);
        }

        private void WriteHttpError(WebResponse response, HttpStatus status, Exception ex)
        {
            string errorContent =
                WebResponse.XHTML10_Doctype +
                WebResponse.XHTML10_Html +
                    WebResponse.StandardErrorHead("HTTP Status " + status.Code + ": " + status.Msg) +
                    "<body>\r\n" +
                    "<h3>" + WebResponse.HtmlEncode(ex.Message) + "</h3>\r\n" +
                    "<p><strong>Stack Trace:</strong></p>\r\n" +
                    "<p style='white-space:pre;'>" +
                    WebResponse.HtmlEncode(ex.StackTrace) +
                    "</p>\r\n" +
                    "</body>\r\n" +
                "</html>";
            response.SetStdHeaders(false, ContentTypes.XHTML, errorContent.Length);
            response.Status = status;
            response.WriteBody(this, errorContent);
        }

        public void WriteResponseHeadData(WebResponse response)
        {
            WriteStatus(response);
            WriteHeaders(response);
        }

        private void WriteStatus(WebResponse response)
        {
            response.WriteHeaderLine("HTTP/1.1 " + response.Status.Code + " " + response.Status.Msg);
        }

        private void WriteHeaders(WebResponse response)
        {
            for (int i = 0; i < response.Headers.Count; i++)
            {
                string name = response.Headers.GetKey(i);
                foreach (string value in response.Headers.GetValues(i))
                {
                    response.WriteHeaderLine(name + ": " + value);
                }
            }
            foreach (string cookieName in response.Cookies)
            {
                HttpCookie cookie = response.Cookies[cookieName];
                StringBuilder cookieText = new StringBuilder(cookie.Value);
                if (cookie.Expires != DateTime.MinValue)
                {
                    cookieText.Append("; Expires=" + cookie.Expires.ToUniversalTime().ToString(
                        "ddd, dd-MMM-yyyy HH':'mm':'ss 'GMT'"));
                }
                if (!string.IsNullOrEmpty(cookie.Domain))
                {
                    cookieText.Append("; Domain=" + cookie.Domain);
                }
                if (cookie.Path != "/")
                {
                    cookieText.Append("; Path=" + cookie.Path);
                }
                if (cookie.Secure)
                {
                    cookieText.Append("; Secure");
                }
                if (cookie.HttpOnly)
                {
                    cookieText.Append("; HttpOnly");
                }
                response.WriteHeaderLine(HttpHeaders.SetCookie + ": " + cookie.Name + "=" + cookieText.ToString());
            }
            response.WriteHeaderLine(string.Empty);
            response.Flush();
        }

    }
}
