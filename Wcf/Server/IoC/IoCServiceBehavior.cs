using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using StructureMap;

namespace Seterlund.Wcf.Server.IoC
{
    public class IoCServiceBehavior : IServiceBehavior
    {
        private readonly IContainer _container;

        public IoCServiceBehavior(IContainer container)
        {
            _container = container;
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
                        new IoCInstanceProvider(_container, serviceDescription.ServiceType);

                    // Add instance context initializers
                    endpointDispatcher.DispatchRuntime.InstanceContextInitializers.Add(new IoCContextCacheInitializer());
                    foreach (var initializer in _container.GetAllInstances<IInstanceContextInitializer>())
                    {
                        endpointDispatcher.DispatchRuntime.InstanceContextInitializers.Add(initializer);
                    }

                    // Add message inspectors
                    foreach (var inspector in _container.GetAllInstances<IDispatchMessageInspector>())
                    {
                        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
                    }
                }

                // Add errorhandlers
                foreach (var errorHandler in _container.GetAllInstances<IErrorHandler>())
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
