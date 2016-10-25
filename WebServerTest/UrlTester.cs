using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace WebServerTest
{
    public class UrlTester
    {
        private static List<UrlTester> _Testers = new List<UrlTester>();
        private Uri _Url;

        private UrlTester(string url)
        {
            _Url = new Uri(url);
        }

        public static void Run(string url)
        {
            UrlTester tester = new UrlTester(url);
            _Testers.Add(tester);
            ThreadPool.QueueUserWorkItem(new WaitCallback(tester.Test));
            ThreadPool.QueueUserWorkItem(new WaitCallback(tester.Test));
            ThreadPool.QueueUserWorkItem(new WaitCallback(tester.Test));
        }

        public void Test(object tester)
        {
            for (; ; )
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_Url);
                try
                {
                    MakeRequest(request);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    return;
                }
            }
        }

        private void MakeRequest(HttpWebRequest request)
        {
            request.KeepAlive = false;
            using (WebResponse response = request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    for (; ; )
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            return;
                    }
                }
            }
        }
    }
}
