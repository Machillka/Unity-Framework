using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
namespace Framework.Core.Service
{
    // 提供Servcice标记
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ServiceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class ServiceConstructorAttribute : Attribute { }

    public class ServiceLocator : IServiceLocator
    {
        // 缓存服务
        private readonly Dictionary<Type, object> _services = new();
        // 缓存类型
        private readonly Dictionary<Type, Type> _typeMap = new();
        private object _scanLock = new();
        private bool _scanned = false;

        // 构造时直接扫一遍　
        public ServiceLocator()
        {
            LazyScan();
        }

        /// <summary>
        /// 显示注册，通过依赖注入
        /// </summary>
        /// <typeparam name="TService">注册的类型</typeparam>
        /// <param name="instance">提供一个实例</param>
        /// <exception cref="ArgumentNullException">当实例为空的时候</exception>
        public void RegisterSingleton<TService>(TService instance) where TService : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            var t = typeof(TService);
            _services[t] = instance;
            _typeMap[t] = t;
        }

        // 隐式注册，仅提供类型，通过默认构造方式提供实例构造
        public void RegisterSingleton<TService>() where TService : class
        {
            var instance = (TService)CreateInstance(typeof(TService));
            RegisterSingleton(instance);
        }

        public void RegisterSingleton(Type t)
        {
            _services[t] = CreateInstance(t);
            _typeMap[t] = t;
        }

        public void ReplaceSingleton<TService>(TService instance) where TService : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _services[typeof(TService)] = instance;
        }

        public TService Resolve<TService>() where TService : class
        {
            return (TService)Resolve(typeof(TService));
        }

        public bool TryReslolve<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        public object Resolve(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            // 已注册实例优先
            if (_services.TryGetValue(serviceType, out var inst))
                return inst;

            // 延迟尝试通过 [Service] 标记发现实现（只在第一次需要时做简单扫描）
            LazyScan();

            // 查找映射的实现类型
            if (!_typeMap.TryGetValue(serviceType, out var implType))
            {
                // 如果请求的是具体类型，允许直接创建
                if (!serviceType.IsAbstract && !serviceType.IsInterface)
                    implType = serviceType;
                else
                {
                    // 找第一个可分配的实现
                    implType = _typeMap.Where(kv => serviceType.IsAssignableFrom(kv.Value)).Select(kv => kv.Value).FirstOrDefault();
                    if (implType == null) throw new InvalidOperationException($"No registered instance or discoverable implementation for {serviceType.FullName}");
                }

            }
            // 创建实例并缓存
            var instance = CreateInstance(implType);
            _services[implType] = instance;

            return instance;
        }

        public bool Has<T>(T service) where T : class => _services.ContainsKey(typeof(T));

        public void Clear()
        {
            _services.Clear();
            _typeMap.Clear();
        }

        /// <summary>
        /// 扫描所有 [Service]
        /// 保证只扫描一次
        /// </summary>
        public void LazyScan()
        {
            if (_scanned)
                return;
            lock (_scanLock)
            {
                if (_scanned)
                    return;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in assemblies)
                {
                    Type[] types;
                    try { types = asm.GetTypes(); } catch { continue; }
                    foreach (var t in types)
                    {
                        if (t.GetCustomAttribute<ServiceAttribute>() == null) continue;

                        if (t.IsInterface)
                        {
                            var impl = FindConcreteImplementationForInterface(t, assemblies);
                            if (impl != null && !_typeMap.ContainsKey(t))
                            {
                                RegisterSingleton(impl);
                            }
                        }
                        else if (t.IsClass || t.IsAbstract)
                        {
                            if (!Has(t))
                            {
                                RegisterSingleton(t);
                            }

                            foreach (var iface in t.GetInterfaces())
                            {
                                if (!Has(t))
                                {
                                    RegisterSingleton(t);
                                }
                            }
                        }
                    }


                }
                _scanned = true;
            }
        }

        Type FindConcreteImplementationForInterface(Type iface, Assembly[] assemblies)
        {
            foreach (var asm in assemblies)
            {
                Type[] types;
                try { types = asm.GetTypes(); } catch { continue; }
                foreach (var t in types)
                {
                    if (t.IsAbstract || t.IsInterface) continue;
                    if (!iface.IsAssignableFrom(t)) continue;

                    var ctor = t.GetConstructor(Type.EmptyTypes);
                    if (ctor == null) continue;
                    return t;
                }
            }
            return null;
        }


        private object CreateInstance(Type implType)
        {
            if (implType == null)
                throw new ArgumentNullException(nameof(implType));
            // TODO: 提供更丰富的构造方式
            var ctor = implType.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                throw new InvalidOperationException($"Type {implType.FullName} has no public parameterless constructor. RegisterInstance required.");

            return Activator.CreateInstance(implType);
        }
    }
}
