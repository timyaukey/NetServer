using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Willowsoft.ServerLib
{
    /// <summary>
    /// Encapsulate all access to the underlying Stream connecting the
    /// client and server, both read and write.
    /// </summary>
    public class Connection
    {
        private Stream _Stream;
        private int _SocketHandle;
        private string _RemoteAddress;
        private bool _ConnectionBroken;
        private Encoder _AsciiEncoder;

        public Connection(Stream stream, string remoteAddress, int socketHandle)
        {
            _Stream = stream;
            _RemoteAddress = remoteAddress;
            _SocketHandle = socketHandle;
            _ConnectionBroken = false;
            _AsciiEncoder = Encoding.ASCII.GetEncoder();
        }

        public Stream Stream
        {
            get { return _Stream; }
        }

        public string RemoteAddress
        {
            get { return _RemoteAddress; }
        }

        public int SocketHandle
        {
            get { return _SocketHandle; }
        }

        public bool ConnectionBroken
        {
            get { return _ConnectionBroken; }
            set { _ConnectionBroken = value; }
        }

        public string ReadAsciiLine()
        {
            bool connectionBroken;
            string line;
            ReadAsciiLine(_Stream, out line, out connectionBroken);
            if (connectionBroken)
                _ConnectionBroken = true;
            return line;
        }

        public static void ReadAsciiLine(Stream input, out string line, out bool connectionBroken)
        {
            StringBuilder lineBuilder = new StringBuilder();
            bool dataRead = false;
            int charCode;
            if (input == null)
                throw new InvalidOperationException("input is null");
            for (; ; )
            {
                try
                {
                    charCode = input.ReadByte();
                }
                catch
                {
                    connectionBroken = true;
                    line = null;
                    return;
                }
                if (charCode < 0)
                    break;
                dataRead = true;
                if (charCode == '\n')
                    break;
                if (charCode != '\r')
                    lineBuilder.Append((char)charCode);
            }
            connectionBroken = false;
            if (dataRead)
                line = lineBuilder.ToString();
            else
                line = null;
        }

        public string ReadAsciiUntilEnd(Stream input)
        {
            string content;
            bool connectionBroken;
            ReadAsciiUntilEnd(input, out content, out connectionBroken);
            if (connectionBroken)
                _ConnectionBroken = true;
            return content;
        }

        public static void ReadAsciiUntilEnd(Stream input, out string content, out bool connectionBroken)
        {
            StringBuilder dataBuilder = new StringBuilder();
            bool dataRead = false;
            connectionBroken = false;
            int charCode;
            if (input == null)
                throw new InvalidOperationException("input is null");
            for (; ; )
            {
                try
                {
                    charCode = input.ReadByte();
                }
                catch
                {
                    connectionBroken = true;
                    content = null;
                    charCode = -1;
                }
                if (charCode < 0)
                    break;
                dataRead = true;
                dataBuilder.Append((char)charCode);
            }
            if (dataRead)
                content = dataBuilder.ToString();
            else
                content = null;
        }

        public void WriteAsciiLine(string line)
        {
            WriteAscii(line);
            WriteAsciiEol();
        }

        public void WriteAsciiEol()
        {
            byte[] data = new byte[] { 13, 10 };
            Write(data);
        }

        public void WriteAscii(string value)
        {
            if (!_ConnectionBroken)
            {
                if (_Stream == null)
                    throw new InvalidOperationException("_Stream is null");
                char[] charValues = value.ToCharArray();
                try
                {
                    int byteCount = _AsciiEncoder.GetByteCount(charValues, 0, charValues.Length, true);
                    byte[] bytes = new byte[byteCount];
                    _AsciiEncoder.GetBytes(charValues, 0, charValues.Length, bytes, 0, true);
                    Write(bytes);
                }
                catch
                {
                    _ConnectionBroken = true;
                }
            }
        }

        public void Flush()
        {
            if (!_ConnectionBroken)
            {
                if (_Stream == null)
                    throw new InvalidOperationException("_Stream is null");
                try
                {
                    _Stream.Flush();
                }
                catch
                {
                    _ConnectionBroken = true;
                }
            }
        }

        private void Write(byte[] content)
        {
            Write(content, content.Length);
        }

        public void Write(byte[] content, int length)
        {
            if (!_ConnectionBroken)
            {
                if (_Stream == null)
                    throw new InvalidOperationException("_Stream is null");
                if (content == null)
                    throw new InvalidOperationException("content is null");
                try
                {
                    _Stream.Write(content, 0, length);
                }
                catch
                {
                    _ConnectionBroken = true;
                }
            }
        }
    }
}
