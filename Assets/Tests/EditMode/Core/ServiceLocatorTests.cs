using Framework.Core.Service;
using NUnit.Framework;

namespace Tests
{
    public class ServiceLocatorTests
    {
        public ServiceLocator serviceManager = new();

        [SetUp]
        public void SetUp()
        {

        }

        [TearDown]
        public void TearDown()
        {
            serviceManager.Clear();
        }

        [Test]
        public void Register_Resolve_Method()
        {
            serviceManager.RegisterSingleton(new Case01());
            serviceManager.RegisterSingleton<Case03>();

            Assert.AreEqual(serviceManager.Resolve<Case01>().Logger(), "Case01");
            Assert.AreEqual(serviceManager.Resolve<Case02>().Logger(), "Case02");
            Assert.AreEqual(serviceManager.Resolve<Case03>().Logger(), "Case03");
        }

        // 直接注册
        public class Case01
        {
            public string Logger() => nameof(Case01);
        }

        // 自动注册
        [Service]
        public class Case02
        {
            public string Logger() => nameof(Case02);
        }

        // 使用接口
        public interface ICase03
        {
            public string Logger();
        }

        public class Case03 : ICase03
        {
            public string Logger() => nameof(Case03);
        }
    }
}

