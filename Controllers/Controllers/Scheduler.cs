using NodeControllers.Loggers;
using Network.Nodes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NodeControllers.Controllers
{
    internal class Scheduler : IDisposable
    {
        private IListenerNetworkNode clientSide;
        private IListenerNetworkNode clusterSide;

        private ConcurrentQueue<(IPEndPoint, byte[])> serializationTasks;
        private ConcurrentQueue<(IPEndPoint, byte[])> processedTasks;

        private ConcurrentDictionary<IPEndPoint, IPEndPoint> clusterClientBinding;
        private ConcurrentDictionary<IPEndPoint, bool> clusterReadiness;
        private bool working = false;
        private ILogger? logger;

        public Scheduler(ILogger? logger, IListenerNetworkNode clientSide, IListenerNetworkNode clusterSide)
        {
            this.clientSide = clientSide;
            this.clusterSide = clusterSide;
            this.logger = logger;
            serializationTasks = new();
            processedTasks = new();
            clusterClientBinding = new();
            clusterReadiness = new();
            SetClientEvents();
            SetClusterEvents();
        }

        public void Start()
        {
            working = true;
            Task.Run(() => ClientsProcessing());
            Task.Run(() => ClusterProcessing());
        }

        public void Dispose()
        {
            working = false;
        }

        private void ClientsProcessing()
        {
            while (working)
            {
                if (serializationTasks.TryDequeue(out var task))
                {
                    var srcPoint = task.Item1;
                    var data = task.Item2;

                    while (!clusterReadiness.Values.Any(r => r)) ;
                    var destPoint = clusterReadiness.FirstOrDefault(r => r.Value).Key;
                    clusterReadiness[destPoint] = false;
                    clusterSide.EnqueueMessage(data, destPoint);
                    clusterClientBinding.TryAdd(destPoint, srcPoint);
                }
            }
        }

        private void ClusterProcessing()
        {
            while (working)
            {
                if (processedTasks.TryDequeue(out var task))
                {
                    var srcPoint = task.Item1;
                    var data = task.Item2;
                    var destPoint = clusterClientBinding[srcPoint];
                    clientSide.EnqueueMessage(data, destPoint);
                    clusterClientBinding.Remove(srcPoint, out var temp);
                }
            }
        }

        private void SetClientEvents()
        {
            clientSide.OnAllReceived += (data, point) => serializationTasks.Enqueue((point, data));
            clientSide.OnClientDisconnected += (point) => logger?.Log($"Can't send message to client: {point}");
        }

        private void SetClusterEvents()
        {
            clusterSide.OnClientConnected += (point) => clusterReadiness.TryAdd(point, true);
            clusterSide.OnFailedMessaging += (data, point) => serializationTasks.Enqueue((point, data));
            clusterSide.OnClientDisconnected += (point) =>
            {
                logger?.Log($"Cluster {point} disconnected ");
                clusterReadiness.TryRemove(point, out bool tmp);
                clusterClientBinding.TryRemove(point, out IPEndPoint? tmp2);
            };
            clusterSide.OnAllReceived += (data, point) =>
            {
                processedTasks.Enqueue((point, data));
                clusterReadiness[point] = true;
            };
        }
    }
}
