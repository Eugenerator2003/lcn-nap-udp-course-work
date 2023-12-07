using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Network.Nodes
{
    public interface IListenerNetworkNode : INetworkNode
    {
        /// <summary>
        /// Событие обработки подключения узла
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        public event ConnectionHandler OnClientDisconnected;

        /// <summary>
        /// Событие обработки отключения узла
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        public event ConnectionHandler OnClientConnected;
    }
}
