using Framework.Core.Interface;
using Framework.Core.Implementations;
using UnityEngine;

namespace Framework.Core.Infra
{
    // 启动 service locator 和 event system
    [DisallowMultipleComponent]
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