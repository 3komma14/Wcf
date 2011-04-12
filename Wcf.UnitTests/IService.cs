using System;
using System.ServiceModel;

namespace Seterlund.Wcf.UnitTests
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string Echo(string input);
    }

    [ServiceContract]
    public interface ITestServiceChannel : IClientChannel, IService, IDisposable
    {
    }
}
