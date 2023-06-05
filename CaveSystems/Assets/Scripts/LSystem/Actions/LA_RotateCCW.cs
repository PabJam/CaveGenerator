using UnityEngine;

public class LA_RotateCCW : L_Action
{
    public LA_RotateCCW()
    {
        character = '-';
    }

    /// <summary>
    /// Rotates the turtle counter clockwise
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        system.turtle.Rotate(Vector3.down * system.angle, Space.World);
    }
}