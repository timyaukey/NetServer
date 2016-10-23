using System;
using System.Collections.Generic;
using System.Text;

using Willowsoft.WebServerLib;
using Willowsoft.WebContentLib;

namespace Willowsoft.SampleWebApp
{
    public class CustomHttpModule : AspNetHttpModule<CustomSiteData, CustomSession>
    {
    }
}
