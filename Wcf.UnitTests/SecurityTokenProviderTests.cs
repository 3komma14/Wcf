using System;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using Microsoft.IdentityModel.Protocols.WSTrust;
using NUnit.Framework;
using Rhino.Mocks;

namespace Seterlund.Wcf.UnitTests
{
    [TestFixture]
    public class SecurityTokenProviderTests
    {
        [Test]
        public void IssueToken_CalledOnContract_CallsChannelIssue()
        {
            // Arrange
            var securityTokenProvider = new SecurityTokenProvider();
            var channelContract = MockRepository.GenerateMock<IWSTrustChannelContract>();
            var actAsToken = MockRepository.GenerateStub<SecurityToken>();

            // Act
            securityTokenProvider.IssueToken(channelContract, "http://localhost/service", actAsToken);

            // Assert
            channelContract.AssertWasCalled(x => x.Issue(Arg<RequestSecurityToken>.Is.Anything, out Arg<RequestSecurityTokenResponse>.Out(null).Dummy));
        }

        [Test]
        public void IssueToken_WS2007HttpBindingIsNotSet_Throws()
        {
            // Arrange
            var securityTokenProvider = new SecurityTokenProvider();
            var binding = MockRepository.GenerateStub<WS2007FederationHttpBinding>();
            var actAsToken = MockRepository.GenerateStub<SecurityToken>();

            // Act
            // Assert
            var ex = Assert.Throws<ApplicationException>(() => securityTokenProvider.IssueToken(binding, "http://localhost/service", actAsToken));
            Assert.AreEqual("Unable to get WS2007HttpBinding", ex.Message);
        }

        [Test]
        public void IssueToken_CalledOnFederationBinding_CallsIssueToken()
        {
            // Arrange
            var returnToken = MockRepository.GenerateStub<SecurityToken>();
            var mockRepository = new MockRepository();
            var securityTokenProvider = mockRepository.PartialMock<SecurityTokenProvider>();
            securityTokenProvider.Stub(x => x.IssueToken(Arg<IWSTrustChannelContract>.Is.Anything, Arg<string>.Is.Anything, Arg<SecurityToken>.Is.Anything, Arg<SecurityToken>.Is.Anything)).Return(returnToken);
            securityTokenProvider.Replay();

            var binding = MockRepository.GenerateStub<WS2007FederationHttpBinding>();
            var messageSecurityOverHttp = new FederatedMessageSecurityOverHttp
            {
                IssuerAddress = new EndpointAddress("http://localhost/issuer"),
                IssuerBinding = new WS2007HttpBinding()
            };
            binding.Security.Message = messageSecurityOverHttp;
            var actAsToken = MockRepository.GenerateStub<SecurityToken>();

            // Act
            var actual = securityTokenProvider.IssueToken(binding, "http://localhost/service", actAsToken);

            // Assert
            securityTokenProvider.AssertWasCalled(
                x => x.IssueToken(Arg<IWSTrustChannelContract>.Is.Anything, Arg<string>.Is.Anything, Arg<SecurityToken>.Is.Anything, Arg<SecurityToken>.Is.Anything));
            Assert.AreEqual(returnToken, actual);
        }

        [Test]
        public void CreateChannel_WhenCalled_ReturnsChannel()
        {
            // Arrange
            var securityTokenProvider = new SecurityTokenProvider();
            var issuerBinding = new WS2007HttpBinding();
            var issuerEndpointAddress = new EndpointAddress("http://localhost/issuer");

            // Act
            var actual = securityTokenProvider.CreateChannel(issuerBinding, issuerEndpointAddress);

            // Assert
            Assert.IsNotNull(actual);
        }
    }
}
