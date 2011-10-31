using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Seterlund.Wcf.Core;

namespace Seterlund.Wcf.Server.InversionOfControl
{
    public class IoCInstanceProvider : IInstanceProvider
    {
        private readonly IDependencyResolver _resolver;
        private readonly Type _serviceType;

        public IoCInstanceProvider(IDependencyResolver resolver, Type serviceType)
        {
            _resolver = resolver;
            _serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return _resolver.Get(_serviceType);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
