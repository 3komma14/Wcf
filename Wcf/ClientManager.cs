using System;
using System.IdentityModel.Tokens;
using System.Runtime.Caching;
using System.Security;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSTrust;

namespace Seterlund.Wcf
{
    /// <summary>
    /// Class for creating service clients
    /// </summary>
    public class ClientManager
    {
        /// <summary>
        /// Cache to hold security tokens
        /// </summary>
        private readonly ObjectCache _cache;

        /// <summary>
        /// Channel factory manager helps with the creation of channel factories
        /// </summary>
        private readonly IChannelFactoryManager _channelFactoryManager;

        /// <summary>
        /// Helper class for federation
        /// </summary>
        private readonly SecurityTokenProvider _securityTokenProvider;

        /// <summary>
        /// Object used for locking purposes
        /// </summary>
        private readonly object _cacheLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientManager"/> class.
        /// </summary>
        /// <param name="cache">
        /// The cache.
        /// </param>
        /// <param name="channelFactoryManager">
        /// The channel factory manager.
        /// </param>
        public ClientManager(ObjectCache cache, IChannelFactoryManager channelFactoryManager, SecurityTokenProvider securityTokenProvider)
        {
            this._cache = cache;
            this._channelFactoryManager = channelFactoryManager;
            this._securityTokenProvider = securityTokenProvider;
        }

        /// <summary>
        /// Create a new client.
        /// </summary>
        /// <typeparam name="T">
        /// The service interface
        /// </typeparam>
        /// <returns>
        /// A new wcf client
        /// </returns>
        public static T Create<T>() where T : class
        {
            return (new ClientManager(MemoryCache.Default, new ChannelFactoryManager(), new SecurityTokenProvider())).CreateChannel<T>();
        }

        /// <summary>
        /// Create a new client and sets the credentials
        /// </summary>
        /// <param name="clientCredentials">
        /// The client credentials.
        /// </param>
        /// <typeparam name="T">
        /// The service contract
        /// </typeparam>
        /// <returns>
        /// a new wcf client
        /// </returns>
        public static T Create<T>(ClientCredentials clientCredentials) where T : class
        {
            return (new ClientManager(MemoryCache.Default, new ChannelFactoryManager(), new SecurityTokenProvider())).CreateChannel<T>(clientCredentials);
        }

        public static T Create<T, TBinding>()
            where T : class
            where TBinding : Binding
        {
            return (new ClientManager(MemoryCache.Default, new ChannelFactoryManager(), new SecurityTokenProvider())).CreateChannel<T, TBinding>();
        }


        /// <summary>
        /// Gets the token cache key
        /// </summary>
        /// <typeparam name="T">
        /// The service contract
        /// </typeparam>
        /// <param name="channelFactory">
        /// The channel Factory.
        /// </param>
        /// <param name="identity">
        /// The identity of the calling user
        /// </param>
        /// <returns>
        /// A string with the key
        /// </returns>
        protected virtual string GetTokenCacheKey<T>(ChannelFactory<T> channelFactory, IIdentity identity)
        {
            return string.Format("{0}_{1}", identity.Name, channelFactory.Endpoint.Address.Uri.AbsoluteUri);
        }

        /// <summary>
        /// Creates a new token by contacting the STS
        /// </summary>
        /// <param name="channelFactory">
        ///   The channel factory.
        /// </param>
        /// <param name="identity">
        ///   The identity of the calling user
        /// </param>
        /// <typeparam name="T">
        /// The service contract
        /// </typeparam>
        /// <returns>
        /// A new security token
        /// </returns>
        protected virtual SecurityToken CreateNewSecurityToken<T>(ChannelFactory<T> channelFactory, IIdentity identity)
        {
            var serviceAddress = channelFactory.Endpoint.Address.Uri.AbsoluteUri;
            var serviceBinding = channelFactory.Endpoint.Binding as WS2007FederationHttpBinding;
            var claimsIdentity = identity as IClaimsIdentity;
            var bootstrapToken = claimsIdentity != null ? claimsIdentity.BootstrapToken : null;
            return this._securityTokenProvider.IssueToken(serviceBinding, serviceAddress, bootstrapToken);
        }

