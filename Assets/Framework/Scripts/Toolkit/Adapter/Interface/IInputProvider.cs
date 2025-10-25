using UnityEngine;

namespace Framework.Toolkit.Adapter.Input
{
    public interface IInputProvider
    {
        Vector2 Get2DInput(string name);
        float GetAxis(string name);
        bool ButtonDown(string name);
        bool ButtonUp(string name);
        bool ButtonHeld(string name);
    }
}