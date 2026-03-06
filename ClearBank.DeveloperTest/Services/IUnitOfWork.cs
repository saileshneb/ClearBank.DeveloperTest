using System;

namespace ClearBank.DeveloperTest.Services
{
    public interface IUnitOfWork
    {
        ITransaction BeginTransaction();
    }

    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
