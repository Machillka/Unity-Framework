using System.Collections.Generic;
using Framework.Core.Service;
using UnityEngine;

namespace Framework.Utilities.ObjectPool
{
    [Service]
    public class PoolManager : IPoolManager
    {
        private readonly Dictionary<string, object> pools = new();

        public IObjectPool<T> CreatePool<T>(string key, IObjectPool<T> pool)
        {
            if (pools.ContainsKey(Hash(key)))
            {
                Debug.LogWarning($"Pool already exists for key:{key}");
                return pool;
            }

            pools.Add(Hash(key), pool);

            return pool;
        }

        public IObjectPool<T> GetPool<T>(string key = null) where T : class
        {
            if (pools.TryGetValue(Hash(key), out var p))
                return (IObjectPool<T>)p;
            return null;
        }

        // 自动匹配类型, 尝试释放对象到对应池子
        public bool TryRelease<T>(T item) where T : class
        {
            foreach (var p in pools.Values)
            {
                if (p is IObjectPool<T> pool)
                {
                    pool.Release(item);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否要做一层key的映射？使得出现不同类型的池子但是可以有相同名字
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Hash(string key)
        {
            return key;
        }
    }
}