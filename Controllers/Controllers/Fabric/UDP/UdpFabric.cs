using Network.Nodes.UDP;
using Newtonsoft.Json.Linq;
using NodeControllers.Controllers;
using NodeControllers.Controllers.Fabric;
using NodeControllers.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NodeControllers.Controllers.Fabric.UDP
{
    public class UdpFabric : IFabric
    {
        private JObject configuration;
        private ILogger? logger;

        public UdpFabric()
        {
            configuration = JObject.Parse(File.ReadAllText("config.json"));
        }

        public void UseLogger<T>() where T : ILogger, new()
        {
            logger = new T();
        }

        public IIOController GetClientController()
        {
            UdpClientNode client = new UdpClientNode();
            IPEndPoint server = GetServerEndPoint();
            ClientController clientController = new ClientController(client, server, logger);
            return clientController;
        }

        public IIOController GetClusterController()
        {
            return GetClientController();
        }

        public IServerController GetServer()
        {
            int? clientPort = configuration["host"]?["clientPort"]?.Value<int>();
            int? clientThreads = configuration["host"]?["clientThreads"]?.Value<int>();

            int? clusterPort = configuration["host"]?["clusterPort"]?.Value<int>();
            int? clusterThreads = configuration["host"]?["clusterThreads"]?.Value<int>();

            if (clusterPort is null || clientPort is null || clientThreads is null || clientThreads is null)
            {
                throw new ArgumentNullException("Invalid configuration to server end point");
            }

            UdpListenerNode udpClientNode = new UdpListenerNode(clientThreads.Value, clientPort.Value);
            UdpListenerNode udpClusterNode = new UdpListenerNode(clusterThreads.Value, clusterPort.Value);
            ServerController serverController = new ServerController(udpClientNode, udpClusterNode, logger);
            return serverController;
        }

        private IPEndPoint GetServerEndPoint()
        {
            string? ip = configuration["server"]?["ip"]?.Value<string>();
            int? port = configuration["server"]?["port"]?.Value<int>();

            if (ip is null ||  port is null)
            {
                throw new ArgumentNullException("Invalid configuration to server end point");
            }
            return new IPEndPoint(IPAddress.Parse(ip), port.Value);
        }
    }
}
