using System;
using System.Collections.Generic;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// Worker class to handle a single web site for our own web server.
    /// Not used for web sites hosted in ASP.NET. Mostly what it does
    /// is find the session and IWebPageFactory to use for the request,
    /// use that factory to get a page instance, and call that page
    /// instance to execute the page.
    /// </summary>
    /// <typeparam name="TSiteData"></typeparam>
    /// <typeparam name="TSession"></typeparam>
    public class WebSite<TSiteData, TSession>
        : IWebSiteExecutor
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        private string _SiteRoot;
        private List<IWebPageHandler<TSiteData, TSession>> _WebPageHandlers;
        private TSiteData _SiteData;

        public WebSite(string siteRoot)
        {
            _SiteRoot = siteRoot;
            _WebPageHandlers = new List<IWebPageHandler<TSiteData, TSession>>();
            _SiteData = new TSiteData();
            AddPageHandlers();
        }

        /// <summary>
        /// Add a page handler for all URL's that end with a specific file type,
        /// like ".jpg".
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="endsWith"></param>
        public void AddEndsWithHandler(IWebPageFactory<TSiteData, TSession> factory, string endsWith)
        {
            _WebPageHandlers.Add(new WebPageEndsWithHandler<TSiteData, TSession>(factory, endsWith));
        }

        /// <summary>
        /// Add a page handler for a specific URL, like "/mypage.custom".
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="path"></param>
        public void AddPathHandler(IWebPageFactory<TSiteData, TSession> factory, string path)
        {
            _WebPageHandlers.Add(new WebPagePathHandler<TSiteData, TSession>(factory, path));
        }

        public void AddPageHandlers(IEnumerable<IWebPageHandler<TSiteData, TSession>> handlers)
        {
            foreach (IWebPageHandler<TSiteData, TSession> handler in handlers)
            {
                _WebPageHandlers.Add(handler);
            }
        }

        protected virtual void AddPageHandlers()
        {
            AddPageHandlers(PageTypes<TSiteData, TSession>.StandardPageHandlers());
        }

        public void ExecuteRequest(WebRequest request, WebResponse response, WebPortListener server)
        {
            WebSessionContainer<TSession> sessionContainer = _SiteData.GetSessionContainer(request, response);
            TSession session = sessionContainer.Session;
            WebContext<TSiteData, TSession> context = new WebContext<TSiteData, TSession>(request, response, server,
                new SiteUtilities(_SiteRoot), _SiteData, session);
            server.WriteDiagMessage(context.Request.Verb + " " + context.Request.RequestURI);
            IWebPageFactory<TSiteData, TSession> matchingHandler = null;
            foreach (IWebPageHandler<TSiteData, TSession> handler in _WebPageHandlers)
            {
                if (handler.IsMatch(context.Request.AbsoluteUriPath))
                {
                    matchingHandler = handler.Factory;
                    break;
                }
            }
            if (matchingHandler == null)
            {
                throw new WebExceptions.ResourceNotFound(request.RequestURI);
            }
            switch (context.Request.Verb)
            {
                case HttpVerbs.Get:
                    WebPage<TSiteData, TSession> getPage = matchingHandler.GetInstance(context);
                    getPage.Process(context);
                    response.FlushBody(server);
                    break;
                case HttpVerbs.Head:
                    WebPage<TSiteData, TSession> headPage = matchingHandler.GetInstance(context);
                    headPage.Process(context);
                    break;
                case HttpVerbs.Post:
                    WebPage<TSiteData, TSession> postPage = matchingHandler.GetInstance(context);
                    postPage.Process(context);
                    response.FlushBody(server);
                    break;
                default:
                    throw new WebExceptions.NotImplemented("Verb " + context.Request.Verb + " not supported");
            }
        }

        private class SiteUtilities : IWebSiteUtilities
        {
            private string _SiteRoot;

            public SiteUtilities(string siteRoot)
            {
                _SiteRoot = siteRoot;
            }

            public string MapPath(string url)
            {
                return _SiteRoot + url.Replace('\\','/');
            }
        }
    }
}
