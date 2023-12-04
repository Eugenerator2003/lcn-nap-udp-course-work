using Network.Nodes;
using Network.Nodes.UDP;
using System.Drawing;
using System.Net;
using System.Text;

namespace ServerTest
{
    internal class Program
    {
        static IPEndPoint point;
        static bool canSend;
        static Bitmap processed;
        static bool canSave;
        static int counter = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Server");

            INetworkNode.MessageHandler handler = (byte[] data, IPEndPoint endPoint) => {
                Console.WriteLine("Out: " + data.Length);
                if (counter > 0)
                {
                    processed = (Bitmap)Image.FromStream(new MemoryStream(data));
                    canSave = true;
                }
                counter++;
            };

            IListenerNetworkNode.ConnectionHandler clientConnected = (IPEndPoint endPoint) =>
            {
                Console.WriteLine(endPoint.ToString() + " connected");
                point = endPoint;
                canSend = true;
            };

            Bitmap testToBytes = new Bitmap("babulex.jpg");

            using var memory = new MemoryStream();
            testToBytes.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);

            UdpListenerNode server = new UdpListenerNode(4, 9000);
            server.OnAllReceived += handler;
            server.OnClientConnected += clientConnected;
            server.Start();
            while (!canSend) ;
            //Thread.Sleep(1000);
            server.EnqueueMessage(memory.ToArray(), point);
            Console.WriteLine("Sended");
            while (!canSave) ;
            processed.Save("processed.jpg");
            Console.ReadLine();
        }
    }
}