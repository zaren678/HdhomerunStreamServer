using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace WpfApplication1
{
    class AsyncSocketServer
    {
        private volatile ManualResetEvent allDone = new ManualResetEvent(false);
        private volatile int mPort = 5454;

        //EVENTS
        public delegate void AcceptHandler(Socket client);
        public event AcceptHandler AcceptComplete;

        public delegate void DataReceivedHandler(String data);
        public event DataReceivedHandler DataReceived;

        public delegate void SendCompleteHandler();
        public event SendCompleteHandler SendComplete;

        private Socket mClient;
        Thread acceptThread;

        public AsyncSocketServer(int port)
        {
            mPort = port;
        }

        public void Accept()
        {
            acceptThread = new Thread(new ThreadStart(AcceptThread));
            acceptThread.Name = "AcceptThread";
            acceptThread.IsBackground = true;

            acceptThread.Start();
        }

        private void AcceptThread()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, mPort);

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                server.Bind(endPoint);
                server.Listen(4); //max 4 connections

                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Listening for connection");

                    server.BeginAccept(new AsyncCallback(AcceptConnection), server);

                    allDone.WaitOne();
                }
            }
            catch (ThreadInterruptedException /*e*/)
            {
                Console.WriteLine("Thread interrupted");
            }
            catch (ThreadAbortException /*e*/)
            {
                Console.WriteLine("Thread Aborted");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                //server.Shutdown(SocketShutdown.Both);
                server.Close();
            }
        }

        private void AcceptConnection(IAsyncResult ar)
        {
            allDone.Set();
            try
            {
                Socket server = (Socket)ar.AsyncState;
                Socket client = server.EndAccept(ar);

                mClient = client;

                AcceptComplete(client);

                SocketData state = new SocketData();
                state.Socket = client;
                client.BeginReceive(state.Buffer, 0, SocketData.BufferSize, 0, new AsyncCallback(ReadCallBack), state);
            }
            catch (ObjectDisposedException)
            {
                //this is the state we get to when the socket is closed
            }
        }

        private void ReadCallBack(IAsyncResult ar)
        {
            String content = String.Empty;

            SocketData clientSocketData = (SocketData)ar.AsyncState;
            Socket client = clientSocketData.Socket;

            int bytesRead = client.EndReceive(ar);

            System.Console.WriteLine("Received {0} bytes of data", bytesRead);

            if (bytesRead > 0)
            {
                //store
                clientSocketData.StringBuild.Append(Encoding.ASCII.GetString(clientSocketData.Buffer, 0, bytesRead));

                content = clientSocketData.StringBuild.ToString();

                if (bytesRead < SocketData.BufferSize)
                {
                    //all data is in
                    content = content.TrimEnd();
                    Console.WriteLine("Read {0} bytes: \n Data: {1}", content.Length, content);

                    DataReceived(content);

                    clientSocketData.StringBuild.Clear();

                    if (content.IndexOf("BYE") > -1)
                    {
                        return;
                    }
                }
                
                Console.WriteLine("Looking for more data");
                //keep reading
                client.BeginReceive(clientSocketData.Buffer, 0, SocketData.BufferSize, 0, new AsyncCallback(ReadCallBack), clientSocketData);
                
            }
        }

        public void Send(Socket handler, String data)
        {
            if (handler.Connected == true)
            {
                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                Console.WriteLine("Sending {0} to client", data);

                // Begin sending the data to the remote device.
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }

            
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                SendComplete();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Close(Socket handler)
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        internal void Stop()
        {
            acceptThread.Interrupt();
        }
    }
}
