using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Seterlund.Wcf.Core;

namespace Seterlund.Wcf.Server.UnitOfWork
{
    class UoWErrorHandler : IErrorHandler
    {
        private readonly IDependencyResolver _resolver;

        public UoWErrorHandler(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            var uow = _resolver.Get<IUnitOfWork>();
            uow.Rollback();
            uow.Dispose();
        }

        public bool HandleError(Exception error)
        {
            return false;
        }
    }
}
