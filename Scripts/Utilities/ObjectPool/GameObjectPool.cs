using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

namespace Framework.Utilities.ObjectPool
{
    public class GameobjectPool : IObjectPool<GameObject>
    {
        private readonly ObjectPool<GameObject> _pool;
        private readonly Transform _parent;
        private readonly GameObject _prefab;
        private readonly int _initializedSize;
        private readonly int _maxSize;

        public int InitializedSize => _initializedSize;

        public int MaxSize => _maxSize;

        /// <summary>
        /// 仅提供预制体和父节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prefab"></param>
        /// <param name="initializedSize"></param>
        /// <param name="maxSize"></param>
        public GameobjectPool(Transform parent, GameObject prefab, int initializedSize, int maxSize)
        {
            _parent = parent;
            _prefab = prefab;
            _initializedSize = initializedSize;
            _maxSize = maxSize;

            _pool = new ObjectPool<GameObject>(
                createFunc: () => Object.Instantiate(_prefab, _parent),
                actionOnGet: go => { if (go != null) go.SetActive(true); },
                actionOnRelease: go => { if (go != null) { go.SetActive(false); go.transform.SetParent(_parent); } },
                actionOnDestroy: obj => { if (obj != null) Object.Destroy(obj); },
                collectionCheck: false,
                defaultCapacity: initializedSize,
                maxSize: maxSize
            );
        }

        public GameObject Create()
        {
            return _pool.Get();
        }

        public void Clear()
        {
            _pool.Clear();
        }

        public void Destroy(GameObject obj)
        {
            if (obj != null)
                Object.Destroy(obj);
        }

        public GameObject Get()
        {
            return _pool.Get();
        }

        // NOTE: 实现一个回调函数, 实现 After Get

        public void Release(GameObject obj)
        {
            _pool.Release(obj);
        }
    }
}