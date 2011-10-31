using System;
using System.ServiceModel;
using StructureMap;

namespace Seterlund.Wcf.Server.InversionOfControl
{
    public class IoCServiceHost : ServiceHost
    {
        protected readonly IContainer _container;

        public IoCServiceHost(IContainer container, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            _container = container;
        }

        protected override void OnOpening()
        {
            Description.Behaviors.Add(new IoCServiceBehavior(_container));
            base.OnOpening();
        }
    }
}
