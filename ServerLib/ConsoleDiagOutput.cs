using System;
using System.Collections.Generic;
using System.Text;

namespace Willowsoft.ServerLib
{
    public class ConsoleDiagOutput : IDiagOutput
    {
        public void WriteMessage(string msg)
        {
            System.Console.Out.WriteLine(GetPrefix() + msg);
        }

        public void WriteException(Exception ex)
        {
            System.Console.Out.WriteLine(GetPrefix() + "Exception: " + ex.Message);
        }

        private string GetPrefix()
        {
            return DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " Thread:" +
                System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + " ";
        }
    }
}
