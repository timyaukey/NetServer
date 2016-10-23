using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Willowsoft.ServerLib
{
    /// <summary>
    /// A generic TCP/IP server intended to be subclassed into a concrete
    /// server like a web server. Mostly responds to incoming TCP/IP connections
    /// by creating a Connection object, and lets subclasses do the rest
    /// of the work in an overridden UseConnection() method.
    /// </summary>
    public abstract class GenericServer : IGenericServerUtilities
    {
        private TcpListener _Listener;
        private IDiagOutput _DiagOutput;

        public GenericServer(int port)
        {
            _Listener = new TcpListener(IPAddress.Any, port);
            _DiagOutput = new NullDiagOutput();
        }

        public IDiagOutput DiagOutput
        {
            get { return _DiagOutput; }
            set { _DiagOutput = value; }
        }

        public void WriteDiagMessage(string msg)
        {
            _DiagOutput.WriteMessage(msg);
        }

        public void WriteDiagException(Exception ex)
        {
            _DiagOutput.WriteException(ex);
        }

        /// <summary>
        /// Start accepting connections, and return immediately without blocking.
        /// </summary>
        public void Start()
        {
            _Listener.Start();
            BeginAcceptConnection();
        }

        /// <summary>
        /// Stop accepting connections.
        /// Causes Connected() to be called one last time, to finish the
        /// asynchronous operation begun by calling BeginAcceptConnection().
        /// </summary>
        public void Stop()
        {
            _Listener.Stop();
        }

        /// <summary>
        /// Asynchronous callback when incoming connection received.
        /// Also triggered by call to TcpListener.Stop().
        /// </summary>
        /// <param name="result"></param>
        private void Connected(IAsyncResult result)
        {
            // Socket.IsBound==false if called because TcpListener.Stop() was called.
            try
            {
                if (_Listener.Server.IsBound)
                {
                    WriteDiagMessage("ConnectionStart");
                    TcpListener listener = (TcpListener)(result.AsyncState);
                    using (TcpClient tcpClient = listener.EndAcceptTcpClient(result))
                    {
                        BeginAcceptConnection();
                        string remoteAddress;
                        int socketHandle;
                        using (Stream stream = GetStream(tcpClient, out remoteAddress, out socketHandle))
                        {
                            Connection con = new Connection(stream, remoteAddress, socketHandle);
                            WriteDiagMessage("UseConnectionStart");
                            UseConnection(con);
                            WriteDiagMessage("UseConnectionEnd");
                        }
                    }
                    WriteDiagMessage("ConnectionEnd");
                }
            }
            catch (Exception ex)
            {
                WriteDiagException(ex);
            }
        }

        private void BeginAcceptConnection()
        {
            AsyncCallback callback = new AsyncCallback(Connected);
            _Listener.BeginAcceptTcpClient(callback, _Listener);
        }

        private Stream GetStream(TcpClient tcpClient, out string remoteAddress, out int socketHandle)
        {
            NetworkStream netStream=tcpClient.GetStream();
            netStream.ReadTimeout = 5 * 1000;   // 5 seconds
            remoteAddress = tcpClient.Client.RemoteEndPoint.ToString();
            int colonOffset = remoteAddress.IndexOf(':');
            if (colonOffset >= 0)
                remoteAddress = remoteAddress.Substring(0, colonOffset);
            socketHandle = (int)tcpClient.Client.Handle;
            BufferedStream bufStream = new BufferedStream(netStream);
            return bufStream;
        }

        protected abstract void UseConnection(Connection con);

        private class NullDiagOutput : IDiagOutput
        {
            public void WriteMessage(string msg) { }
            public void WriteException(Exception ex) { }
        }
    }
}
