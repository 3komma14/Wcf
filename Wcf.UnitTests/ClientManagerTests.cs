using System;
using System.IdentityModel.Tokens;
using System.Runtime.Caching;
using System.Security;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using Microsoft.IdentityModel.Protocols.WSTrust;
using NUnit.Framework;
using Rhino.Mocks;

namespace Seterlund.Wcf.UnitTests
{

    [TestFixture]
    public class ClientManagerTests
    {
        private MemoryCache TestCache;

        #region ----- Fixture setup -----

        /// <summary>
        /// Called once before first test is executed
        /// </summary>
        [TestFixtureSetUp]
        public void Init()
        {
            // Init tests
        }

        /// <summary>
        /// Called once after last test is executed
        /// </summary>
        [TestFixtureTearDown]
        public void Cleanup()
        {
            // Cleanup tests
        }

        #endregion

        #region ------ Test setup -----

        /// <summary>
        /// Called before each test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.TestCache = new MemoryCache("ClientManagerTestCache");
        }

        /// <summary>
        /// Called before each test
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.TestCache.Dispose();
        }

        #endregion

        [Test]
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

        [Test]
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
            var ex = Assert.Throws<SecurityException>(() => clientManager.CreateChannel<IService>());
            Assert.AreEqual("Identity must be set", ex.Message);
        }

        [Test]
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

        [Test]
        public void GetSecurityToken_WhenTokenIsCached_ReturnsCachedToken()
        {
            // Arrange
            var channelFactory = new ChannelFactory<IService>(new WS2007FederationHttpBinding(), new EndpointAddress("http://localhost"));
            channelFactory.ConfigureChannelFactory();
            var channelFactoryManager = StubChannelFactoryManager(channelFactory);
            var securityTokenProvider = MockRepository.GenerateStub<SecurityTokenProvider>();
            var securityToken = StubSecurityToken();
            this.TestCache.Add("SomeName_http://localhost/", securityToken, new DateTimeOffset(DateTime.Now.AddMinutes(1)));
            var clientManager = new TestableClientManager(this.TestCache, channelFactoryManager, securityTokenProvider);

            Thread.CurrentPrincipal = StubPrincipal<IPrincipal>(StubIdentity<IIdentity>("SomeName"));

            // Act
            var actual = clientManager.GetSecurityToken(channelFactory);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(securityToken, actual);
        }

        [Test]
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
            var clientManager = new TestableClientManager(this.TestCache, channelFactoryManager, securityTokenProvider);

            Thread.CurrentPrincipal = StubPrincipal<IPrincipal>(StubIdentity<IIdentity>("SomeName"));

            // Act
            var actual = clientManager.GetSecurityToken(channelFactory);

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(this.TestCache.Contains(@"SomeName_http://localhost/"));
        }

        [Test]
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
            var clientManager = new TestableClientManager(this.TestCache, channelFactoryManager, securityTokenProvider);

            Thread.CurrentPrincipal = StubPrincipal<IPrincipal>(StubIdentity<IIdentity>("SomeName"));

            // Act
            // Assert
            Assert.DoesNotThrow(() => clientManager.CreateFederatedChannel<IFederatedService>(channelFactory));
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

        private class TestableClientManager : ClientManager
        {
            public TestableClientManager(ObjectCache cache, IChannelFactoryManager channelFactoryManager, SecurityTokenProvider securityTokenProvider) : base(cache, channelFactoryManager, securityTokenProvider)
            {
            }

            public new SecurityToken GetSecurityToken<T>(ChannelFactory<T> channelFactory)
            {
                return base.GetSecurityToken<T>(channelFactory);
            }

            public new T CreateFederatedChannel<T>(ChannelFactory<T> channelFactory)
            {
                return base.CreateFederatedChannel<T>(channelFactory);
            }
        }

    }
}
