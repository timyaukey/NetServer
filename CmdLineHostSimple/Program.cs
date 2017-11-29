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
            int port = 83;
            string siteRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\SiteRoot");
            System.Console.WriteLine("The simplest possible web server using WebSiteSimple instead of");
            System.Console.WriteLine("custom web site data and session objects.");
            System.Console.WriteLine("SiteRoot [" + siteRoot + "]");
            System.Console.WriteLine("Port [" + port.ToString() + "]");
            System.Console.WriteLine("Home URL http://localhost:{0}/index.html", port);

            WebSiteSimple webSite = new WebSiteSimple(siteRoot);

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
