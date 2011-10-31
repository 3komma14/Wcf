using System;
using System.ServiceModel.Activation;
using StructureMap;

namespace Seterlund.Wcf.Server.IoC
{
    public class IoCServiceHostFactory : ServiceHostFactory
    {
        private readonly IContainer _container;

        public IoCServiceHostFactory()
        {
            _container = new Container(cfg => cfg.Scan(s => s.Assembly("SomeAssembly")));
        }

        public IoCServiceHostFactory(IContainer container)
        {
            _container = container;
        }

        protected override System.ServiceModel.ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var host = new IoCServiceHost(_container, serviceType, baseAddresses);
            return host;
        }
    }
}
