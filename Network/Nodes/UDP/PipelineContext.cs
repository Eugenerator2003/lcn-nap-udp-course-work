using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Nodes.UDP
{
    public class PipelineContext
    {
        public bool SendOk { get; set; }

        public bool Next { get; set; }

        public PipelineContext()
        {
            SendOk = true;
            Next = true;
        }
    }
}
