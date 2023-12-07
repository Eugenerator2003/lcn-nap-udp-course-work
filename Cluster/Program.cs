using AForge.Imaging.Filters;
using ImageProcessing;
using ImageProcessing.Gaussian;
using Network;
using Network.Nodes.UDP;
using Network.Util;
using NodeControllers.Controllers.Fabric;
using NodeControllers.Controllers.Fabric.UDP;
using NodeControllers.Loggers;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text;

namespace ClientTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IImageFilter filter = new AForgeGaussianFilter(3, 1);

            UdpFabric fabric = new UdpFabric();
            fabric.UseLogger<ConsoleLogger>();
            var cluster = fabric.GetClusterController();

            cluster.OnReceiving += (data) =>
            {
                GaussianTask task = GaussianTask.FromBytes(data);

                GaussianFilter gFilter = filter as GaussianFilter;
                gFilter.SetParameters(task.Radius, task.Sigma);

                Bitmap filtred = filter.Apply(task.Bitmap);
                MemoryStream forOut = new MemoryStream();
                filtred.Save(forOut, System.Drawing.Imaging.ImageFormat.Jpeg);
                var bytes = forOut.ToArray();
                cluster.Send(bytes);
                forOut.Close();
            };

            Thread.SpinWait(100);
            cluster.Start();
            if (!cluster.ConnectToServer())
            {
                cluster.Dispose();
                Console.WriteLine("Can'n connect to server");
                Console.Write("Press enter to continue...");
                Console.ReadLine();
            }
            else
            {
                while (true) ;
            }
        }
    }
}