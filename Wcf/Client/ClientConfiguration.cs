using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace Seterlund.Wcf.Client
{
    public class ClientConfiguration
    {
        /// <summary>
        /// Name of the client section in System.ServiceModel
        /// </summary>
        private const string ConfigSectionSystemServiceModelClient = "system.serviceModel/client";

        /// <summary>
        /// Gets the client section from the config file
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the section is not found
        /// </exception>
        /// <returns>The section</returns>
        public static ClientSection GetClientSection()
        {
            var clientSection = ConfigurationManager.GetSection(ConfigSectionSystemServiceModelClient) as ClientSection;
            if (clientSection == null)
            {
                throw new InvalidOperationException(string.Format("Section {0} not found.", ConfigSectionSystemServiceModelClient));
            }
            return clientSection;
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
        public static ChannelEndpointElement GetClientEndpoint<TContract>()
        {
            ChannelEndpointElement channelEndpointElement = null;
            ClientSection clientSection = GetClientSection();

            List<ChannelEndpointElement> endpoints =
                clientSection.Endpoints.Cast<ChannelEndpointElement>().Where(
                    x => x.Contract == typeof(TContract).FullName).ToList();
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
        public static ChannelEndpointElement GetClientEndpoint<TContract, TBinding>() where TBinding : Binding
        {
            ChannelEndpointElement channelEndpointElement = null;
            ClientSection clientSection = GetClientSection();

            List<ChannelEndpointElement> endpoints = clientSection.Endpoints.Cast<ChannelEndpointElement>()
                .Where(x => x.Contract == typeof(TContract).FullName)
                .Where(x => IsBindingMatches<TBinding>(x.Binding))
                .ToList();
            if (endpoints.Count() == 1)
            {
                channelEndpointElement = endpoints[0];
            }

            return channelEndpointElement;
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
