using System;
using System.Collections.Generic;
using System.Text;

using Willowsoft.WebServerLib;
using Willowsoft.WebContentLib;

namespace Willowsoft.SampleWebApp
{
    public class CustomDebugPageFactory : IWebPageFactory<CustomSiteData, CustomSession>
    {
        public WebPage<CustomSiteData, CustomSession> GetInstance(WebContext<CustomSiteData, CustomSession> context)
        {
            return new CustomDebugPage();
        }
    }

}
