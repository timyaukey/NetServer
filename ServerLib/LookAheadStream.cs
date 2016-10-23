using System;
using System.IO;
using System.Text;

namespace Willowsoft.ServerLib
{
    /// <summary>
    /// A Stream that encapsulates another Stream and adds a look-ahead buffer
    /// of any size.
    /// </summary>
    public class LookAheadStream : ReadOnlyStream
    {
        private int _BufferSize;
        private bool _EndOfInputReached;
        private Stream _Stream;
        private byte[] _CircularBuffer;
        private int _NextWriteOffset;
        private int _BytesInBuffer;

        public LookAheadStream(Stream stream, int bufferSize)
        {
            _Stream = stream;
            _BufferSize = bufferSize;
            _EndOfInputReached = false;
            _CircularBuffer = new byte[_BufferSize];
            _NextWriteOffset = 0;
            _BytesInBuffer = 0;
            FillBuffer();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            int readOffset = GetStartingIndex(0);
            for (int copiedCount = 0; copiedCount < count; copiedCount++)
            {
                // Since we always keep the look-ahead buffer as full as possible,
                // if it is empty then the encapsulated Stream has reached EOF
                // and there is no more data to be read.
                if (_BytesInBuffer == 0)
                    break;
                buffer[offset + bytesRead++] = _CircularBuffer[readOffset];
                _BytesInBuffer--;
                readOffset = (readOffset + 1) % _BufferSize;
                FillBuffer();
            }
            return bytesRead;
        }

        private void FillBuffer()
        {
            while (_BytesInBuffer < _BufferSize && !_EndOfInputReached)
            {
                int bytesRead = _Stream.Read(_CircularBuffer, _NextWriteOffset, 1);
                if (bytesRead <= 0)
                {
                    _EndOfInputReached = true;
                    return;
                }
                _BytesInBuffer++;
                _NextWriteOffset = (_NextWriteOffset + 1) % _BufferSize;
            }
        }

        /// <summary>
        /// Compare for equality the passed byte array to the contents of the circular buffer
        /// at the specified offset from the current beginning index in the circular buffer.
        /// </summary>
        /// <param name="compareTo"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool IsEqualAhead(byte[] compareTo, int offset)
        {
            int compareLength = compareTo.Length;
            if (_BytesInBuffer < compareLength)
                return false;
            int circularIndex = GetStartingIndex(offset);
            for (int compareIndex = 0; compareIndex < compareLength; compareIndex++)
            {
                if (compareTo[compareIndex] != _CircularBuffer[circularIndex])
                    return false;
                circularIndex = (circularIndex + 1) % _BufferSize;
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            int circularIndex = GetStartingIndex(0);
            for (int count = 0; count < _BytesInBuffer; count++)
            {
                char chr = (char)_CircularBuffer[circularIndex];
                if (chr == '\r')
                    result.Append("\\r");
                else if (chr== '\n')
                    result.Append("\\n");
                else
                result.Append(chr);
                circularIndex = (circularIndex + 1) % _BufferSize;
            }
            return result.ToString();
        }

        /// <summary>
        /// The index of the first character in the circular buffer.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        private int GetStartingIndex(int offset)
        {
            // Adding _BufferSize before the modulus insures the number is positive
            // if _BytesInBuffer > _NextWriteOffset.
            return (_NextWriteOffset - _BytesInBuffer + offset + _BufferSize) % _BufferSize;
        }
    }
}
