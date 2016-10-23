using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Willowsoft.ServerLib;
using Willowsoft.WebServerLib;
using Willowsoft.WebContentLib;
using Willowsoft.SampleWebApp;

namespace NetServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 82;
            string siteRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\SiteRoot");
            System.Console.WriteLine("A web server supporting a web site with a subclassed WebSiteData<>");
            System.Console.WriteLine("and a custom TSession type, whose members are accessed by a custom debug page.");
            System.Console.WriteLine("Also has a custom page showing how to write dynamic output and access");
            System.Console.WriteLine("the WebSiteData<> and TSession data.");
            System.Console.WriteLine("SiteRoot [" + siteRoot + "]");
            System.Console.WriteLine("Port [" + port.ToString() + "]");
            System.Console.WriteLine("Home URL http://localhost:{0}/index.html", port);

            WebSite<CustomSiteData, CustomSession> webSite = new WebSite<CustomSiteData, CustomSession>(siteRoot);
            webSite.AddPageHandlers(PageTypes<CustomSiteData, CustomSession>.StandardPageHandlers());
            // This debug page outputs members of CustomApplication and CustomSession.
            webSite.AddEndsWithHandler(new CustomDebugPageFactory(), ".tdebug");
            // Handle requests for URL "/special.mytype"
            webSite.AddPathHandler(new CustomPage.Factory(), "/special.mytype");

            WebServer webServer = new WebServer(port);
            webServer.DiagOutput = new ConsoleDiagOutput();
            webServer.Add(webSite, "localhost");

            webServer.Start();
            System.Console.Out.WriteLine("Press ENTER to exit...");
            System.Console.In.ReadLine();
            webServer.Stop();
        }
    }
}
