using System.ServiceModel;
using Seterlund.Wcf.Core;

namespace Seterlund.Wcf.Server.UnitOfWork
{
    public class InstanceContextUoWExtension : IExtension<InstanceContext>
    {
        private readonly IDependencyResolver _resolver;

        public InstanceContextUoWExtension(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public void Attach(InstanceContext owner)
        {
            var unitOfWork = _resolver.Get<IUnitOfWork>();
            unitOfWork.Initialize();
            owner.Closing += new System.EventHandler(owner_Closing);
        }

        public void Detach(InstanceContext owner)
        {
            var unitOfWork = _resolver.Get<IUnitOfWork>();
            unitOfWork.Commit();
            unitOfWork.Dispose();
        }

        void owner_Closing(object sender, System.EventArgs e)
        {
            Detach((InstanceContext)sender);
        }

    }
}
