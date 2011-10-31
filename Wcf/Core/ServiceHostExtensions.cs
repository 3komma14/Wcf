using System.ServiceModel;
using System.ServiceModel.Description;

namespace Seterlund.Wcf.Core
{
    public static class ServiceHostExtensions
    {
        public static void SetClientCredentials(this ServiceHostBase serviceHost, ServiceCredentials serviceCredentials)
        {
            serviceHost.Description.Behaviors.Remove<ServiceCredentials>();
            serviceHost.Description.Behaviors.Add(serviceCredentials);
        }
    }
}
