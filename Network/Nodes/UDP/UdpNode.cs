using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Network.Util;

namespace Network.Nodes.UDP
{
    public abstract class UdpNode : INetworkNode
    {
        private static readonly int NUMBER_LENGHT = 8;

        protected static PipelineContext InitPipelineContext(UdpReceiveResult result)
        {
            byte[] bytes = result.Buffer;
            long[] number = new long[1];
            byte[] data = new byte[bytes.Length - NUMBER_LENGHT];
            Buffer.BlockCopy(bytes, 0, number, 0, NUMBER_LENGHT);
            Buffer.BlockCopy(bytes, NUMBER_LENGHT, data, 0, data.Length);
            var context = new PipelineContext()
            {
                Data = data,
                TransferNumber = number[0],
                EndPoint = result.RemoteEndPoint
            };
            return context;
        }

        public event INetworkNode.EventHandler OnAllReceived;
        public event INetworkNode.EventHandler OnFailedMessaging;

        public IPEndPoint LocalEndPoint { get => (IPEndPoint)client.Client.LocalEndPoint; }

        protected int port;
        protected Mutex mutex;
        protected bool isEnded = false;
        protected UdpClient client;
        protected ConcurrentQueue<(byte[], IPEndPoint)> sendingQueue;
        protected Dictionary<IPEndPoint, List<Transfer>> transferDict;

        protected INetworkNode.ReceiveHandle pipeline;

        public UdpNode(int port)
        {
            this.port = port;
            sendingQueue = new ConcurrentQueue<(byte[], IPEndPoint)>();
            transferDict = new Dictionary<IPEndPoint, List<Transfer>>(new IPEndPointComparer());
            client = new UdpClient(port);
            mutex = new Mutex(false);
            SetReceivePipeline();
        }

        public abstract void Start();

        /// <summary>
        /// Установка конвейера обработки полученных данных
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual void SetReceivePipeline()
        {
            pipeline += CheckAll;
            pipeline += CheckOk;

            OnFailedMessaging += EnqueueAgain;
        }

        public void EnqueueMessage(byte[] data, IPEndPoint endPoint)
        {
            sendingQueue.Enqueue((data, endPoint));
        }

        public void End()
        {
            isEnded = true;
        }

        public void Dispose()
        {
            End();
            client.Dispose();
            foreach (var list in transferDict.Values)
            {
                foreach (var stream in list)
                {
                    stream.Dispose();
                }
            }
        }

        protected virtual void Sending()
        {
            while (!isEnded)
            {
                if (!sendingQueue.IsEmpty)
                {
                    if (sendingQueue.TryDequeue(out var pair))
                    {
                        SendData(pair.Item1, pair.Item2);
                    }
                }
                Thread.Sleep(30);
            }
        }

        protected void Receiving()
        {
            while (!isEnded)
            {
                var result = client.ReceiveAsync().Result;

                PipelineContext context = InitPipelineContext(result);

                //string text = Encoding.UTF8.GetString(context.Data);
 
                foreach (var handle in pipeline.GetInvocationList())
                {
                    handle.DynamicInvoke(context);
                }

                if (context.Next)
                {
                    ReceiveData(context);
                }

                if (context.SendOk)
                {
                    SendOkMessage(context);
                }
            }
        }

        protected void SendOkMessage(PipelineContext context)
        {
            byte[] data = new byte[NUMBER_LENGHT + Messages.OK.Length];
            long[] number = new long[] { context.TransferNumber };
            Buffer.BlockCopy(number, 0, data, 0, NUMBER_LENGHT);
            Buffer.BlockCopy(Messages.OK, 0, data, NUMBER_LENGHT, Messages.OK.Length);
            client.Send(data, context.EndPoint);
            //SendData(data, context.EndPoint);
        }

