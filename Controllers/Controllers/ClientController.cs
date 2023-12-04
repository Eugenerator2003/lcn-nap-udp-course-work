using NodeControllers.Loggers;
using Network.Nodes;
using Network.Nodes.UDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NodeControllers.Controllers
{
    internal class ClientController : IIOController
    {
        public event IIOController.ByteProcessing? OnReceiving;

        public ILogger? Logger { get; }

        private IPEndPoint server;
        private IClientNetworkNode ioNode;

        private bool? isConneted;

        public ClientController(IClientNetworkNode node, IPEndPoint serverEndPoint, ILogger? logger)
        {
            Logger = logger;
            server = serverEndPoint;
            ioNode = node;
        }

        public void Start()
        {
            ioNode.OnAllReceived += (bytes, point) => OnReceiving?.Invoke(bytes);
            ioNode.OnAllReceived += (bytes, point) => Logger?.Log($"received bytes: {bytes.Length}");
        }

        public void Send(byte[] bytes)
        {
            ioNode.EnqueueMessage(bytes, server);
        }

        public bool ConnectToServer()
        {
            ioNode.OnFailedConnection += (point) => isConneted = false;
            ioNode.OnSuccessConnection += (point) => isConneted = true;
            ioNode.OnAllReceived += (data, point) => OnReceiving?.Invoke(data);
            ioNode.Connect(server);
            while (isConneted == null) ;
            return (bool)isConneted;
        }

        public void Dispose()
        {
            ioNode.End();
            ioNode.Dispose();
        }
    }
}
