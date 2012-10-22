using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HdhrStreamServer
{
    class BadVlcVersionException : Exception
    {
        public BadVlcVersionException() : base() { }
        public BadVlcVersionException(string message) : base(message) { }
        public BadVlcVersionException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected BadVlcVersionException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }

    }
}
