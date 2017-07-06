using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Emerald.Net.TCP.Server
{
    public sealed class UserToken
    {
        public Socket OwnerSocket { get; set; }

        public UserToken ( Socket owner )
        {
            OwnerSocket = owner;
        }
    }
}
