using System;
using System.Collections.Generic;

namespace Framework.Core.Service
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly Dictionary<Type, object> _services = new();

        public void RegisterSingleton<T>(T instance) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var t = typeof(T);
            if (_services.ContainsKey(t))
            {
                throw new InvalidOperationException($"Service {t.FullName} already registered");
            }
            _services[t] = instance;
        }

        public void ReplaceSingleton<T>(T instance) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _services[typeof(T)] = instance;
        }

        public T Resolve<T>() where T : class
        {
            if (!_services.TryGetValue(typeof(T), out var obj))
            {
                throw new InvalidOperationException($"Service {typeof(T).FullName} not found");
            }

            return (T)obj;
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

        public bool Has<T>(T service) where T : class => _services.ContainsKey(typeof(T));

        public void Clear()
        {
            _services.Clear();
        }
    }
}
