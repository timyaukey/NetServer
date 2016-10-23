using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public class HttpStatus
    {
        private string _Code;
        private string _Msg;

        public static HttpStatus Ok = new HttpStatus("200", "Ok");
        public static HttpStatus BadRequest = new HttpStatus("400", "Bad Request");
        public static HttpStatus NotFound = new HttpStatus("404", "Not Found");
        public static HttpStatus InternalError = new HttpStatus("500", "Internal Server Error");
        public static HttpStatus NotImplemented = new HttpStatus("501", "Not Implemented");

        private HttpStatus(string code, string msg)
        {
            _Code = code;
            _Msg = msg;
        }

        public string Code { get { return _Code; } }
        public string Msg { get { return _Msg; } }

        public override string ToString()
        {
            return "[" + _Code + " " + _Msg + "]";
        }
    }
}
