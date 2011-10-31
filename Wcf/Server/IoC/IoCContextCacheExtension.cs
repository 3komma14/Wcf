using System.ServiceModel;
using StructureMap.Pipeline;

namespace Seterlund.Wcf.Server.IoC
{
    public class IoCContextCacheExtension : IExtension<InstanceContext>
    {
        public IObjectCache Cache { get; private set; }

        public static IoCContextCacheExtension Current
        {
            get
            {
                return OperationContext.Current != null ? OperationContext.Current.InstanceContext.Extensions.Find<IoCContextCacheExtension>() : null;
            }
        }

        public void Attach(InstanceContext owner)
        {
            Cache = new MainObjectCache();
        }

        public void Detach(InstanceContext owner)
        {
        }
    }
}
