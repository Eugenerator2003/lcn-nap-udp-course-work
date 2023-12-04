using NodeControllers.Loggers;

namespace NodeControllers.Controllers
{
    public interface IController : IDisposable
    {
        ILogger? Logger { get; }

        void Start();
    }
}