        protected bool SendData(byte[] data, IPEndPoint endPoint)
        {
            var result = true;
            int datagramsCount = (int)Math.Ceiling(data.Length / (((double)Messages.DatagramBodyLenght) - NUMBER_LENGHT)) + 1;
            byte[] fullDatagram = new byte[Messages.DatagramBodyLenght];
            long[] numberBytes = new long[] { Random.Shared.NextInt64() };
            for (int i = 0; i < datagramsCount && result; i++)
            {
                byte[] datagram;
                int datagramBytes;
                int dataOffset;
                if (i + 1 < datagramsCount)
                {
                    if (i + 2 == datagramsCount)
                    {
                        datagramBytes = data.Length - i * (Messages.DatagramBodyLenght - NUMBER_LENGHT);
                        datagram = new byte[datagramBytes + NUMBER_LENGHT];
                    }
                    else
                    {
                        datagramBytes = Messages.DatagramBodyLenght - NUMBER_LENGHT;
                        datagram = fullDatagram;
                    }
                    dataOffset = (Messages.DatagramBodyLenght - NUMBER_LENGHT) * i;   
                }
                else
                {
                    datagramBytes = Messages.ALL.Length;
                    data = Messages.ALL;
                    datagram = new byte[datagramBytes + NUMBER_LENGHT];
                    dataOffset = 0;
                }

                Buffer.BlockCopy(numberBytes, 0, datagram, 0, NUMBER_LENGHT);
                Buffer.BlockCopy(data, dataOffset, datagram, NUMBER_LENGHT, datagramBytes);

                var received = SendDatagram(datagram, endPoint, numberBytes[0]);

                if (!received)
                {
                    OnFailedMessaging?.Invoke(data, endPoint);
                    result = false;
                }
            }
            return result;
        }

        protected void EnqueueAgain(byte[] bytes, IPEndPoint endPoint)
        {
            sendingQueue.Enqueue((bytes, endPoint));
        }

        private bool SendDatagram(byte[] datagramData, IPEndPoint endPoint, long number)
        {
            int counter = 200;
            //int counter = 10000;
            bool received = false;
            client.Send(datagramData, endPoint);

            while (!received && counter > 0)
            {
                received = HasOkFromEndPoint(endPoint, number);
                counter--;
                Thread.Sleep(10);
            }

            return received;
        }

        private void CheckAll(PipelineContext context)
        {
            if (context.Next)
            {
                var endPoint = context.EndPoint;
                if (context.Data.SequenceEqual(Messages.ALL))
                {
                    //mutex.WaitOne();
                    var tranfser = transferDict[endPoint].FirstOrDefault(l => l.Number == context.TransferNumber);
                    if (tranfser != null)
                    {
                        var data = tranfser.ToByteArray();
                        transferDict[endPoint].Remove(tranfser);
                        //transferDict.Remove(endPoint);
                        OnAllReceived?.Invoke(data, endPoint);
                    }
                    context.Next = false;
                    //mutex.ReleaseMutex();
                }
            }
        }

        private void CheckOk(PipelineContext context)
        {
            if (context.Next)
            {
                var endPoint = context.EndPoint;
                if (context.Data.SequenceEqual(Messages.OK))
                {
                    //mutex.WaitOne();
                    if (!transferDict.ContainsKey(endPoint))
                    {
                        transferDict.TryAdd(endPoint, new List<Transfer>());
                    }
                    var tranfser = transferDict[endPoint].FirstOrDefault(l => l.Number == context.TransferNumber);
                    if (tranfser == null)
                    {
                        tranfser = new Transfer() { Number = context.TransferNumber };
                        transferDict[endPoint].Add(tranfser);
                    }
                    tranfser.OkStatus = true;
                    context.SendOk = false;
                    context.Next = false;
                    //mutex.ReleaseMutex();
                }
            }
        }

        private void ReceiveData(PipelineContext context)
        {
            if (context.Next)
            {
                //mutex.WaitOne();
                var endPoint = context.EndPoint;
                if (!transferDict.ContainsKey(endPoint))
                {
                    transferDict.TryAdd(endPoint, new List<Transfer>());
                }
                var transfer = transferDict[endPoint].FirstOrDefault(l => l.Number == context.TransferNumber);
                if (transfer == null)
                {
                    transfer = new Transfer() { Number = context.TransferNumber };
                    transferDict[endPoint].Add(transfer);
                }
                transfer.Stream.Write(context.Data, 0, context.Data.Length);
                context.Next = false;
                //mutex.ReleaseMutex();
            }
        }

        private bool HasOkFromEndPoint(IPEndPoint endPoint, long number)
        {
            var result = false;
            //mutex.WaitOne();
            if (transferDict.ContainsKey(endPoint))
            {
                var transfer = transferDict[endPoint].FirstOrDefault(l => l.Number == number);
                if (transfer != null && transfer.OkStatus)
                {
                    result = transfer.OkStatus;
                    transfer.OkStatus = false;
                }
            }
            //mutex.ReleaseMutex();
            return result;
        }
    }
}
