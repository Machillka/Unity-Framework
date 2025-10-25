using Framework.Toolkit.Adapter.Physics;
using UnityEngine;

namespace Framework.Toolkit.Movement
{
    public class Movement2D : IMovement2D
    {
        private float _input;
        private MovementProfileSO _profile;
        private bool _enabled;
        private IPhysicsAdapter _phy;

        public void ApplyInput(float input) => _input = input;

        public void Initalze(MovementProfileSO profile, IPhysicsAdapter phy)
        {
            _profile = profile;
            _phy = phy;
        }

        // FIXED: 使用一种更聪明的实现方式
        public void SetEnabled(bool enabled) => _enabled = enabled;

        public void UpdateDriver(float deltaTime)
        {
            if (!_enabled)
                return;
        }

        public void FixedUpdateDriver(float fixedDeltaTime)
        {
            if (!_enabled)
                return;

            float targetSpeed = _input * _profile.maxSpeed;
            float deltaSpeed = targetSpeed - _phy.VelocityX;
            float accelerationRate = Mathf.Abs(targetSpeed) > 0.01f ? _profile.acceleration : _profile.deceleration;
            float movemntSpeed = deltaSpeed * accelerationRate * Time.fixedDeltaTime;

            _phy.VelocityX += movemntSpeed;
        }
    }
}
