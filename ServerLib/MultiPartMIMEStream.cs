using System;
using System.IO;
using System.Text;

namespace Willowsoft.ServerLib
{
    /// <summary>
    /// A Stream that reads the next part from a MIME multipart message contained in a LookAheadStream.
    /// Reads and returns bytes from the LookAheadStream up until the start of the next boundary.
    /// The caller may choose to read past the part terminating boundary, after all calls to Read() on
    /// this instance are complete, by calling SkipBoundary(). After SkipBoundary() has been called
    /// this class will no longer see the boundary in the LookAheadStream and will merrily read
    /// until another boundary is encountered.
    ///
    /// To read all parts from a MIME multipart message open the underlying message
    /// as a Stream and encapsulate that in a LookAheadStream. Then for each message part create a
    /// new MultiPartMIMEStream and read until EOF. After reaching EOF on each MultiPartMIMEStream
    /// check the LastPartRead property to determine if the part just read was the last part.
    /// The first MultiPartMIMEStream will be used to read the preamble, which is normally ignored.
    /// After reading the last part, the caller should read from the LookAheadStream until EOF
    /// to discard the epilogue.
    /// </summary>
    public class MultiPartMIMEStream : ReadOnlyStream
    {
        private LookAheadStream _Stream;
        private byte[] _BoundaryWithLeadingChars;
        private byte[] _LastBoundaryEnding;
        private byte[] _NonLastBoundaryEnding;
        private int _TerminatingBoundaryLength;
        private bool _LastPartRead;
        private int _TotalBytesRead;

        public MultiPartMIMEStream(LookAheadStream stream, string boundaryWithLeadingChars)
        {
            _Stream = stream;
            Encoder encoder = Encoding.ASCII.GetEncoder();
            string textToEncode;
            int charCount;

            textToEncode = boundaryWithLeadingChars;
            charCount = textToEncode.Length;
            _BoundaryWithLeadingChars = new byte[charCount];
            encoder.GetBytes(textToEncode.ToCharArray(), 0, charCount, _BoundaryWithLeadingChars, 0, true);
            
            textToEncode = "--\r\n";
            charCount = textToEncode.Length;
            _LastBoundaryEnding = new byte[charCount];
            encoder.GetBytes(textToEncode.ToCharArray(), 0, charCount, _LastBoundaryEnding, 0, true);

            textToEncode = "\r\n";
            charCount = textToEncode.Length;
            _NonLastBoundaryEnding = new byte[charCount];
            encoder.GetBytes(textToEncode.ToCharArray(), 0, charCount, _NonLastBoundaryEnding, 0, true);

            _TerminatingBoundaryLength = 0;
            _LastPartRead = false;
            _TotalBytesRead = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            for (; ; )
            {
                if (bytesRead >= count)
                    break;
                if (_Stream.IsEqualAhead(_BoundaryWithLeadingChars, 0))
                {
                    if (_Stream.IsEqualAhead(_LastBoundaryEnding, _BoundaryWithLeadingChars.Length))
                    {
                        _LastPartRead = true;
                        _TerminatingBoundaryLength = _BoundaryWithLeadingChars.Length + _LastBoundaryEnding.Length;
                        break;
                    }
                    if (_Stream.IsEqualAhead(_NonLastBoundaryEnding, _BoundaryWithLeadingChars.Length))
                    {
                        _TerminatingBoundaryLength = _BoundaryWithLeadingChars.Length + _NonLastBoundaryEnding.Length;
                        break;
                    }
                }
                int inputBytesRead = _Stream.Read(buffer, offset + bytesRead, 1);
                if (inputBytesRead == 0)
                    break;
                bytesRead++;
                _TotalBytesRead++;
            }
            return bytesRead;
        }

        public void SkipBoundary()
        {
            byte[] boundaryBuffer = new byte[100];
            _TotalBytesRead += _Stream.Read(boundaryBuffer, 0, _TerminatingBoundaryLength);
        }

        public bool LastPartRead
        {
            get { return _LastPartRead; }
        }

        public int TotalBytesRead
        {
            get { return _TotalBytesRead; }
        }
    }
}