        /// <summary>
        /// Gets the claim identity from the thread
        /// </summary>
        /// <returns>
        /// A claim identity
        /// </returns>
        /// <exception cref="SecurityException">
        /// If IClaimsPrincipal is not found
        /// </exception>
        /// <exception cref="SecurityException">
        /// If IClaimsIdentity is not found
        /// </exception>
        private static IIdentity GetIdentity()
        {
            var identity = Thread.CurrentPrincipal.Identity;
            if (identity == null)
            {
                throw new SecurityException("Identity must be set");
            }

            return identity;
        }

        /// <summary>
        /// Creates a federated channel.
        /// Security tokens are cached, to eliminate the need for contacting the STS every time.
        /// </summary>
        /// <param name="channelFactory">
        /// The channel factory.
        /// </param>
        /// <typeparam name="T">
        /// The service interface
        /// </typeparam>
        /// <returns>
        /// A new federated channel
        /// </returns>
        private T CreateFederatedChannel<T>(ChannelFactory<T> channelFactory)
        {
            var token = GetSecurityToken(channelFactory);
            return channelFactory.CreateChannelWithIssuedToken(token);
        }

        /// <summary>
        /// Gets the token. If token not found in cache, one will be created
        /// </summary>
        /// <typeparam name="T">
        /// The service contract
        /// </typeparam>
        /// <param name="channelFactory">
        /// The channel Factory.
        /// </param>
        /// <returns>
        /// The security token
        /// </returns>
        private SecurityToken GetSecurityToken<T>(ChannelFactory<T> channelFactory)
        {
            var identity = GetIdentity();
            var cacheKey = GetTokenCacheKey(channelFactory, identity);
            SecurityToken token;
            if (this._cache.Contains(cacheKey))
            {
                token = this._cache[cacheKey] as SecurityToken;
            }
            else
            {
                token = CreateNewSecurityToken(channelFactory, identity);
                lock (this._cacheLock)
                {
                    if (!this._cache.Contains(cacheKey))
                    {
                        this._cache.Add(cacheKey, token, new DateTimeOffset(token.ValidTo));
                    }
                }
            }

            return token;
        }

        /// <summary>
        /// Creates a new service client for the specified interface
        /// </summary>
        /// <typeparam name="T">
        /// The service contract/interface
        /// </typeparam>
        /// <returns>
        /// A new disposable service client
        /// </returns>
        public T CreateChannel<T>() where T : class
        {
            var channelFactory = this._channelFactoryManager.GetFactory<T>();
            return CreateChannel(channelFactory);
        }

        /// <summary>
        /// Creates a new service client for the specified interface
        /// </summary>
        /// <param name="clientCredentials">
        /// The client Credentials.
        /// </param>
        /// <typeparam name="T">
        /// The service contract/interface
        /// </typeparam>
        /// <returns>
        /// A new disposable service client
        /// </returns>
        public T CreateChannel<T>(ClientCredentials clientCredentials) where T : class
        {
            var channelFactory = this._channelFactoryManager.GetFactory<T>(clientCredentials);
            return CreateChannel(channelFactory);
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <typeparam name="TBinding">
        /// </typeparam>
        /// <returns>
        /// </returns>
        public T CreateChannel<T, TBinding>()
            where T : class
            where TBinding : Binding
        {
            var channelFactory = this._channelFactoryManager.GetFactory<T, TBinding>();
            return CreateChannel(channelFactory);
        }

        /// <summary>
        /// CReate a new service client for specified interces
        /// </summary>
        /// <param name="channelFactory">
        /// The channel factory to use
        /// </param>
        /// <typeparam name="T">
        /// The service contract
        /// </typeparam>
        /// <returns>
        /// A new disposable service client
        /// </returns>
        private T CreateChannel<T>(ChannelFactory<T> channelFactory) where T : class
        {
            T channel;
            if (channelFactory.IsFederated())
            {
                channel = CreateFederatedChannel(channelFactory);
            }
            else
            {
                channel = channelFactory.CreateChannel();
            }

            return channel;
        }
    }
}
