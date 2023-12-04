using Network.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Network.Nodes
{
    public interface IClientNetworkNode : INetworkNode
    {
        /// <summary>
        /// Неуспешное подключение к серверу
        /// </summary>
        /// <param name="endPoint"></param>
        public delegate void ConnnectionHandler(IPEndPoint endPoint);

        /// <summary>
        /// Событие успешного подключения к серверу
        /// </summary>
        public event ConnnectionHandler OnSuccessConnection;

        /// <summary>
        /// Событие неуспешного подключения к серверу
        /// </summary>
        public event ConnnectionHandler OnFailedConnection;

        /// <summary>
        /// Сообщение серверу о том, что кластер может получать данные
        /// </summary>
        /// <param name="endPoint"></param>
        public void Connect(IPEndPoint endPoint);
    }
}
