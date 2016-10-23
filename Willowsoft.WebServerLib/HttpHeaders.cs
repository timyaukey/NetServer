using System;

using Willowsoft.ServerLib;

namespace Willowsoft.WebServerLib
{
    public static class HttpHeaders
    {
        public const string Connection = "Connection";
        public const string Connection_KeepAlive = "keep-alive";
        public const string Connection_Close = "close";
        public const string ContentLength = "Content-Length";
        public const string ContentType = "Content-Type";
        public const string ContentType_WwwFormUrlEncoded = "application/x-www-form-urlencoded";
        public const string ContentType_MultiPartFormData = "multipart/form-data";
        public const string ContentType_TextPlain = "text/plain";
        public const string TransferEncoding = "Transfer-Encoding";
        public const string TransferEncoding_Identity = "identity";
        public const string TransferEncoding_Chunked = "chunked";
        public const string Date = "Date";
        public const string Cookie = "Cookie";
        public const string SetCookie = "Set-Cookie";
        public const string ContentDisposition = "Content-Disposition";
    }
}
