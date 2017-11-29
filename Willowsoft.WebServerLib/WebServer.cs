using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public class WebServer
    {
        private IDiagOutput _DiagOutput;
        private Dictionary<int, WebPortListener> _Listeners;

        public WebServer()
        {
            _Listeners = new Dictionary<int, WebPortListener>();
        }

        public IDiagOutput DiagOutput
        {
            get { return _DiagOutput; }
            set { _DiagOutput = value; }
        }

        public void Add(string hostName, int port, IWebSiteExecutor site)
        {
            WebPortListener listener;
            if (!_Listeners.TryGetValue(port, out listener))
            {
                listener = new WebPortListener(port);
                listener.DiagOutput = DiagOutput;
                listener.Start();
                _Listeners.Add(port, listener);
            }
            listener.Add(hostName, site);
        }

        public void Stop()
        {
            foreach(WebPortListener listener in _Listeners.Values)
            {
                listener.Stop();
            }
        }
    }
}
