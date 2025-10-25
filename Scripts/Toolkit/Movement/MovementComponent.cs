using Framework.Core.Infra;
using Framework.Toolkit.Adapter.Input;
using Framework.Toolkit.Adapter.Physics;
using UnityEngine;

namespace Framework.Toolkit.Movement
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementComponent : MonoBehaviour
    {
        public MovementProfileSO movementProfile;

        private IMovement2D _movementModule;
        private IPhysicsAdapter _phy;
        private IInputProvider _inputController;
        private float _horizontalInput;

        private void Awake()
        {
            _phy = new Rigidbody2DAdapter();
            _movementModule.Initalze(movementProfile, _phy);
            var bootstrapper = FindAnyObjectByType<BootstrapperBase>();
            if (bootstrapper == null)
            {
                Debug.LogWarning("[MovementComponent] Bootstrapper not found; creating local default services.");
                bootstrapper = gameObject.AddComponent<BootstrapperBase>();
            }

            if (!bootstrapper.ServiceManager.TryReslolve<IInputProvider>(out _inputController))
            {
                _inputController = new InputSystemAdapter();
                bootstrapper.ServiceManager.RegisterSingleton<IInputProvider>(_inputController);
            }
        }

        private void Update()
        {
            // NOTE: 输入事件是否需要在这边执行？还是统一用一个Controller处理？
            _horizontalInput = _inputController.GetAxis("Horizontal");
            _movementModule.ApplyInput(_horizontalInput);
        }

        private void FixedUpdate()
        {
            _movementModule.FixedUpdateDriver(Time.fixedDeltaTime);
        }
    }
}

