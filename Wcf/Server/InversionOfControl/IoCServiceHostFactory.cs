using System;
using System.ServiceModel.Activation;
using Seterlund.Wcf.Core;
using Seterlund.Wcf.Server.StructureMap;
using StructureMap;

namespace Seterlund.Wcf.Server.InversionOfControl
{
    public class IoCServiceHostFactory : ServiceHostFactory
    {
        private readonly IDependencyResolver _resolver;

        public IoCServiceHostFactory()
        {
            var container = new Container(cfg => cfg.Scan(s => s.Assembly("SomeAssembly")));
            _resolver = new StructureMapResolver(container);
        }

        public IoCServiceHostFactory(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        protected override System.ServiceModel.ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var host = new IoCServiceHost(_resolver, serviceType, baseAddresses);
            return host;
        }
    }
}
