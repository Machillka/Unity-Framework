using Framework.Profile.Scriptable;
using UnityEngine;

namespace Framework.Toolkit.Movement
{
    /// <summary>
    /// Movement-related parameters for a player or entity.
    /// Designed to be simple and designer-friendly.
    /// </summary>
    [CreateAssetMenu(fileName = "MovementProfile", menuName = "Framework/Profiles/MovementProfile")]
    public class MovementProfileSO : BaseProfileSO
    {
        [Header("Movement Parameters")]

        public float maxSpeed = 6f;
        public float acceleration = 30f;
        public float deceleration = 40f;

        public override string Validate()
        {
            var baseErr = base.Validate();
            if (!string.IsNullOrEmpty(baseErr)) return baseErr;
            if (maxSpeed <= 0f) return "maxSpeed must be > 0";
            if (acceleration <= 0f) return "acceleration must be > 0";
            return null;
        }
    }
}
