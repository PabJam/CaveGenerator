using System.Collections.Generic;
using UnityEngine;

public class LA_LoadPos : L_Action
{
    public LA_LoadPos()
    {
        character = 'L';
    }

    /// <summary>
    /// Sets the turtle to the last saved position and creates new branch of positions
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        system.turtle.position = system.savePos;
        system.timesLoaded++;
        system.positions.Add(new List<Vector3>());
        system.positions[system.timesLoaded].Add(system.turtle.transform.position);
    }
}