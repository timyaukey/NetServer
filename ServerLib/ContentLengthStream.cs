using System;
using System.IO;

namespace Willowsoft.ServerLib
{
    public class ContentLengthStream : ReadOnlyStream
    {
        private Stream _Stream;
        private int _ContentLength;
        private int _BytesRemaining;

        public ContentLengthStream(Stream stream, int contentLength)
        {
            _Stream = stream;
            _ContentLength = contentLength;
            _BytesRemaining = _ContentLength;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_BytesRemaining <= 0)
                return 0;
            int limitedCount;
            if (count <= _ContentLength)
                limitedCount = count;
            else
                limitedCount = _ContentLength;
            int readCount = _Stream.Read(buffer, offset, limitedCount);
            _BytesRemaining -= readCount;
            return readCount;
        }
    }
}
