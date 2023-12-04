using Network.Nodes;
using Network.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Network.Nodes.IClientNetworkNode;

namespace Network.Nodes.UDP
{
    public class UdpClientNode : UdpNode, IClientNetworkNode
    {
        public event ConnnectionHandler OnSuccessConnection;

        public event ConnnectionHandler OnFailedConnection;

        public UdpClientNode() : this(0)
        {

        }

        public UdpClientNode(int port) : base(port)
        {
        }

        public override void Start()
        {
            Task.Run(() => Receiving());
            Task.Run(() => Sending());
        }

        public void Connect(IPEndPoint endPoint)
        {
            if (!SendData(Messages.HELLO, endPoint))
            {
                OnFailedConnection?.Invoke(endPoint);
            }
            else
            {
                OnSuccessConnection?.Invoke(endPoint);
            }
        }
    }
}
