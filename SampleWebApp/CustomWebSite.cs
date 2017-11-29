using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Willowsoft.ServerLib;
using Willowsoft.WebServerLib;

namespace Willowsoft.SampleWebApp
{
    public class CustomWebSite : WebSite<CustomSiteData, CustomSession>
    {
        public CustomWebSite(string siteRoot)
            :base(siteRoot)
        {
        }

        protected override void AddPageHandlers()
        {
            base.AddPageHandlers();
            // This debug page outputs members of CustomApplication and CustomSession.
            AddEndsWithHandler(new CustomDebugPageFactory(), ".tdebug");
            // Handle requests for URL "/special.mytype"
            AddPathHandler(new CustomPage.Factory(), "/special.mytype");
        }
    }
}
