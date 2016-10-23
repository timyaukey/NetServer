using System;
using System.IO;

namespace Willowsoft.WebServerLib
{
    public class WebUploadedFile
    {
        private string _FileName;
        private Stream _ContentStream;
        private long _Length;
        private string _ContentType;

        public WebUploadedFile(string fileName, Stream contentStream, long length, string contentType)
        {
            _FileName = fileName;
            _ContentStream = contentStream;
            _Length = length;
            _ContentType = contentType;
        }

        public string FileName { get { return _FileName; } }
        public Stream Content { get { return _ContentStream; } }
        public long Length { get { return _Length; } }
        public string ContentType { get { return _ContentType; } }
    }
}
