using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seterlund.Wcf.Server.UoW
{
    interface IUnitOfWork
    {
        void Initialize();
        void Commit();
        void Rollback();
        void Dispose();
    }
}
