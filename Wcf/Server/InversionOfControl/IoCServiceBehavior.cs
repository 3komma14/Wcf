using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Seterlund.Wcf.Core;
using Seterlund.Wcf.Server.StructureMap;
using Seterlund.Wcf.Server.StructureMap.Pipeline;

namespace Seterlund.Wcf.Server.InversionOfControl
{
    public class IoCServiceBehavior : IServiceBehavior
    {
        private readonly IDependencyResolver _resolver;

        public IoCServiceBehavior(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var cdb in serviceHostBase.ChannelDispatchers)
            {
                var cd = cdb as ChannelDispatcher;
                if (cd == null) continue;
                foreach (var endpointDispatcher in cd.Endpoints)
                {
                    // Add instance provider
                    endpointDispatcher.DispatchRuntime.InstanceProvider =
                        new IoCInstanceProvider(_resolver, serviceDescription.ServiceType);

                    // Add instance context initializers
                    endpointDispatcher.DispatchRuntime.InstanceContextInitializers.Add(new InstanceContextCacheInitializer());
                    foreach (var initializer in _resolver.GetAll<IInstanceContextInitializer>())
                    {
                        endpointDispatcher.DispatchRuntime.InstanceContextInitializers.Add(initializer);
                    }

                    // Add message inspectors
                    foreach (var inspector in _resolver.GetAll<IDispatchMessageInspector>())
                    {
                        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
                    }
                }

                // Add errorhandlers
                foreach (var errorHandler in _resolver.GetAll<IErrorHandler>())
                {
                    cd.ErrorHandlers.Add(errorHandler);
                }
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
