using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seterlund.Wcf.Core
{
    public interface IDependencyResolver
    {
        T Get<T>();
        object Get(Type type);
        IEnumerable<T> GetAll<T>();
    }
}
