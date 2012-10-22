using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HdhrStreamServer
{
    [Serializable()]
    public class ViewerNotFoundException : System.Exception
    {
        public ViewerNotFoundException() : base() { }
        public ViewerNotFoundException(string message) : base(message) { }
        public ViewerNotFoundException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected ViewerNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
