using System;
using System.Collections.Generic;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// One of these is passed to WebPage.Process(). The object passed is actually
    /// a downcast WebContext<TApplication, TSession>, which the WebPage subclass
    /// can upcast to the actual type if the page has been subclassed for a specific
    /// WebSite<TApplication, TSession>.
    /// </summary>
    public class WebContextBase
    {
        private WebRequest _Request;
        private WebResponse _Response;
        private IWebServerUtilities _WebServerUtilities;
        private IWebSiteUtilities _WebSiteInfo;

        public WebContextBase(WebRequest request, WebResponse response,
            IWebServerUtilities webServerUtilities, IWebSiteUtilities webSiteInfo)
        {
            _Request = request;
            _Response = response;
            _WebServerUtilities = webServerUtilities;
            _WebSiteInfo = webSiteInfo;
        }

        public WebRequest Request { get { return _Request; } }
        public WebResponse Response { get { return _Response; } }
        public IWebServerUtilities WebServerUtilities { get { return _WebServerUtilities; } }
        public IWebSiteUtilities WebSiteInfo { get { return _WebSiteInfo; } }
    }
}
