using Toolkit.Adapter.Physics;
using UnityEngine;

namespace Toolkit.Movement
{
    public interface IMovement2D
    {
        void Initalze(MovementProfileSO profile, IPhysicsAdapter phy);
        void UpdateDriver(float deltaTime);
        void FixedUpdateDriver(float fixedDeltaTime);
        void SetEnabled(bool enabled);
        void ApplyInput(float input);
    }
}