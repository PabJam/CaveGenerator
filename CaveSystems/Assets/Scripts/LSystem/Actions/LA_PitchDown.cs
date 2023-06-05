using UnityEngine;

public class LA_PitchDown : L_Action
{
    public LA_PitchDown()
    {
        character = 'D';
    }

    /// <summary>
    /// Rotates the turtle down
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        system.turtle.Rotate(Vector3.left * system.angle, Space.World);
    }
}