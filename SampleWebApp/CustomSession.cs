using System;

namespace Willowsoft.SampleWebApp
{
    /// <summary>
    /// A strongly typed session type usable as the TSession parameter
    /// type for WebSite<> and related classes.
    /// </summary>
    public class CustomSession
    {
        public string Title;
        public int Limit;
    }
}
