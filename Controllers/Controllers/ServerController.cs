using NodeControllers.Loggers;
using Network.Nodes;
using Network.Nodes.UDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeControllers.Controllers
{
    internal class ServerController : IServerController
    {
        public ILogger? Logger { get; }

        private Scheduler scheduler;
        private IListenerNetworkNode clientSide;
        private IListenerNetworkNode clusterSide;

        public ServerController(IListenerNetworkNode clientSide, IListenerNetworkNode clusterSide, ILogger? logger)
        {
            Logger = logger;
            this.clientSide = clientSide;
            this.clusterSide = clusterSide;
            scheduler = new Scheduler(logger, this.clientSide, clusterSide);
        }

        public void Start()
        {
            scheduler.Start();
            clientSide.Start();
            clusterSide.Start();
        }

        public void Dispose()
        {
            clientSide.Dispose();
            clusterSide.Dispose();
            scheduler.Dispose();
        }
    }
}
