﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Willowsoft.WebServerLib;
using Willowsoft.WebContentLib;

namespace Willowsoft.SampleWebApp
{
    public class CustomSiteData : WebSiteData<CustomSession>
    {
        public int MaxUsers;
    }
}
