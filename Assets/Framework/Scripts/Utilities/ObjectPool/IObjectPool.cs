namespace Framework.Utilities.ObjectPool
{
    public interface IObjectPool<T>
    {
        T Get();
        void Release(T obj);
        void Clear();
    }
}