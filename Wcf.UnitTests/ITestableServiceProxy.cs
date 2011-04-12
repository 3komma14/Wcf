using System.ServiceModel;

namespace Seterlund.Wcf.UnitTests
{
    /// <summary>
    /// Testable serviceproxy interface
    /// </summary>
    public interface ITestableServiceProxy : ICommunicationObject
    {
        /// <summary>
        /// Returns the input
        /// </summary>
        void SomeMethod();

        /// <summary>
        /// Returns the input
        /// </summary>
        void SomeOtherMethod();
    }
}
