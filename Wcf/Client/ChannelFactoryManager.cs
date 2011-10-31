using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Seterlund.Wcf.Core;

namespace Seterlund.Wcf.Client
{
    /// <summary>
    /// Manages creation of channel factories
    /// </summary>
    public class ChannelFactoryManager : IChannelFactoryManager
    {

        /// <summary>
        /// Initializes static members of the <see cref="ChannelFactoryManager"/> class.
        /// </summary>
        static ChannelFactoryManager()
        {
            ChannelFactories = new Dictionary<Type, ChannelFactory>();
        }

        /// <summary>
        /// Gets ChannelFactories.
        /// </summary>
        public static Dictionary<Type, ChannelFactory> ChannelFactories { get; private set; }

        #region IChannelFactoryManager Members

        public ChannelFactory<T> GetFactory<T>()
        {
            return GetChannelFactory<T>();
        }

        public ChannelFactory<T> GetFactory<T>(ClientCredentials clientCredentials)
        {
            return GetChannelFactory<T>(clientCredentials);
        }

        public ChannelFactory<T> GetFactory<T, TBinding>() where TBinding : Binding
        {
            return GetChannelFactory<T, TBinding>();
        }

        #endregion

        /// <summary>
        /// Creates a channel factory for the given interface/type.
        /// It first reads the system.serviceModel/client section to find a single endpoint matching the contract.
        /// If none or multiple entries found it uses the ChannelFactoryManager section to select the correct one.
        /// </summary>
        /// <typeparam name="TContract">
        /// Interface or type
        /// </typeparam>
        /// <returns>
        /// A ChannelFactory
        /// </returns>
        public static ChannelFactory<TContract> CreateChannelFactory<TContract>()
        {
            var channelEndpointElement = ClientConfiguration.GetClientEndpoint<TContract>();
            if (channelEndpointElement == null)
            {
                throw new InvalidOperationException("Unable to get client endpoint");
            }

            return CreateChannelFactory<TContract>(channelEndpointElement.Name);
        }

        public static ChannelFactory<TContract> CreateChannelFactory<TContract, TBinding>() where TBinding : Binding
        {
            var channelEndpointElement = ClientConfiguration.GetClientEndpoint<TContract, TBinding>();
            if (channelEndpointElement == null)
            {
                throw new InvalidOperationException("Unable to get client endpoint");
            }

            return CreateChannelFactory<TContract>(channelEndpointElement.Name);
        }

        public static ChannelFactory<TContract> CreateChannelFactory<TContract>(string endpointConfigurationName)
        {
            var channelFactory = new ChannelFactory<TContract>(endpointConfigurationName);
            if (channelFactory.IsFederated())
            {
                channelFactory.ConfigureChannelFactory();
            }

            return channelFactory;
        }

        /// <summary>
        /// Gets the channelfactory. 
        /// If factory not created before a new instance is created and stored for later retrievals
        /// </summary>
        /// <typeparam name="T">
        /// Type of channelfactory
        /// </typeparam>
        /// <returns>
        /// A ChannelFactory of type T
        /// </returns>
        public static ChannelFactory<T> GetChannelFactory<T>()
        {
            Type key = typeof (T);
            ChannelFactory channelFactory;
            if (!ChannelFactories.TryGetValue(key, out channelFactory))
            {
                channelFactory = CreateChannelFactory<T>();
                ChannelFactories.Add(key, channelFactory);
            }

            return channelFactory as ChannelFactory<T>;
        }

        private ChannelFactory<T> GetChannelFactory<T>(ClientCredentials clientCredentials)
        {
            ChannelFactory<T> channelFactory = CreateChannelFactory<T>();
            channelFactory.SetClientCredentials(clientCredentials);
            return channelFactory;
        }

        private ChannelFactory<T> GetChannelFactory<T, TBinding>() where TBinding : Binding
        {
            ChannelFactory<T> channelFactory = CreateChannelFactory<T, TBinding>();
            return channelFactory;
        }
    }
}
