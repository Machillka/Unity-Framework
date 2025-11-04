namespace Framework.Utilities.ObjectPool
{
    public interface IObjectPool<T>
    {
        int InitializedSize { get; }
        int MaxSize { get; }

        T Get();
        T Create();
        void Release(T obj);
        void Destroy(T obj);
        void Clear();
    }
}