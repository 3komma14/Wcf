using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Seterlund.Wcf.Server
{
    class InstanceContextInitializer : IInstanceContextInitializer
    {
        private readonly Func<IExtension<InstanceContext>> _extension;

        public InstanceContextInitializer(Func<IExtension<InstanceContext>> extension)
        {
            _extension = extension;
        }

        public InstanceContextInitializer(IExtension<InstanceContext> extension)
        {
            _extension = () => extension;
        }

        public void Initialize(InstanceContext instanceContext, Message message)
        {
            instanceContext.Extensions.Add(_extension());
        }
    }
}
