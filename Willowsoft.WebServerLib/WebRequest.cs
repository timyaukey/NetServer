using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public class WebRequest
    {
        private string _Verb;
        private string _RequestURI;             // The second token from the first line of the request.
        private string _AbsoluteUriPath;        // Like "/home/index.html".
        private NameValueCollection _QueryArgs;
        private NameValueCollection _PostVars;
        private NameValueCollection _Headers;
        private bool _KeepAlive;
        private HttpCookieCollection _Cookies;
        private string _SourceIPAddress;
        private Dictionary<string, WebUploadedFile> _UploadedFiles;

        /*
        For ASP.NET:
        Empirically, the Request.Headers collection DOES include the request "Cookie" headers.
        Empirically, the Response.Cookies collection does NOT include request cookies.
        Empirically, adding to the Response.Cookies collection DOES add that cookie to
        the Request.Cookies collection as the documentation states.
         */

        public WebRequest(string verb, NameValueCollection headers, string requestURI, string path,
            NameValueCollection queryArgs, string sourceIPAddress)
        {
            _Verb = verb;
            _Headers = headers;
            _RequestURI = requestURI;
            _AbsoluteUriPath = path;
            _QueryArgs = queryArgs;
            _PostVars = new NameValueCollection();
            _UploadedFiles = new Dictionary<string, WebUploadedFile>();
            _SourceIPAddress = sourceIPAddress;

            SetKeepAlive();

            // Extract cookies from the headers collection.
            _Cookies = new HttpCookieCollection();
            string[] cookieHeaderValues = _Headers.GetValues(HttpHeaders.Cookie);
            if (cookieHeaderValues != null)
            {
                foreach (string cookieHeaderValue in cookieHeaderValues)
                {
                    string[] cookieParts = cookieHeaderValue.Split(';');
                    if (cookieParts.Length > 0)
                    {
                        foreach (string cookiePart in cookieParts)
                        {
                            int equalOffset = cookiePart.IndexOf('=');
                            if (equalOffset > 0)
                            {
                                string cookieName = cookiePart.Substring(0, equalOffset).Trim();
                                string cookieValue = cookiePart.Substring(equalOffset + 1).Trim();
                                HttpCookie cookie = new HttpCookie(cookieName, cookieValue);
                                _Cookies.Add(cookie);
                            }
                        }
                    }
                }
            }
        }

        public string Verb { get { return _Verb; } }
        public string RequestURI { get { return _RequestURI; } }
        public string AbsoluteUriPath { get { return _AbsoluteUriPath; } }
        public NameValueCollection QueryArgs { get { return _QueryArgs; } }
        public string SourceIPAddress { get { return _SourceIPAddress; } }
        public NameValueCollection Headers { get { return _Headers; } }
        public NameValueCollection PostVars { get { return _PostVars; } set { _PostVars = value; } }
        public Dictionary<string, WebUploadedFile> UploadedFiles { get { return _UploadedFiles; } }
        public HttpCookieCollection Cookies { get { return _Cookies; } }

        public bool KeepAlive
        {
            get { return _KeepAlive; }
            set { _KeepAlive = value; }
        }

        private void SetKeepAlive()
        {
            string headerValue = _Headers.Get(HttpHeaders.Connection);
            if (headerValue != null)
            {
                _KeepAlive = headerValue.ToLower() != HttpHeaders.Connection_Close;
                return;
            }
            _KeepAlive = true;
        }

        public string HostName
        {
            get
            {
                string hostName = _Headers.Get("Host Name");
                if (hostName != null)
                {
                    int colonPos = hostName.IndexOf(':');
                    if (colonPos > 0)
                        return hostName.Substring(0, colonPos);
                    return hostName;
                }
                return "localhost";
            }
        }
    }
}
