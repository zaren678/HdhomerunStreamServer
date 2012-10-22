using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace HdhrStreamServer
{
    class SocketData
    {
        private Socket mSocket = null;
        public const int BufferSize = 1024;
        private byte[] mBuffer = new byte[BufferSize];
        private StringBuilder mStringBuilder = new StringBuilder();

        public SocketData()
        {
        }

        public Socket Socket 
        { 
            get{ return mSocket;}
            set{ mSocket = value;}
        }

        public byte[] Buffer 
        { 
            get{ return mBuffer;}
            set{ mBuffer = value;}
        }

        public StringBuilder StringBuild 
        { 
            get{ return mStringBuilder;}
            set{ mStringBuilder = value;} 
        }

    }
}
