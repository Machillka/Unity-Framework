using UnityEngine;

namespace Framework.Toolkit.Adapter.Physics
{
    public class Rigidbody2DAdapter : IPhysicsAdapter
    {
        private Rigidbody2D _rb;

        public Vector2 Velocity
        {
            get => _rb.linearVelocity;
            set => _rb.linearVelocity = value;
        }

        public float VelocityX
        {
            get => _rb.linearVelocityX;
            set => _rb.linearVelocityX = value;
        }

        public float VelocityY
        {
            get => _rb.linearVelocityY;
            set => _rb.linearVelocityY = value;
        }
    }
}