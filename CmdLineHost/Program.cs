using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Willowsoft.ServerLib;
using Willowsoft.WebServerLib;
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

            CustomWebSite webSite = new CustomWebSite(siteRoot);

            WebServer webServer = new WebServer();
            webServer.DiagOutput = new ConsoleDiagOutput();
            webServer.Add("localhost", port, webSite);

            System.Console.Out.WriteLine("Press ENTER to exit...");
            System.Console.In.ReadLine();
            webServer.Stop();
        }
    }
}
