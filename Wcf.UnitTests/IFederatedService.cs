using System.ServiceModel;

namespace Seterlund.Wcf.UnitTests
{
    [ServiceContract]
    public interface IFederatedService
    {
        [OperationContract]
        string Echo(string input);
    }
}
