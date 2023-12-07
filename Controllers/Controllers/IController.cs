using NodeControllers.Loggers;

namespace NodeControllers.Controllers
{
    public interface IController : IDisposable
    {
        delegate void ByteProcessing(byte[] data);

        delegate void ConnectionHandler();

        event ConnectionHandler OnFailedMessaging;

        ILogger? Logger { get; }

        void Start();
    }
}