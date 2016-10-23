using System;
using System.Collections.Generic;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// Add strongly typed site data and session types to WebContextBase.
    /// These strongly typed objects may be of any class type with a parameterless
    /// constructor. These type parameters can simply be "object" for web sites 
    /// not wishing to use this feature.
    /// </summary>
    /// <typeparam name="TSiteData"></typeparam>
    /// <typeparam name="TSession"></typeparam>
    public class WebContext<TSiteData, TSession>
        : WebContextBase
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        private TSiteData _SiteData;
        private TSession _Session;

        public WebContext(WebRequest request, WebResponse response,
            IWebServerUtilities webServerUtilities, IWebSiteUtilities webSiteInfo,
            TSiteData siteData, TSession session)
            : base(request, response, webServerUtilities, webSiteInfo)
        {
            _SiteData = siteData;
            _Session = session;
        }

        public TSiteData SiteData { get { return _SiteData; } }
        public TSession Session { get { return _Session; } }
    }
}
