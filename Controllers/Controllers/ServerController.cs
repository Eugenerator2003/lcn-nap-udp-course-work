using NodeControllers.Loggers;
using Network.Nodes;
using Network.Nodes.UDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NodeControllers.Controllers
{
    public class ServerController : IServerController
    {
        public ILogger? Logger { get; }

        private Scheduler scheduler;
        private IListenerNetworkNode clientSide;
        private IListenerNetworkNode clusterSide;

        public event IController.ConnectionHandler OnFailedMessaging;

        public ServerController(IListenerNetworkNode clientSide, IListenerNetworkNode clusterSide, ILogger? logger)
        {
            Logger = logger;
            this.clientSide = clientSide;
            this.clusterSide = clusterSide;
            scheduler = new Scheduler(logger, this.clientSide, clusterSide);
        }

        public void Start()
        {
            clusterSide.OnClientConnected += (point) =>
            {
                Logger?.Log($"Cluster {point} connected");
            };

            scheduler.Start();
            clientSide.Start();
            clusterSide.Start();

            Logger?.Log($"Client side started at {clientSide.LocalEndPoint}");
            Logger?.Log($"Cluster side started at {clusterSide.LocalEndPoint}");
        }

        public void Dispose()
        {
            clientSide.Dispose();
            clusterSide.Dispose();
            scheduler.Dispose();
        }
    }
}
