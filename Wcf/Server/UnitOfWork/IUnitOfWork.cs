namespace Seterlund.Wcf.Server.UnitOfWork
{
    interface IUnitOfWork
    {
        void Initialize();
        void Commit();
        void Rollback();
        void Dispose();
    }
}
