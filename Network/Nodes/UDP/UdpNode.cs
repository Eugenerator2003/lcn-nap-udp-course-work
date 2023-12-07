using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Network.Nodes;
using Network.Util;

namespace Network.Nodes.UDP
{
    public abstract class UdpNode : INetworkNode
    {
        public event INetworkNode.EventHandler OnAllReceived;
        public event INetworkNode.EventHandler OnFailedMessaging;

        public IPEndPoint LocalEndPoint { get => (IPEndPoint)client.Client.LocalEndPoint; }

        protected int port;
        protected Mutex mutex;
        protected bool isEnded = false;
        protected UdpClient client;
        protected ConcurrentQueue<(byte[], IPEndPoint)> sendingQueue;
        protected Dictionary<IPEndPoint, Transfer> transferDict;

        protected INetworkNode.OnReceived pipeline;

        public UdpNode(int port)
        {
            this.port = port;
            sendingQueue = new ConcurrentQueue<(byte[], IPEndPoint)>();
            transferDict = new Dictionary<IPEndPoint, Transfer>(new IPEndPointComparer());
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
            foreach (var stream in transferDict.Values)
            {
                stream.Dispose();
            }
        }

        protected virtual void Sending()
        {
            while (!isEnded)
            {
                if (!sendingQueue.IsEmpty)
                {
                    sendingQueue.TryDequeue(out var pair);
                    SendData(pair.Item1, pair.Item2);
                }
                Thread.Sleep(30);
            }
        }

        protected void Receiving()
        {
            while (!isEnded)
            {
                var result = client.ReceiveAsync().Result;

                PipelineContext context = new PipelineContext();
                foreach (var handle in pipeline.GetInvocationList())
                {
                    handle.DynamicInvoke(result, context);
                }

                if (context.Next)
                {
                    ReceiveData(result, context);
                }

                if (context.SendOk)
                {
                    SendOkMessage(result.RemoteEndPoint);
                }
            }
        }

        protected void SendOkMessage(IPEndPoint endPoint)
        {
            client.Send(Messages.OK, endPoint);
        }

        protected void SendAllMessage(IPEndPoint endPoint)
        {
            client.Send(Messages.ALL, endPoint);
        }

        protected bool SendData(byte[] data, IPEndPoint endPoint)
        {
            var result = true;
            int datagramsCount = (int)Math.Ceiling(data.Length / 65507d) + 1;
            byte[] fullDatagram = new byte[65507];
            for (int i = 0; i < datagramsCount && result; i++)
            {
                byte[] datagram;
                if (i + 1 < datagramsCount)
                {
                    int remainingBytes;
                    if (i + 2 == datagramsCount)
                    {
                        remainingBytes = data.Length - i * Messages.DatagramBodyLenght;
                        byte[] partialDatagram = new byte[remainingBytes];
                        datagram = partialDatagram;
                    }
                    else
                    {
                        remainingBytes = Messages.DatagramBodyLenght;
                        datagram = fullDatagram;
                    }

                    Buffer.BlockCopy(data, Messages.DatagramBodyLenght * i, datagram, 0, remainingBytes);
                }
                else
                {
                    datagram = Messages.ALL;
                }

                var received = SendDatagram(datagram, endPoint);

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

        private bool SendDatagram(byte[] datagramData, IPEndPoint endPoint)
        {
            int counter = 500;
            //int counter = 10000;
            bool received = false;
            client.Send(datagramData, endPoint);

            while (!received && counter > 0)
            {
                received = HasOkFromEndPoint(endPoint);
                counter--;
                Thread.Sleep(10);
            }

            return received;
        }

        private void CheckAll(UdpReceiveResult result, PipelineContext context)
        {
            if (context.Next)
            {
                var endPoint = result.RemoteEndPoint;
                if (result.Buffer.SequenceEqual(Messages.ALL))
                {
                    mutex.WaitOne();
                    if (transferDict.ContainsKey(endPoint))
                    {
                        var data = transferDict[endPoint].ToByteArray();
                        transferDict.Remove(endPoint);
                        OnAllReceived?.Invoke(data, endPoint);
                    }
                    context.Next = false;
                    mutex.ReleaseMutex();
                }
            }
        }

        private void CheckOk(UdpReceiveResult result, PipelineContext context)
        {
            if (context.Next)
            {
                var endPoint = result.RemoteEndPoint;
                if (result.Buffer.SequenceEqual(Messages.OK))
                {
                    mutex.WaitOne();
                    if (!transferDict.ContainsKey(endPoint))
                    {
                        transferDict.TryAdd(endPoint, new Transfer());
                    }
                    transferDict[endPoint].OkStatus = true;
                    context.SendOk = false;
                    context.Next = false;
                    mutex.ReleaseMutex();
                }
            }
        }

        private void ReceiveData(UdpReceiveResult result, PipelineContext context)
        {
            if (context.Next)
            {
                mutex.WaitOne();
                var endPoint = result.RemoteEndPoint;
                if (!transferDict.ContainsKey(endPoint))
                {
                    transferDict.TryAdd(endPoint, new Transfer());
                }
                transferDict[endPoint].Stream.Write(result.Buffer, 0, result.Buffer.Length);
                context.Next = false;
                mutex.ReleaseMutex();
            }
        }

        private bool HasOkFromEndPoint(IPEndPoint endPoint)
        {
            var result = false;
            mutex.WaitOne();
            if (transferDict.ContainsKey(endPoint))
            {
                result = transferDict[endPoint].OkStatus;
                transferDict[endPoint].OkStatus = false;
            }
            mutex.ReleaseMutex();
            return result;
        }
    }
}
