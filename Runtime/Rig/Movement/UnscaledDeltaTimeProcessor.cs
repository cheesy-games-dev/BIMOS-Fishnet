using UnityEngine;
using UnityEngine.InputSystem;

public class UnscaledDeltaTimeProcessor : InputProcessor<Vector2>
{
    public override Vector2 Process(Vector2 value, InputControl control) => value * Time.unscaledDeltaTime;
}