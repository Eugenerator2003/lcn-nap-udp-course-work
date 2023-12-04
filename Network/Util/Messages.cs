using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Util
{
    public static class Messages
    {
        public static int DatagramBodyLenght => 65507;

        public static readonly byte[] OK = Encoding.UTF8.GetBytes("OK");

        public static readonly byte[] ALL = Encoding.UTF8.GetBytes("ALL");

        public static readonly byte[] HELLO = Encoding.UTF8.GetBytes("HELLO");
    }
}
