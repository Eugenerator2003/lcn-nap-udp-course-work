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
using System.Threading.Tasks.Dataflow;
using System.Drawing;

namespace NodeControllers.Controllers
{
    internal class Scheduler : IDisposable
    {
        private IListenerNetworkNode clientSide;
        private IListenerNetworkNode clusterSide;

        private ConcurrentQueue<(IPEndPoint, byte[])> serializationTasks;
        private ConcurrentQueue<(IPEndPoint, byte[])> processedTasks;

        private ConcurrentDictionary<IPEndPoint, Queue<IPEndPoint>> clusterClientBinding;
        private ConcurrentDictionary<IPEndPoint, bool> clusterReadiness;
        private bool working = false;
        private ILogger? logger;

        private readonly object _lock = new object();

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
                    var clientPoint = task.Item1;
                    var data = task.Item2;

                    while (!clusterReadiness.Values.Any(r => r)) ;

                    var clusterPoint = clusterReadiness.FirstOrDefault(r => r.Value).Key;
                    lock (_lock)
                    {
                        clusterReadiness[clusterPoint] = false;
                        clusterSide.EnqueueMessage(data, clusterPoint);
                        clusterClientBinding.TryGetValue(clusterPoint, out var q);
                        q.Enqueue(clientPoint);
                        //clusterClientBinding.TryAdd(destPoint, srcPoint);
                    }
                }
            }
        }

        private void ClusterProcessing()
        {
            while (working)
            {
                if (processedTasks.TryDequeue(out var task))
                {
                    var clusterPoint = task.Item1;
                    var data = task.Item2;
                    var clientPoints = clusterClientBinding[clusterPoint];
                    //var validData = new byte[data.Length - 3];
                    //Buffer.BlockCopy(data, 0, validData, 0, validData.Length);
                    lock (_lock)
                    {
                        var client = clientPoints.Dequeue();
                        clientSide.EnqueueMessage(data, client);
                        //clusterClientBinding.Remove(clusterPoint, out var temp);
                        clusterReadiness[clusterPoint] = true;
                        logger?.Log($"Sending to {client}");
                    }
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
            clusterSide.OnClientConnected += (point) =>
            {
                clusterReadiness.TryAdd(point, true);
                clusterClientBinding.TryAdd(point, new Queue<IPEndPoint>());
            };
            clusterSide.OnClientDisconnected += (point) =>
            {
                logger?.Log($"Cluster {point} disconnected ");
                clusterReadiness.TryRemove(point, out bool tmp);
                clusterClientBinding.TryRemove(point, out var q);
            };

            clusterSide.OnFailedMessaging += (data, point) => serializationTasks.Enqueue((point, data));

            clusterSide.OnAllReceived += (data, point) =>
            {
                processedTasks.Enqueue((point, data));
                //clusterReadiness[point] = true;
                logger?.Log($"Cluster {point} complete task");
            };
        }
    }
}
