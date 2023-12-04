using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Network.Nodes.UDP
{
    public class Transfer : IDisposable
    {
        public MemoryStream Stream { get; }
        public bool OkStatus { get; set; }

        public Transfer()
        {
            Stream = new MemoryStream();
        }

        public byte[] ToByteArray()
        {
            return Stream.ToArray();
        }

        public void Dispose()
        {
            Stream.Dispose();
        }
    }
}
