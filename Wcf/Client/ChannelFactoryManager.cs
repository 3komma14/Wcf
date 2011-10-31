using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
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
        /// Name of the client section in System.ServiceModel
        /// </summary>
        private const string ConfigSectionSystemServiceModelClient = "system.serviceModel/client";

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
            var channelEndpointElement = GetClientEndpoint<TContract>();
            if (channelEndpointElement == null)
            {
                throw new InvalidOperationException("Unable to get client endpoint");
            }

            return CreateChannelFactory<TContract>(channelEndpointElement.Name);
        }

        public static ChannelFactory<TContract> CreateChannelFactory<TContract, TBinding>() where TBinding : Binding
        {
            var channelEndpointElement = GetClientEndpoint<TContract, TBinding>();
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

        /// <summary>
        /// Get the client endpoint from the ServiceModel section in app.config/web.config
        /// Looks for a contract that matches the type T
        /// </summary>
        /// <typeparam name="TContract">The contract to look for</typeparam>
        /// <returns>A ChannelEndpointElement if found, else null</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no section is found in the config file
        /// </exception>
        private static ChannelEndpointElement GetClientEndpoint<TContract>()
        {
            ChannelEndpointElement channelEndpointElement = null;
            ClientSection clientSection = GetClientSection();

            List<ChannelEndpointElement> endpoints =
                clientSection.Endpoints.Cast<ChannelEndpointElement>().Where(
                    x => x.Contract == typeof (TContract).FullName).ToList();
            if (endpoints.Count() == 1)
            {
                channelEndpointElement = endpoints[0];
            }

            return channelEndpointElement;
        }

        /// <summary>
        /// Get the client endpoint from the ServiceModel section in app.config/web.config
        /// Looks for a contract that matches the type T
        /// </summary>
        /// <typeparam name="TContract">The contract to look for</typeparam>
        /// <typeparam name="TBinding">The binding to look for</typeparam>
        /// <returns>A ChannelEndpointElement if found, else null</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no section is found in the config file
        /// </exception>
        private static ChannelEndpointElement GetClientEndpoint<TContract, TBinding>() where TBinding : Binding
        {
            ChannelEndpointElement channelEndpointElement = null;
            ClientSection clientSection = GetClientSection();

            List<ChannelEndpointElement> endpoints = clientSection.Endpoints.Cast<ChannelEndpointElement>()
                .Where(x => x.Contract == typeof (TContract).FullName)
                .Where(x => IsBindingMatches<TBinding>(x.Binding))
                .ToList();
            if (endpoints.Count() == 1)
            {
                channelEndpointElement = endpoints[0];
            }

            return channelEndpointElement;
        }

        /// <summary>
        /// Gets the client section from the config file
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the section is not found
        /// </exception>
        /// <returns>The section</returns>
        private static ClientSection GetClientSection()
        {
            var clientSection = ConfigurationManager.GetSection(ConfigSectionSystemServiceModelClient) as ClientSection;
            if (clientSection == null)
            {
                throw new InvalidOperationException(string.Format("Section {0} not found.", ConfigSectionSystemServiceModelClient));
            }
            return clientSection;
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
            return string.Compare(typeof (TBinding).Name, bindingName, true) == 0;
        }
    }
}
