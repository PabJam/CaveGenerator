public class LA_SavePos : L_Action
{
    public LA_SavePos()
    {
        character = 'S';
    }

    /// <summary>
    /// saves the current position of the turtle
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        system.savePos = system.turtle.transform.position;
    }
}