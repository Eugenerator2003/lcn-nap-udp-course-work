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
                int taskCount = serializationTasks.Count();
                if (taskCount > 0)
                {
                    IPEndPoint[] free;
                    (IPEndPoint, byte[])[] tasks;
                    while (!clusterReadiness.Any(c => c.Value)) ;
                    lock (_lock)
                    {
                        free = clusterReadiness.Where(c => c.Value).Select(c => c.Key).ToArray();
                        tasks = new (IPEndPoint, byte[])[free.Length < taskCount ? free.Length : taskCount];
                    }
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        serializationTasks.TryDequeue(out tasks[i]);
                    }
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        var clientpoint = tasks[i].Item1;
                        var data = tasks[i].Item2;

                        clusterReadiness[free[i]] = false;
                        clusterSide.EnqueueMessage(data, free[i]);
                        clusterClientBinding.TryGetValue(free[i], out var q);
                        q.Enqueue(clientpoint);
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
                    IPEndPoint client;
                    lock (_lock)
                    {
                        client = clientPoints.Dequeue();
                        clusterReadiness[clusterPoint] = true;
                    }
                    clientSide.EnqueueMessage(data, client);
                    logger?.Log($"Sending to {client}");
                }
            }
        }

        private void SetClientEvents()
        {
            clientSide.OnAllReceived += (data, point) => logger?.Log($"Received {data.Length} bytes from {point}");
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
