using Network.Nodes.UDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network.Nodes
{
    /// <summary>
    /// Сетевой узел, реализующий получение и отправку данных
    /// </summary>
    public interface INetworkNode : IDisposable
    {
        /// <summary>
        /// Обработка события
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        public delegate void MessageHandler(byte[] data, IPEndPoint endPoint);

        /// <summary>
        /// Событие обработки полученных данных
        /// </summary>
        public event MessageHandler OnAllReceived;

        /// <summary>
        /// Событие обработки ошибки отправки сообщения
        /// </summary>
        public event MessageHandler OnFailedMessaging;

        /// <summary>
        /// Обработка полученного результата
        /// </summary>
        /// <param name="receiveResult">Результат чтения из UDP сокета</param>
        /// <param name="sendOk">Необходимость отправлять </param>
        /// <returns></returns>
        public delegate void OnReceived(UdpReceiveResult receiveResult, PipelineContext sendOk);

        /// <summary>
        /// Запуск сетевого узла. Чтение и отправка данных
        /// </summary>
        void Start();

        /// <summary>
        /// Остановка работы узла
        /// </summary>
        void End();

        /// <summary>
        /// Размещениие сообщения в очереди на отправку
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        void EnqueueMessage(byte[] data, IPEndPoint endPoint);
    }
}
