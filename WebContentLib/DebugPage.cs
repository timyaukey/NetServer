using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

using Willowsoft.ServerLib;
using Willowsoft.WebServerLib;

namespace Willowsoft.WebContentLib
{
    /// <summary>
    /// A web page that outputs standard debugging information instead of
    /// content loaded from a file or coming from some other source.
    /// Can be used to test exception handling and HTTP error status
    /// reporting by adding a "force_error" query argument.
    /// </summary>
    public class DebugPage<TSiteData, TSession> : WebPage<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
{
        private IBodyWriter _BodyWriter;

        override public void Process(WebContext<TSiteData, TSession> context)
        {
            WebRequest request = context.Request;
            if (request.QueryArgs["force_error"] != null)
                throw new Exception("Forced exception");
            StringBuilder content = new StringBuilder();
            context.WebServerUtilities.WriteDiagMessage("Executing Debugpage.Initialize()");
            content.Append(WebResponse.XHTML10_Doctype);
            content.Append(WebResponse.XHTML10_Html);
            content.Append(WebResponse.StandardErrorHead("Diagnostic Page"));
            content.AppendLine("<body>");
            content.AppendLine("Verb " + WebResponse.HtmlEncode(request.Verb) + "<br/>");
            content.AppendLine("URL [" + WebResponse.HtmlEncode(request.RequestURI) + "]<br/>");
            content.AppendLine("Path [" + WebResponse.HtmlEncode(request.AbsoluteUriPath) + "]<br/>");
            content.AppendLine("Query Args:<br/>");
            WriteNameValueCollection(content, request.QueryArgs);
            content.AppendLine("Post Vars:<br/>");
            WriteNameValueCollection(content, request.PostVars);

            content.AppendLine("Request Headers:<br/>");
            foreach (string headerName in request.Headers)
            {
                foreach (string headerValue in request.Headers.GetValues(headerName))
                {
                    content.AppendLine("&nbsp;&nbsp;" + headerName + ": [" + WebResponse.HtmlEncode(headerValue) + "]<br/>");
                }
            }

            content.AppendLine("Uploaded Files:<br/>");
            foreach(KeyValuePair<string, WebUploadedFile> pair in request.UploadedFiles)
            {
                content.AppendLine("&nbsp;&nbsp;[name=" + WebResponse.HtmlEncode(pair.Key) +
                    ";file=" + WebResponse.HtmlEncode(pair.Value.FileName) +
                    ";type=" + WebResponse.HtmlEncode(pair.Value.ContentType) +
                    ";Length=" + pair.Value.Length.ToString() + "]<br/>");
            }

            content.AppendLine("Request Cookies:<br/>");
            foreach (string cookieName in request.Cookies)
            {
                HttpCookie cookie = request.Cookies[cookieName];
                content.AppendLine("&nbsp;&nbsp;[" + WebResponse.HtmlEncode(cookie.Name) +
                    "]=[" + WebResponse.HtmlEncode(cookie.Value) + "]<br/>");
            }

            AppendMore(context, content);

            content.AppendLine("</body>");
            content.AppendLine("</html>");

            context.Response.SetStdHeaders(request.KeepAlive, ContentTypes.XHTML, content.Length);

            string setCookieName = context.Request.QueryArgs["cookie-name"];
            if (setCookieName != null)
            {
                string setCookieValue = context.Request.QueryArgs["cookie-value"];
                HttpCookie setCookie = new HttpCookie(setCookieName, setCookieValue);
                string setCookieMinutesText = context.Request.QueryArgs["cookie-minutes"];
                if (setCookieMinutesText != null)
                {
                    double setCookieMinutes;
                    if (double.TryParse(setCookieMinutesText, out setCookieMinutes))
                    {
                        setCookie.Expires = DateTime.Now.AddMinutes(setCookieMinutes);
                    }
                }
                string setCookieDomain = context.Request.QueryArgs["cookie-domain"];
                if (setCookieDomain != null)
                {
                    setCookie.Domain = setCookieDomain;
                }
                string setCookiePath = context.Request.QueryArgs["cookie-path"];
                if (setCookiePath != null)
                {
                    setCookie.Path = setCookiePath;
                }
                context.Response.Cookies.Add(setCookie);
            }

            _BodyWriter = new TextBodyWriter(content.ToString());
            _BodyWriter.WriteBody(context.Response, context.WebServerUtilities);
        }

        private static void WriteNameValueCollection(StringBuilder content, NameValueCollection col)
        {
            foreach (string name in col.AllKeys)
            {
                string[] values = col.GetValues(name);
                foreach (string value in values)
                {
                    content.AppendLine("&nbsp;&nbsp;[" + WebResponse.HtmlEncode(name) +
                        "]=[" + WebResponse.HtmlEncode(value) + "]<br/>");
                }
            }
        }

        /// <summary>
        /// Subclasses can override this method to output strongly typed TApplication and TSession information
        /// by casting "context" to the correct WebContext<TApplication, TSession> type.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        public virtual void AppendMore(WebContext<TSiteData, TSession> context, StringBuilder content)
        {
        }
    }
}
