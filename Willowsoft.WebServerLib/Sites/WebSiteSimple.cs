using System;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// A simple WebSite<> that doesn't have custom web site or session data.
    /// </summary>
    public class WebSiteSimple : WebSite<WebSiteDataSimple, object>
    {
        public WebSiteSimple(string siteRoot)
            : base(siteRoot)
        {
        }
    }
}
