using UnityEngine;

public class LA_PitchUp : L_Action
{
    public LA_PitchUp()
    {
        character = 'U';
    }

    /// <summary>
    /// Rotates the turtle up
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        system.turtle.Rotate(Vector3.right * system.angle, Space.World);
    }
}