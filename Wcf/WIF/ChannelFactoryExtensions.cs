using System.ServiceModel;
using Microsoft.IdentityModel.Protocols.WSTrust;

namespace Seterlund.Wcf.WIF
{
    public static class ChannelFactoryExtensions
    {
        /// <summary>
        /// Checks id the channelFactory is configured for WIF.
        /// channelFactory.ConfigureChannelFactory() has been called
        /// </summary>
        /// <param name="channelFactory">
        /// The channel factory.
        /// </param>
        /// <typeparam name="T">
        /// The service contract
        /// </typeparam>
        /// <returns>
        /// true if the ClientCredentials is of type FederatedClientCredentials, else false
        /// </returns>
        public static bool IsConfiguredAsFederated<T>(this ChannelFactory<T> channelFactory)
        {
            if (channelFactory.Credentials is FederatedClientCredentials)
            {
                return true;
            }

            return false;
        }
    }
}
