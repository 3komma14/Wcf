using System;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Tokens;

namespace Seterlund.Wcf
{
    /// <summary>
    /// Class for geting security tokens
    /// </summary>
    public class SecurityTokenProvider
    {
        /// <summary>
        /// Issues security token
        /// </summary>
        /// <param name="issuerBinding">
        /// The issuer binding.
        /// </param>
        /// <param name="issuerEndpointAddress">
        /// The issuer endpoint address.
        /// </param>
        /// <param name="serviceAddress">
        /// The service address.
        /// </param>
        /// <param name="actAsToken">
        /// The act as token.
        /// </param>
        /// <returns>
        /// The security token
        /// </returns>
        public virtual SecurityToken IssueToken(WS2007HttpBinding issuerBinding, EndpointAddress issuerEndpointAddress, string serviceAddress, SecurityToken actAsToken)
        {
            var channel = this.CreateChannel(issuerBinding, issuerEndpointAddress);
            return this.IssueToken(channel, serviceAddress, actAsToken);
        }

        /// <summary>
        /// Create a trust channel
        /// </summary>
        /// <param name="issuerBinding">
        /// The issuer binding.
        /// </param>
        /// <param name="issuerEndpointAddress">
        /// The issuer endpoint address.
        /// </param>
        /// <returns>
        /// the trust channel
        /// </returns>
        public virtual IWSTrustChannelContract CreateChannel(WS2007HttpBinding issuerBinding, EndpointAddress issuerEndpointAddress)
        {
            var trustChannelFactory = new WSTrustChannelFactory(issuerBinding, issuerEndpointAddress);
            return trustChannelFactory.CreateChannel() as WSTrustChannel;
        }

        /// <summary>
        /// Issues security token
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="serviceAddress">
        /// The service address.
        /// </param>
        /// <param name="actAsToken">
        /// The act as token.
        /// </param>
        /// <param name="onBehalfOf">
        /// the on behalf of token
        /// </param>
        /// <returns>
        /// the security token
        /// </returns>
        public virtual SecurityToken IssueToken(IWSTrustChannelContract channel, string serviceAddress, SecurityToken actAsToken = null, SecurityToken onBehalfOf = null)
        {
            var rst = new RequestSecurityToken(RequestTypes.Issue)
            {
                AppliesTo = new EndpointAddress(serviceAddress)
            };
            if (actAsToken != null)
            {
                rst.ActAs = new SecurityTokenElement(actAsToken);
            }

            if (onBehalfOf != null)
            {
                rst.OnBehalfOf = new SecurityTokenElement(onBehalfOf);
            }

            RequestSecurityTokenResponse rstr;
            return channel.Issue(rst, out rstr);
        }

        /// <summary>
        /// Issues security token
        /// </summary>
        /// <param name="binding">
        /// The binding.
        /// </param>
        /// <param name="serviceAddress">
        /// The service address.
        /// </param>
        /// <param name="actAsToken">
        /// The act as token.
        /// </param>
        /// <returns>
        /// The security token
        /// </returns>
        /// <exception cref="ApplicationException">
        /// </exception>
        public virtual SecurityToken IssueToken(WS2007FederationHttpBinding binding, string serviceAddress, SecurityToken actAsToken)
        {
            var issuerEndpointAddress = binding.Security.Message.IssuerAddress;
            var issuerBinding = binding.Security.Message.IssuerBinding as WS2007HttpBinding;
            if (issuerBinding == null)
            {
                throw new ApplicationException("Unable to get WS2007HttpBinding");
            }

            var token = this.IssueToken(issuerBinding, issuerEndpointAddress, serviceAddress, actAsToken);
            return token;
        }
    }
}
