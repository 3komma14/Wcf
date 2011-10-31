using Seterlund.Wcf.Server.IoC;
using StructureMap.Pipeline;

namespace Seterlund.Wcf.Server.StructureMap
{
    public class InstanceContextLifecycle : ILifecycle
    {
        public void EjectAll()
        {
            FindCache().DisposeAndClear();
        }

        public IObjectCache FindCache()
        {
            return IoCContextCacheExtension.Current.Cache;
        }

        public string Scope
        {
            get { return typeof(InstanceContextLifecycle).Name; }
        }

        public static bool HasContext()
        {
            return (IoCContextCacheExtension.Current != null);
        }

    }
}
