using System.ServiceModel;
using StructureMap.Pipeline;

namespace Seterlund.Wcf.Server.StructureMap.Pipeline
{
    public class InstanceContextCacheExtension : IExtension<InstanceContext>
    {
        public IObjectCache Cache { get; private set; }

        public static InstanceContextCacheExtension Current
        {
            get
            {
                return OperationContext.Current != null ? OperationContext.Current.InstanceContext.Extensions.Find<InstanceContextCacheExtension>() : null;
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
