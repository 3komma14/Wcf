using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Seterlund.Wcf.Server.IoC
{
    class IoCContextCacheInitializer : IInstanceContextInitializer
    {
        public void Initialize(InstanceContext instanceContext, Message message)
        {
            instanceContext.Extensions.Add(new IoCContextCacheExtension());
        }
    }
}
