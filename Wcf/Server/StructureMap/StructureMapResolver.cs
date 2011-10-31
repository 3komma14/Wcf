using System;
using System.Collections.Generic;
using Seterlund.Wcf.Core;
using StructureMap;

namespace Seterlund.Wcf.Server.StructureMap
{
    public class StructureMapResolver : IDependencyResolver
    {
        private readonly IContainer _container;

        public StructureMapResolver(IContainer container)
        {
            _container = container;
        }

        public T Get<T>()
        {
            return _container.GetInstance<T>();
        }

        public object Get(Type type)
        {
            return _container.GetInstance(type);
        }

        public IEnumerable<T> GetAll<T>()
        {
            return _container.GetAllInstances<T>();
        }
    }
}
