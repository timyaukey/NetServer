using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public class WebSessionContainer<TSession>
        where TSession : class, new()
    {
        private TSession _Session;
        private DateTime _LastAccess;
        private string _RemoteAddress;

        public WebSessionContainer(string remoteAddress)
        {
            _Session = new TSession();
            _LastAccess = DateTime.Now;
            _RemoteAddress = remoteAddress;
        }

        public TSession Session { get { return _Session; } }
        public string RemoteAddress { get { return _RemoteAddress; } }
        public DateTime LastAccess { get { return _LastAccess; } set { _LastAccess = value; } }
    }
}
