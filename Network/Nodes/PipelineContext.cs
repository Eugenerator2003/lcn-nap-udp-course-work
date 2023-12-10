using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Network.Nodes
{
    public class PipelineContext
    {
        public IPEndPoint EndPoint { get; set; }

        public long TransferNumber { get; set; }

        public byte[] Data { get; set; }

        public bool SendOk { get; set; }

        public bool Next { get; set; }

        public PipelineContext()
        {
            SendOk = true;
            Next = true;
        }
    }
}
