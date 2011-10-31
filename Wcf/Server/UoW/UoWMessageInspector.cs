using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using StructureMap;

namespace Seterlund.Wcf.Server.UoW
{
    class UoWMessageInspector : IDispatchMessageInspector
    {
        private readonly IContainer _container;

        public UoWMessageInspector(IContainer container)
        {
            _container = container;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var sessionBuilder = _container.GetInstance<IUnitOfWork>();
            sessionBuilder.Initialize();
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var sessionBuilder = _container.GetInstance<IUnitOfWork>();
            sessionBuilder.Commit();
        }
    }
}
