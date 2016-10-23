using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// An ASP.NET IHttpHandler that can serve pages of any implementation of WebPage.
    /// Is abstract because must be specialized to support specific WebPage
    /// and IWebPageFactory implementations.
    /// There must also be an IHttpModule subclassing AspNetHttpModule<> for the same TSiteData and
    /// TSession, because that AspNetHttpModule<> stores the TSiteData in the underlying ASP.NET Application[]
    /// collection for AspNetHttpHandler<> to use. Any number of different AspNetHttpHandler<> subclasses
    /// may be registered on the web site and share this one AspNetHttpModule<>, because AspNetHttpModule<>
    /// is not specialized for a particular page type.
    /// </summary>
    /// <typeparam name="TSiteData"></typeparam>
    /// <typeparam name="TSession"></typeparam>
    public abstract class AspNetHttpHandler<TSiteData, TSession>
        : IHttpHandler
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        public void ProcessRequest(HttpContext aspNetContext)
        {
            Connection con = new Connection(aspNetContext.Response.OutputStream,
                aspNetContext.Request.UserHostAddress, 0);
            
            // Construct a copy of the HttpRequest.Headers collection, because the one created by ASP.NET
            // is automagically modified whenever the response collection created by ASP.NET is modified.
            // There is no good way to mimic this behavior when running in our own web server, so to
            // maintain compatibility with our own server we use a copy of the ASP.NET request headers
            // so they won't be modified when we add to the response headers.
            NameValueCollection requestHeaders = new NameValueCollection();
            NameValueCollection aspNetRequestHeaders = aspNetContext.Request.Headers;
            foreach (string headerName in aspNetRequestHeaders)
            {
                foreach (string value in aspNetRequestHeaders.GetValues(headerName))
                {
                    requestHeaders.Add(headerName, value);
                }
            }
            WebRequest request = new WebRequest(aspNetContext.Request.RequestType, requestHeaders,
                aspNetContext.Request.RawUrl, aspNetContext.Request.Path,
                aspNetContext.Request.QueryString, con.RemoteAddress);
            for (int fileIndex = 0; fileIndex < aspNetContext.Request.Files.Count; fileIndex++)
            {
                string fieldName = aspNetContext.Request.Files.AllKeys[fileIndex];
                HttpPostedFile aspNetFile = aspNetContext.Request.Files[fileIndex];
                WebUploadedFile file = new WebUploadedFile(aspNetFile.FileName, aspNetFile.InputStream,
                    aspNetFile.ContentLength, aspNetFile.ContentType);
                request.UploadedFiles.Add(fieldName, file);
            }

            WebResponse response = new WebResponse(con, aspNetContext.Response.Headers,
                aspNetContext.Response.Cookies, false);
            TSiteData siteData = (TSiteData)aspNetContext.Application[
                AspNetHttpModule<TSiteData, TSession>.SiteDataKey];
            WebSessionContainer<TSession> sessionContainer = siteData.GetSessionContainer(request, response);
            WebContext<TSiteData, TSession> webContext = new WebContext<TSiteData, TSession>(
                request, response, new ServerUtilities(new NullDiagOutput()), new SiteUtilities(aspNetContext.Server),
                siteData, sessionContainer.Session);

            WebPage<TSiteData, TSession> page = PageFactory.GetInstance(webContext);
            page.Process(webContext);
        }

        public abstract IWebPageFactory<TSiteData, TSession> PageFactory { get; }

        public bool IsReusable
        {
            get { return true; }
        }

        private class SiteUtilities : IWebSiteUtilities
        {
            private HttpServerUtility _Server;

            public SiteUtilities(HttpServerUtility server)
            {
                _Server = server;
            }

            public string MapPath(string url)
            {
                return _Server.MapPath(url);
            }
        }

        private class ServerUtilities : IWebServerUtilities
        {
            private IDiagOutput _DiagOutput;

            public void WriteResponseHeadData(WebResponse response)
            {
                // Does nothing in ASP.NET, because IIS writes this information.
            }

            public ServerUtilities(IDiagOutput diagOutput)
            {
                _DiagOutput = diagOutput;
            }

            public IDiagOutput DiagOutput
            {
                get { return _DiagOutput; }
            }

            public void WriteDiagMessage(string msg)
            {
                _DiagOutput.WriteMessage(msg);
            }

            public void WriteDiagException(Exception ex)
            {
                _DiagOutput.WriteException(ex);
            }
        }
    }
}
