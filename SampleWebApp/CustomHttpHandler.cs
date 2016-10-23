using System;
using System.Collections.Generic;
using System.Text;

using Willowsoft.WebServerLib;
using Willowsoft.WebContentLib;

namespace Willowsoft.SampleWebApp
{
    // TO DO: Is this class even needed if the page factory class
    // has a parameterless constructor and can be made a parameter type?

    /// <summary>
    /// An ASP.NET IHttpHandler to serve pages using CustomDebugPage.
    /// </summary>
    public class CustomHttpHandler : AspNetHttpHandler<CustomSiteData, CustomSession>
    {
        static CustomHttpHandler()
        {
            _PageFactory = new CustomDebugPageFactory();
        }

        private static IWebPageFactory<CustomSiteData, CustomSession> _PageFactory;

        public override IWebPageFactory<CustomSiteData, CustomSession> PageFactory
        {
            get { return _PageFactory; }
        }
    }

}
