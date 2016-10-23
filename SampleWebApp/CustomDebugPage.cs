using System;
using System.Collections.Generic;
using System.Text;

using Willowsoft.WebServerLib;
using Willowsoft.WebContentLib;

namespace Willowsoft.SampleWebApp
{
    /// <summary>
    /// Subclass the standard debug page to output data from the custom
    /// site data and session.
    /// </summary>
    public class CustomDebugPage : DebugPage<CustomSiteData, CustomSession>
    {
        public override void AppendMore(WebContext<CustomSiteData, CustomSession> context, StringBuilder content)
        {
            context.Session.Limit++;
            context.SiteData.MaxUsers += 10;
            content.AppendLine("Session.Limit=" + context.Session.Limit.ToString() + "<br/>");
            content.AppendLine("SiteData.MaxUsers=" + context.SiteData.MaxUsers.ToString() + "<br/>");
        }
    }

}
