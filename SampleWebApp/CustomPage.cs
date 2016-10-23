using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

using Willowsoft.ServerLib;
using Willowsoft.WebServerLib;

namespace Willowsoft.SampleWebApp
{
    /// <summary>
    /// A WebPage<> that accesses a custom WebSiteData<> and a custom TSession.
    /// </summary>
    public class CustomPage : WebPage<CustomSiteData, CustomSession>
    {
        public override void Process(WebContext<CustomSiteData, CustomSession> context)
        {
            WebResponse response = context.Response;
            IWebServerUtilities server = context.WebServerUtilities;
            response.SetStdHeaders(context.Request.KeepAlive, ContentTypes.XHTML, 0);

            response.WriteBody(server, WebResponse.XHTML10_Doctype);
            response.WriteBody(server, WebResponse.XHTML10_Html);
            response.WriteBody(server, "<head><title>Custom Title</title></head>");
            response.WriteBody(server, "<body>");
            response.WriteBody(server, "<p>Custom paragraph.</p>");
            response.WriteBody(server, "<p>Executed at: " + DateTime.Now.ToString() + "</p>");
            response.WriteBody(server, "<p>CustomSiteData.MaxUsers=" + context.SiteData.MaxUsers.ToString() + "</p>");
            context.Session.Limit++;
            response.WriteBody(server, "<p>CustomSession.Limit=" + context.Session.Limit.ToString() + "</p>");
            response.WriteBody(server, "</body>");
            response.WriteBody(server, "</html>");
        }

        /// <summary>
        /// The factory that creates this page. Since it is dedicated to CustomPage,
        /// we make it a nested class inside that page. Add an instance of this factory
        /// to the web site with WebSite.AddPathHandler() to hook up the page at a specific URL.
        /// </summary>
        public class Factory : IWebPageFactory<CustomSiteData, CustomSession>
        {
            public WebPage<CustomSiteData, CustomSession> GetInstance(WebContext<CustomSiteData, CustomSession> context)
            {
                return new CustomPage();
            }
        }
    }
}
