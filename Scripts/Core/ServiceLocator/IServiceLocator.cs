namespace Framework.Core.Service
{
    // 服务管理器接口，之后使用依赖注入来代替单例
    public interface IServiceLocator
    {
        void RegisterSingleton<T>(T instance) where T : class;
        void ReplaceSingleton<T>(T instance) where T : class;
        T Resolve<T>() where T : class;
        bool TryReslolve<T>(out T service) where T : class;
        bool Has<T>(T service) where T : class;
        void Clear();
    }
}