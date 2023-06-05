using UnityEngine;

public class LA_RotateCW : L_Action
{
    public LA_RotateCW()
    {
        character = '+';
    }

    /// <summary>
    /// Rotates the turtle clock wise
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        system.turtle.Rotate(Vector3.up * system.angle, Space.World);
    }
}