using System.Runtime.Caching;
using System.ServiceModel;
using System.ServiceModel.Description;
using NUnit.Framework;
using Rhino.Mocks;
using Seterlund.Wcf.Client;

namespace Seterlund.Wcf.UnitTests
{

    [TestFixture]
    public class ChannelFactoryManagerTests
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
            var manager = new ChannelFactoryManager();

            // Act
            var factory = manager.GetFactory<IService>();

            // Assert
            Assert.IsNotNull(factory);
        }

        [Test]
        public void CreateChannel_WhenCalledWithBinding_ReturnsTheCorrectType()
        {
            // Arrange
            var manager = new ChannelFactoryManager();

            // Act
            var factory = manager.GetFactory<IService, BasicHttpBinding>();

            // Assert
            Assert.IsNotNull(factory);
        }

        [Test]
        public void CreateChannel_WhenCalledWithClientCredentials_ReturnsTheCorrectType()
        {
            // Arrange
            var manager = new ChannelFactoryManager();

            // Act
            ClientCredentials clientCredentials = MockRepository.GenerateStub<ClientCredentials>();
            var factory = manager.GetFactory<IService>(clientCredentials);

            // Assert
            Assert.IsNotNull(factory);
        }

    }
}
