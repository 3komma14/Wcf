using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using Microsoft.IdentityModel.Protocols.WSTrust;

namespace Seterlund.Wcf
{
    /// <summary>
    /// Manages creation of channel factories
    /// </summary>
    public class ChannelFactoryManager : IChannelFactoryManager
    {
        /// <summary>
        /// Name of the client section in System.ServiceModel
        /// </summary>
        private const string ConfigSectionSystemServiceModelClient = "system.serviceModel/client";

        /// <summary>
        /// Used for testing purposes only
        /// </summary>
        private static object _channelToReturnInTests;

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

        /// <summary>
        /// Creates a channel factory for the given interface/type.
        /// It first reads the system.serviceModel/client section to find a single endpoint matching the contract.
        /// If none or multiple entries found it uses the ChannelFactoryManager section to select the correct one.
        /// </summary>
        /// <typeparam name="T">
        /// Interface or type
        /// </typeparam>
        /// <returns>
        /// A ChannelFactory
        /// </returns>
        public static ChannelFactory<T> CreateChannelFactory<T>()
        {
            ChannelEndpointElement channelEndpointElement;
            if (!TryGetClientEndpoint<T>(out channelEndpointElement))
            {
                throw new InvalidOperationException("Unable to get client endpoint");
            }
            
            var channelFactory = new ChannelFactory<T>(channelEndpointElement.Name);
            if (channelFactory.IsFederated())
            {
                channelFactory.ConfigureChannelFactory();
            }

            return channelFactory;
        }

        public static ChannelFactory<TContract> CreateChannelFactory<TContract, TBinding>() where TBinding : Binding
        {
            ChannelEndpointElement channelEndpointElement;
            if (!TryGetClientEndpoint<TContract, TBinding>(out channelEndpointElement))
            {
                throw new InvalidOperationException("Unable to get client endpoint");
            }

            var channelFactory = new ChannelFactory<TContract>(channelEndpointElement.Name);
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
            var key = typeof(T);
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
            var channelFactory = CreateChannelFactory<T>();
            channelFactory.SetClientCredentials(clientCredentials);
            return channelFactory;
        }

        private ChannelFactory<T> GetChannelFactory<T, TBinding>() where TBinding : Binding
        {
            var channelFactory = CreateChannelFactory<T, TBinding>();
            return channelFactory;
        }

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


        /// <summary>
        /// Returns the channelfactory. If a mock channel is set then this is returned
        /// </summary>
        /// <param name="endpointConfigurationName">
        /// The endpoint configuration name.
        /// </param>
        /// <typeparam name="T">
        /// Type of the channelfactory
        /// </typeparam>
        /// <returns>
        /// A channelfactory
        /// </returns>
        protected static ChannelFactory<T> CreateChannelFactory<T>(string endpointConfigurationName)
        {
            if (_channelToReturnInTests != null)
            {
                return (ChannelFactory<T>)_channelToReturnInTests;
            }

            return new ChannelFactory<T>(endpointConfigurationName);
        }

        /// <summary>
        /// Sets a channelfactory to return in tests
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <typeparam name="T">
        /// The type of the channel
        /// </typeparam>
        protected static void SetChannelToReturn<T>(ChannelFactory<T> channel)
        {
            _channelToReturnInTests = channel;
        }

        /// <summary>
        /// Try to get the client endpoint from the ServiceModel section in app.config/web.config
        /// Looks for a contract that matches the type T
        /// </summary>
        /// <param name="channelEndpointElement">
        /// The channel endpoint element found
        /// </param>
        /// <typeparam name="T">
        /// The contract to look for
        /// </typeparam>
        /// <returns>
        /// true if one (and only one) contract is found, else false
        /// </returns>
        /// <exception cref="ApplicationException">
        /// </exception>
        private static bool TryGetClientEndpoint<T>(out ChannelEndpointElement channelEndpointElement)
        {
            channelEndpointElement = null;
            var clientSection = ConfigurationManager.GetSection(ConfigSectionSystemServiceModelClient) as ClientSection;
            if (clientSection == null)
            {
                throw new InvalidOperationException(string.Format("Section {0} not found.", ConfigSectionSystemServiceModelClient));
            }

            var endpoints = clientSection.Endpoints.Cast<ChannelEndpointElement>().Where(x => x.Contract == typeof(T).FullName).ToList();
            if (endpoints.Count() == 1)
            {
                channelEndpointElement = endpoints[0];
            }

            return channelEndpointElement == null ? false : true;
        }

        /// <summary>
        /// Try to get the client endpoint from the ServiceModel section in app.config/web.config
        /// Looks for a contract that matches the type TContract and binding TBinding
        /// </summary>
        /// <param name="channelEndpointElement">
        /// The channel endpoint element found
        /// </param>
        /// <typeparam name="TContract">
        /// The contract to look for
        /// </typeparam>
        /// <typeparam name="TBinding">
        /// The binding to look for
        /// </typeparam>
        /// <returns>
        /// true if one (and only one) contract is found, else false
        /// </returns>
        /// <exception cref="ApplicationException">
        /// </exception>
        private static bool TryGetClientEndpoint<TContract, TBinding>(out ChannelEndpointElement channelEndpointElement)
            where TBinding : Binding
        {
            channelEndpointElement = null;
            var clientSection = ConfigurationManager.GetSection(ConfigSectionSystemServiceModelClient) as ClientSection;
            if (clientSection == null)
            {
                throw new InvalidOperationException(string.Format("Section {0} not found.", ConfigSectionSystemServiceModelClient));
            }

            var endpoints = clientSection.Endpoints.Cast<ChannelEndpointElement>()
                .Where(x => x.Contract == typeof(TContract).FullName)
                .Where(x => IsBindingMatches<TBinding>(x.Binding))
                .ToList();
            if (endpoints.Count() == 1)
            {
                channelEndpointElement = endpoints[0];
            }

            return channelEndpointElement == null ? false : true;
        }

        /// <summary>
        /// Is the binding a match against the type
        /// </summary>
        /// <param name="bindingName">
        /// The binding name.
        /// </param>
        /// <typeparam name="TBinding">
        /// The binding tyep to check against
        /// </typeparam>
        /// <returns>
        /// True if it matches, else false
        /// </returns>
        private static bool IsBindingMatches<TBinding>(string bindingName)
        {
            return string.Compare(typeof(TBinding).Name, bindingName, true) == 0;
        }

    }
}
