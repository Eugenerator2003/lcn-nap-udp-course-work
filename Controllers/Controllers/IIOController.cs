using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeControllers.Controllers
{
    public interface IIOController : IController
    {
        event ByteProcessing OnReceiving;

        bool ConnectToServer();

        void Send(byte[] bytes);
    }
}
