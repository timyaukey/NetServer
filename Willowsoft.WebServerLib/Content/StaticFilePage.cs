using System;
using System.Collections.Generic;

namespace Willowsoft.WebServerLib
{
    public class StaticFilePage<TSiteData, TSession> : WebPage<TSiteData, TSession>
        where TSiteData : WebSiteData<TSession>, new()
        where TSession : class, new()
	{
        private string _ContentType;

        public StaticFilePage(string contentType)
        {
            _ContentType = contentType;
        }

        override public void Process(WebContext<TSiteData, TSession> context)
        {
            try
            {
                string uriPath = context.WebSiteInfo.MapPath(context.Request.AbsoluteUriPath);
                byte[] content = System.IO.File.ReadAllBytes(uriPath);
                context.Response.SetStdHeaders(context.Request.KeepAlive, _ContentType, content.Length);
                IBodyWriter bodyWriter = new BytesBodyWriter(content);
                bodyWriter.WriteBody(context.Response, context.WebServerUtilities);
            }
            catch(System.IO.FileNotFoundException)
            {
                throw new WebExceptions.ResourceNotFound(context.Request.RequestURI);
            }
        }
    }
}
