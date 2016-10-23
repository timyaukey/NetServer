﻿using System;

using Willowsoft.WebServerLib;

namespace Willowsoft.WebContentLib
{
    public class DebugPageFactory<TSiteData, TSession> : IWebPageFactory<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
    {
        public WebPage<TSiteData, TSession> GetInstance(WebContext<TSiteData, TSession> context)
        {
            return new DebugPage<TSiteData, TSession>();
        }
    }
}