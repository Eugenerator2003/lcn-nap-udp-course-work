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
    public class ClientController : IIOController
    {
        public event IController.ByteProcessing? OnReceiving;

        public event IController.ConnectionHandler OnFailedMessaging;

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
            ioNode.OnAllReceived += (bytes, point) => Logger?.Log($"Received bytes: {bytes.Length}");


            ioNode.OnFailedMessaging += (bytes, point) => OnFailedMessaging?.Invoke(); 
            ioNode.OnFailedMessaging += (bytes, point) => Logger?.Log($"Failed on sending to {point}");
            ioNode.OnSuccessConnection += (point) => Logger?.Log($"Successfully connected to {point}");

            ioNode.OnFailedConnection += (point) => OnFailedMessaging?.Invoke();
            ioNode.OnFailedConnection += (point) => isConneted = false;
            ioNode.OnSuccessConnection += (point) => isConneted = true;

            ioNode.Start();
            Logger?.Log($"Client started at {ioNode.LocalEndPoint}");
        }

        public void Send(byte[] bytes)
        {
            ioNode.EnqueueMessage(bytes, server);
        }

        public bool ConnectToServer()
        {
            isConneted = null;
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
