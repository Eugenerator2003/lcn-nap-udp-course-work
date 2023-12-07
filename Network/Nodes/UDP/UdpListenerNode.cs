using Network.Nodes;
using Network.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Network.Nodes.IListenerNetworkNode;

namespace Network.Nodes.UDP
{
    public class UdpListenerNode : UdpNode, IListenerNetworkNode
    {

        /// <summary>
        /// Событие обработки подключения узла
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        public event INetworkNode.ConnectionHandler OnClientDisconnected;

        /// <summary>
        /// Событие обработки отключения узла
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        public event INetworkNode.ConnectionHandler OnClientConnected;

        private int listeningThreads;

        public UdpListenerNode(int listeningThreads, int port) : base(port)
        {
            this.listeningThreads = listeningThreads;
            //this.client = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
        }

        public override void Start()
        {
            Receiving();
            //Task.Run(() => Receiving());
            Task.Run(() => Sending());
        }

        protected new void Receiving()
        {
            for (int i = 0; i < listeningThreads; i++)
            {
                Task.Run(() => base.Receiving());
            }
        }

        protected override void SetReceivePipeline()
        {
            base.SetReceivePipeline();
            pipeline += CheckHello;

            OnFailedMessaging -= EnqueueAgain;
            OnFailedMessaging += (data, endPoint) => OnClientDisconnected.Invoke(endPoint);
        }

        protected void CheckHello(UdpReceiveResult result, PipelineContext context)
        {
            if (context.Next)
            {
                if (result.Buffer.SequenceEqual(Messages.HELLO))
                {
                    OnClientConnected?.Invoke(result.RemoteEndPoint);
                    context.SendOk = true;
                    context.Next = false;
                }
            }
        }
    }
}
