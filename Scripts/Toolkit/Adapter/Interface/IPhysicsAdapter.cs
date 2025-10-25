using UnityEngine;

namespace Framework.Toolkit.Adapter.Physics
{
    public interface IPhysicsAdapter
    {
        public Vector2 Velocity { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
    }
}