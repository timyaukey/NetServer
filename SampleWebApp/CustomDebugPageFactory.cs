using System;
using System.Collections.Generic;
using System.Text;

using Willowsoft.WebServerLib;

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
