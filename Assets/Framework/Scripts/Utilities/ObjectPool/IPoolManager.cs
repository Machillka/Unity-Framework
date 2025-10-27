using UnityEngine;

namespace Framework.Utilities.ObjectPool
{
    /// <summary>
    /// 工厂方法, 用于管理所有的对象池
    /// </summary>
    public interface IPoolManager
    {
        IObjectPool<T> GetPool<T>(string key = null) where T : class;
        IObjectPool<T> CreatePool<T>(string key, IObjectPool<T> pool);
        bool TryRelease<T>(T item) where T : class;
    }
}