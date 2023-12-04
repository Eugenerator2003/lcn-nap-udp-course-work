using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Network.Util
{
    public class IPEndPointComparer : IEqualityComparer<IPEndPoint>
    {
        public bool Equals(IPEndPoint? x, IPEndPoint? y)
        {
            return x?.Port == y?.Port && (x?.Address.Equals(y?.Address) ?? false);
        }

        public int GetHashCode([DisallowNull] IPEndPoint obj)
        {
            return obj.GetHashCode();
        }
    }
}
