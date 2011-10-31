using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Seterlund.Wcf.Core;

namespace Seterlund.Wcf.Server.UnitOfWork
{
    class UoWMessageInspector : IDispatchMessageInspector
    {
        private readonly IDependencyResolver _resolver;

        public UoWMessageInspector(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var unitOfWork = _resolver.Get<IUnitOfWork>();
            unitOfWork.Initialize();
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var unitOfWork = _resolver.Get<IUnitOfWork>();
            unitOfWork.Commit();
        }
    }
}
