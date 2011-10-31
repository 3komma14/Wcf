using System.ServiceModel;
using StructureMap.Pipeline;

namespace Seterlund.Wcf.Server.StructureMap.Pipeline
{
    public class OperationContextCacheExtension : IExtension<OperationContext>
    {
        public IObjectCache Cache { get; private set; }

        public static OperationContextCacheExtension Current
        {
            get
            {
                return OperationContext.Current != null ? OperationContext.Current.Extensions.Find<OperationContextCacheExtension>() : null;
            }
        }

        public void Attach(OperationContext owner)
        {
            Cache = new MainObjectCache();
        }

        public void Detach(OperationContext owner)
        {
        }
    }
}
