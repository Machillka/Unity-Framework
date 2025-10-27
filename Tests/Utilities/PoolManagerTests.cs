using UnityEngine;
using NUnit.Framework;
using Framework.Core.Service;
using Framework.Utilities.ObjectPool;

namespace Tests
{
    public class PoolManagerTests
    {
        public GameObject prefab;
        public GameObject parent;
        public ServiceLocator serviceManager;

        [SetUp]
        public void SetUp()
        {
            prefab = new GameObject("TestPrefab");
            serviceManager = new();
            parent = new GameObject("TestParent");
            serviceManager.RegisterSingleton(serviceManager);
        }

        [Test]
        public void CreateAndGetPool_Works()
        {
            var poolManager = serviceManager.Resolve<PoolManager>();
            var pool = new GameobjectPool(parent.transform, prefab, 5, 20);
            var returnedPool = poolManager.CreatePool("TestPool", pool);
            Assert.AreEqual(pool, returnedPool);
            var fetchPool = poolManager.GetPool<GameObject>("TestPool");
            Assert.AreEqual(pool, fetchPool);
        }

        [Test]
        public void PoolOperation_Works()
        {
            var poolManager = serviceManager.Resolve<PoolManager>();
            var pool = new GameobjectPool(parent.transform, prefab, 5, 20);
            poolManager.CreatePool("Op", pool);
            var obj = pool.Get();
            Assert.IsNotNull(obj);
            pool.Release(obj);

            var tryReleaseOut = poolManager.TryRelease(obj);
            Assert.IsTrue(tryReleaseOut);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(prefab);
        }
    }
}