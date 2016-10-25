using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Test1();
            Console.WriteLine("Press ENTER to terminate test.");
            Console.ReadLine();
        }

        static void Test1()
        {
            UrlTester.Run("http://localhost:82/index2.html");
            UrlTester.Run("http://localhost:82/bird_turkey.jpg");
            UrlTester.Run("http://localhost:82/redyellow.jpg");
        }
    }
}
