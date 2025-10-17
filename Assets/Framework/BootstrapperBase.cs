using Framework.Core.EventSystem;
using Framework.Core.Service;
using UnityEngine;

namespace Framework.Core.Infra
{
    // 启动 service locator 和 event system
    [DisallowMultipleComponent]
    // BUG: 重构成一个统一的入口
    public class BootstrapperBase : MonoBehaviour
    {
        public ServiceLocator ServiceManager { get; private set; }
        private protected virtual void Awake()
        {
            ServiceManager = new ServiceLocator();
            // register itself
            ServiceManager.RegisterSingleton<IServiceLocator>(ServiceManager);
            ServiceManager.RegisterSingleton<IEventBus>(new EventBus());

            Debug.Log("[Bootstrapper] Core services registered: IServiceRegistry, IMessageBus");
        }

        private void OnDestroy()
        {
            ServiceManager?.Clear();
        }
    }
}