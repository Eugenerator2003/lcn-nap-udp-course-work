using Network.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
            //Task.Run(() => Receiving());
            //Task.Run(() => Sending());
            for (int i = 0; i < listeningThreads; i++)
            {
                Task.Run(() => Sending());
                Task.Run(() => Receiving());
            }
        }

        protected override void SetReceivePipeline()
        {
            base.SetReceivePipeline();
            pipeline += CheckHello;

            //OnFailedMessaging -= EnqueueAgain;
            OnFailedMessaging += (data, endPoint) => OnClientDisconnected?.Invoke(endPoint);
        }

        protected void CheckHello(PipelineContext context)
        {
            if (context.Next)
            {
                if (context.Data.SequenceEqual(Messages.HELLO))
                {
                    OnClientConnected?.Invoke(context.EndPoint);
                    context.SendOk = true;
                    context.Next = false;
                    transferDict.Add(context.EndPoint, new List<Transfer>());
                }
            }
        }
    }
}
