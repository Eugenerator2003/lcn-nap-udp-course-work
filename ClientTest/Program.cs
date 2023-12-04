using ImageProcessing.Gaussian;
using Network;
using Network.Nodes.UDP;
using Network.Util;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text;

namespace ClientTest
{
    internal class Program
    {
        static bool canProcess;
        static byte[] imageBytes;
        static IPEndPoint receivedEndPoint;

        static byte[] Process()
        {
            AForgeGaussianFilter gaussianFilter = new AForgeGaussianFilter(3);
            var stream = new MemoryStream();
            stream.Write(imageBytes);
            var image = (Bitmap)Image.FromStream(stream);
            var newImage = gaussianFilter.Apply(image);
            var newStream = new MemoryStream();
            newImage.Save(newStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return newStream.ToArray();
        }

        static void Main(string[] args)
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000);
            UdpClientNode udpClusterNode = new UdpClientNode();
            udpClusterNode.OnAllReceived += (byte[] data, IPEndPoint endPoint) =>
            {
                imageBytes = data;
                receivedEndPoint = endPoint;
            };
            udpClusterNode.OnSuccessConnection += (IPEndPoint EndPoint) =>
            {
                canProcess = true;
            };
            Console.Write("Press any key...");
            Console.ReadLine();
            udpClusterNode.Start();
            udpClusterNode.Connect(serverEndPoint);
            while (!canProcess) ;
            while (imageBytes == null) ;
            udpClusterNode.EnqueueMessage(Process(), receivedEndPoint);
            while (true) ;
        }
    }
}