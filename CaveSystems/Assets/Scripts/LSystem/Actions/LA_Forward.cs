using UnityEngine;

public class LA_Forward : L_Action
{
    public LA_Forward()
    {
        character = 'F';
    }

    /// <summary>
    /// Moves the turtle forward and adds its position to the list of positions
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        system.turtle.Translate(system.turtle.forward * system.compression, Space.World);
        system.positions[system.timesLoaded].Add(system.turtle.transform.position);
    }
}