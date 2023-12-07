using Network.Nodes;
using Network.Nodes.UDP;
using NodeControllers.Controllers.Fabric.UDP;
using NodeControllers.Loggers;
using System.Drawing;
using System.Net;
using System.Text;

namespace ServerTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UdpFabric fabric = new UdpFabric();
            fabric.UseLogger<ConsoleLogger>();
            var server = fabric.GetServer();

            server.Start();
            while (true) ;
        }
    }
}