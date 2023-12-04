using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeControllers.Controllers;

namespace NodeControllers.Controllers.Fabric
{
    public interface IFabric
    {
        IIOController GetClientController();

        IIOController GetClusterController();

        IServerController GetServer();
    }
}
