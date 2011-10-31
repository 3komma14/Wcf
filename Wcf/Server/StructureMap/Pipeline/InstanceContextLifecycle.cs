using StructureMap.Pipeline;

namespace Seterlund.Wcf.Server.StructureMap.Pipeline
{
    public class InstanceContextLifecycle : ILifecycle
    {
        public void EjectAll()
        {
            FindCache().DisposeAndClear();
        }

        public IObjectCache FindCache()
        {
            return InstanceContextCacheExtension.Current.Cache;
        }

        public string Scope
        {
            get { return typeof(InstanceContextLifecycle).Name; }
        }

        public static bool HasContext()
        {
            return (InstanceContextCacheExtension.Current != null);
        }

    }
}
