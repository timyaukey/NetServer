using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public interface IWebPageHandler<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        IWebPageFactory<TSiteData, TSession> Factory { get; }
        bool IsMatch(string absoluteUriPath);
    }

    public class WebPageEndsWithHandler<TSiteData, TSession> : IWebPageHandler<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        private IWebPageFactory<TSiteData, TSession> _Factory;
        private string _EndsWith;

        public WebPageEndsWithHandler(IWebPageFactory<TSiteData, TSession> factory, string endsWith)
        {
            _Factory = factory;
            _EndsWith = endsWith;
        }

        public IWebPageFactory<TSiteData, TSession> Factory
        {
            get { return _Factory; }
        }

        public bool IsMatch(string absoluteUriPath)
        {
            return absoluteUriPath.EndsWith(_EndsWith);
        }
    }

    public class WebPagePathHandler<TSiteData, TSession> : IWebPageHandler<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        private IWebPageFactory<TSiteData, TSession> _Factory;
        private string _Path;

        public WebPagePathHandler(IWebPageFactory<TSiteData, TSession> factory, string path)
        {
            _Factory = factory;
            _Path = path;
        }

        public IWebPageFactory<TSiteData, TSession> Factory
        {
            get { return _Factory; }
        }

        public bool IsMatch(string absoluteUriPath)
        {
            return absoluteUriPath == _Path;
        }
    }
}
