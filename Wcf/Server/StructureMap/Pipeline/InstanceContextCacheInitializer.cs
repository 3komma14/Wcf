using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Seterlund.Wcf.Server.StructureMap.Pipeline
{
    class InstanceContextCacheInitializer : IInstanceContextInitializer
    {
        public void Initialize(InstanceContext instanceContext, Message message)
        {
            instanceContext.Extensions.Add(new InstanceContextCacheExtension());
        }
    }
}
