using Seterlund.Wcf.Server.IoC;
using StructureMap.Pipeline;

namespace Seterlund.Wcf.Server.StructureMap
{
    public class HybridInstanceContextLifecycle : InstanceContextLifecycleBase<InstanceContextLifecycle, ThreadLocalStorageLifecycle>
    {
        public override string Scope
        {
            get { return typeof(HybridInstanceContextLifecycle).Name; }
        }
    }
}
