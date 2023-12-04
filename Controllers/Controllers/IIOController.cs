using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeControllers.Controllers
{
    public interface IIOController : IController
    {
        delegate void ByteProcessing(byte[] data);

        event ByteProcessing OnReceiving;

        bool ConnectToServer();

        void Send(byte[] bytes);
    }
}
