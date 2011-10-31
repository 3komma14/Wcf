using System;
using StructureMap.Pipeline;

namespace Seterlund.Wcf.Server.StructureMap.Pipeline
{
    public abstract class InstanceContextLifecycleBase<TWcf, TNonWcf> : ILifecycle
        where TWcf : ILifecycle, new()
        where TNonWcf : ILifecycle, new()
    {
        private ILifecycle _wcf;
        private ILifecycle _nonWcf;

        public InstanceContextLifecycleBase()
        {
            _wcf = (default(TWcf) == null) ? Activator.CreateInstance<TWcf>() : ((ILifecycle)default(TWcf));
            _nonWcf = (default(TNonWcf) == null) ? Activator.CreateInstance<TNonWcf>() : ((ILifecycle)default(TNonWcf));
        }


        public void EjectAll()
        {
            _wcf.EjectAll();
            _nonWcf.EjectAll();
        }

        public IObjectCache FindCache()
        {
            if (!InstanceContextLifecycle.HasContext())
            {
                return _nonWcf.FindCache();
            }
            return _wcf.FindCache();
        }

        public abstract string Scope { get; }
    }
}
