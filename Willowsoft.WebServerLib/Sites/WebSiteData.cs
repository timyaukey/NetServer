using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Web;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    /// <summary>
    /// State information for a web site, both in our web server and ASP.NET.
    /// Includes both application wide (shared) state by subclassing WebSiteData, and a dictionary of sessions.
    /// Is responsible for finding, creating and expiring sessions.
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    public class WebSiteData<TSession>
        where TSession : class, new()
    {
        private IDictionary<string, WebSessionContainer<TSession>> _Sessions;
        private TimeSpan _ExpiresAfter;
        private int _PurgeCountdown;
        private object _SessionsSyncRoot;

        private const int _PurgeCountdownMax = 1000;

        public WebSiteData()
        {
            _Sessions = new ConcurrentDictionary<string, WebSessionContainer<TSession>>();
            _SessionsSyncRoot = new object();
            _ExpiresAfter = new TimeSpan(0, 20, 0);
            _PurgeCountdown = _PurgeCountdownMax;
        }

        private string SessionCookieName { get { return "WebSessionCookieXYZZY"; } }

        public WebSessionContainer<TSession> GetSessionContainer(WebRequest request, WebResponse response)
        {
            lock (SessionsSyncRoot)
            {
                // Periodically purge expired sessions.
                DateTime now = DateTime.Now;
                if (--_PurgeCountdown <= 0)
                {
                    PurgeExpiredSessions(now);
                    _PurgeCountdown = _PurgeCountdownMax;
                }
                // Look for an existing session.
                HttpCookie sessionCookie = request.Cookies[SessionCookieName];
                WebSessionContainer<TSession> sessionContainer;
                if (sessionCookie != null)
                {
                    if (_Sessions.TryGetValue(sessionCookie.Value, out sessionContainer))
                    {
                        if (SessionIsValid(response, now, sessionCookie, sessionContainer))
                            return sessionContainer;
                    }
                }
                // Start a new session.
                string sessionCookieValue = MakeSessionKey(response);
                sessionContainer = new WebSessionContainer<TSession>(response.Con.RemoteAddress);
                _Sessions[sessionCookieValue] = sessionContainer;
                sessionCookie = new HttpCookie(SessionCookieName, sessionCookieValue);
                response.Cookies.Add(sessionCookie);
                return sessionContainer;
            }
        }

        private void PurgeExpiredSessions(DateTime now)
        {
            List<string> keysToDelete = new List<string>();
            foreach (KeyValuePair<string, WebSessionContainer<TSession>> pair in _Sessions)
            {
                if (now.Subtract(pair.Value.LastAccess).CompareTo(_ExpiresAfter) > 0)
                {
                    keysToDelete.Add(pair.Key);
                }
            }
            foreach (string keyToDelete in keysToDelete)
            {
                _Sessions.Remove(keyToDelete);
            }
        }

        private bool SessionIsValid(WebResponse response, DateTime now, HttpCookie sessionCookie,
            WebSessionContainer<TSession> sessionContainer)
        {
            // Has session expired?
            if (now.Subtract(sessionContainer.LastAccess).CompareTo(_ExpiresAfter) > 0)
            {
                // Remove it and say to create a new session.
                _Sessions.Remove(sessionCookie.Value);
            }
            else
            {
                // Make sure this connection is from the same remote address
                // as was used to create the session.
                if (sessionContainer.RemoteAddress == response.Con.RemoteAddress)
                {
                    sessionContainer.LastAccess = now;
                    return true;
                }
            }
            return false;
        }

        private static string MakeSessionKey(WebResponse response)
        {
            string remoteAddress = response.Con.RemoteAddress;
            string sessionCookieValueRaw =
                remoteAddress + ":" +
                DateTime.Now.ToString() + ":" +
                System.Guid.NewGuid().ToString();
            byte[] sessionCookieValueBytes = Encoding.Unicode.GetBytes(sessionCookieValueRaw);
            SHA256Managed sha256 = new SHA256Managed();
            byte[] hashBytes = sha256.ComputeHash(sessionCookieValueBytes);
            string hashText = Convert.ToBase64String(hashBytes);
            string sessionCookieValue = hashText.Replace("=", string.Empty);
            return sessionCookieValue;
        }

        public object SessionsSyncRoot
        {
            get { return _SessionsSyncRoot; }
        }
    }
}
