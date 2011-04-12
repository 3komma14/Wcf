using System;
using System.IdentityModel.Tokens;
using System.Runtime.Caching;
using System.Security;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Seterlund.Wcf.UnitTests
{

    [TestClass]
    public class ClientManagerTests
    {
        private MemoryCache TestCache;

        [TestInitialize]
        public void InitializeTest()
        {
            this.TestCache = new MemoryCache("ClientManagerTestCache");
        }

        [TestCleanup]
        public void CleanupTest()
        {
            this.TestCache.Dispose();
        }

        [TestMethod]
        public void CreateChannel_WhenCalled_ReturnsTheCorrectType()
        {
            // Arrange

            var channelFactory = new ChannelFactory<IService>(new BasicHttpBinding(), new EndpointAddress("http://localhost"));
            var channelFactoryManager = StubChannelFactoryManager(channelFactory);
            var securityTokenProvider = MockRepository.GenerateStub<SecurityTokenProvider>();
            var clientManager = new ClientManager(this.TestCache, channelFactoryManager, securityTokenProvider);

            // Act
            var client = clientManager.CreateChannel<IService>();
            var clientType = client.GetType();

            // Assert
            Assert.AreEqual(typeof(IService).Name, clientType.Name);
        }

        [TestMethod]
        public void CreateChannel_FederationBindingAndNoIdentity_Throws()
        {
            // Arrange
            var channelFactory = new ChannelFactory<IService>(new WS2007FederationHttpBinding(), new EndpointAddress("http://localhost"));
            var channelFactoryManager = StubChannelFactoryManager(channelFactory);
            var securityTokenProvider = MockRepository.GenerateStub<SecurityTokenProvider>();
            var clientManager = new ClientManager(this.TestCache, channelFactoryManager, securityTokenProvider);
            Thread.CurrentPrincipal = StubPrincipal<IPrincipal>(null);


            // Act
            // Assert
            ExceptionAssert.Throws<SecurityException>(
                () => clientManager.CreateChannel<IService>(),
                ex => Assert.AreEqual("Identity must be set", ex.Message));
        }

        [TestMethod]
        public void CreateChannel_FederationBinding_CallsIssueActAsToken()
        {
            // Arrange
            var channelFactory = new ChannelFactory<IService>(new WS2007FederationHttpBinding(), new EndpointAddress("http://localhost"));
            channelFactory.ConfigureChannelFactory();
            var channelFactoryManager = StubChannelFactoryManager(channelFactory);
            var securityTokenProvider = MockRepository.GenerateMock<SecurityTokenProvider>();
            var expectedToken = StubSecurityToken();
            securityTokenProvider.Stub(
                x => x.IssueToken(Arg<WS2007FederationHttpBinding>.Is.Anything, Arg<string>.Is.Anything, Arg<SecurityToken>.Is.Anything)).Return(expectedToken);
            var clientManager = new ClientManager(this.TestCache, channelFactoryManager, securityTokenProvider);
            Thread.CurrentPrincipal = StubPrincipal<IPrincipal>(StubIdentity<IIdentity>("SomeName"));


            // Act
            var actual = clientManager.CreateChannel<IService>();

            // Assert
            securityTokenProvider.AssertWasCalled(x => x.IssueToken(Arg<WS2007FederationHttpBinding>.Is.Anything, Arg<string>.Is.Anything, Arg<SecurityToken>.Is.Anything));
        }

        [TestMethod]
        public void GetSecurityToken_WhenTokenIsCached_ReturnsCachedToken()
        {
            // Arrange
            var channelFactory = new ChannelFactory<IService>(new WS2007FederationHttpBinding(), new EndpointAddress("http://localhost"));
            var channelFactoryManager = StubChannelFactoryManager(channelFactory);
            var securityTokenProvider = MockRepository.GenerateStub<SecurityTokenProvider>();
            var clientManager = new ClientManager_Accessor(this.TestCache, channelFactoryManager, securityTokenProvider);
            var expected = StubSecurityToken();
            this.TestCache.Add("SomeName_http://localhost/", expected, new DateTimeOffset(DateTime.Now.AddMinutes(1)));

            Thread.CurrentPrincipal = StubPrincipal<IPrincipal>(StubIdentity<IIdentity>("SomeName"));

            // Act
            var actual = clientManager.GetSecurityToken(channelFactory);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetSecurityToken_WhenTokenIsNotCached_ReturnsCreatesTokenAndCachesIt()
        {
            // Arrange
            var channelFactory = new ChannelFactory<IService>(new WS2007FederationHttpBinding(), new EndpointAddress("http://localhost"));
            var channelFactoryManager = StubChannelFactoryManager(channelFactory);
            var expected = StubSecurityToken();
            var securityTokenProvider = MockRepository.GenerateStub<SecurityTokenProvider>();
            securityTokenProvider.Stub(
                x =>
                x.IssueToken(Arg<WS2007FederationHttpBinding>.Is.Anything, Arg<string>.Is.Anything, Arg<SecurityToken>.Is.Anything)).Return(expected);
            var clientManager = MockRepository.GenerateStub<ClientManager_Accessor>(this.TestCache, channelFactoryManager, securityTokenProvider);

            Thread.CurrentPrincipal = StubPrincipal<IPrincipal>(StubIdentity<IIdentity>("SomeName"));

            // Act
            var actual = clientManager.GetSecurityToken(channelFactory);

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(this.TestCache.Contains(@"SomeName_http://localhost/"));
        }

        [TestMethod]
        public void CreateFederatedChannel_WhenCalled_DoesNotThrow()
        {
            // Arrange
            var channelFactory = new ChannelFactory<IFederatedService>(new WS2007FederationHttpBinding(), new EndpointAddress("http://localhost"));
            channelFactory.ConfigureChannelFactory();
            var channelFactoryManager = StubChannelFactoryManager(channelFactory);
            var expected = StubSecurityToken();
            var securityTokenProvider = MockRepository.GenerateStub<SecurityTokenProvider>();
            securityTokenProvider.Stub(
                x =>
                x.IssueToken(Arg<WS2007FederationHttpBinding>.Is.Anything, Arg<string>.Is.Anything, Arg<SecurityToken>.Is.Anything)).Return(expected);
            var clientManager = new ClientManager_Accessor(this.TestCache, channelFactoryManager, securityTokenProvider);

            Thread.CurrentPrincipal = StubPrincipal<IPrincipal>(StubIdentity<IIdentity>("SomeName"));

            // Act
            // Assert
            ExceptionAssert.DoesNotThrow(() => clientManager.CreateFederatedChannel<IFederatedService>(channelFactory));
        }


        private static IChannelFactoryManager StubChannelFactoryManager<T>(ChannelFactory<T> channelFactory)
        {
            var channelFactoryManager = MockRepository.GenerateStub<IChannelFactoryManager>();
            channelFactoryManager.Stub(x => x.GetFactory<T>()).Return(channelFactory);
            return channelFactoryManager;
        }

        private static SecurityToken StubSecurityToken()
        {
            var expected = MockRepository.GenerateStub<SecurityToken>();
            expected.Stub(x => x.ValidTo).Return(DateTime.Now.AddMinutes(1));
            return expected;
        }

        private static T StubPrincipal<T>(IIdentity identity) where T : class, IPrincipal
        {
            var principal = MockRepository.GenerateStub<T>();
            principal.Stub(x => x.Identity).Return(identity);
            return principal;
        }

        private static T StubIdentity<T>(string name) where T : class, IIdentity
        {
            var identity = MockRepository.GenerateStub<T>();
            identity.Stub(x => x.Name).Return(name);
            return identity;
        }
    }
}
