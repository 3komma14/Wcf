using System;
using System.ServiceModel;
using Seterlund.Wcf.Core;

namespace Seterlund.Wcf.Server.InversionOfControl
{
    public class IoCServiceHost : ServiceHost
    {
        protected readonly IDependencyResolver Resolver;

        public IoCServiceHost(IDependencyResolver resolver, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            Resolver = resolver;
        }

        protected override void OnOpening()
        {
            Description.Behaviors.Add(new IoCServiceBehavior(Resolver));
            base.OnOpening();
        }
    }
}
