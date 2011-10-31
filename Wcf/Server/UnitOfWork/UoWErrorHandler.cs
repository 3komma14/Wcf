using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using StructureMap;

namespace Seterlund.Wcf.Server.UnitOfWork
{
    class UoWErrorHandler : IErrorHandler
    {
        private readonly IContainer _container;

        public UoWErrorHandler(IContainer container)
        {
            _container = container;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            var uow = _container.GetInstance<IUnitOfWork>();
            uow.Rollback();
            uow.Dispose();
        }

        public bool HandleError(Exception error)
        {
            return false;
        }
    }
}
