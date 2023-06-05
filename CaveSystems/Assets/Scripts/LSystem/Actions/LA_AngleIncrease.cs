public class LA_AngleIncrease : L_Action
{
    public LA_AngleIncrease()
    {
        character = 'O';
    }

    /// <summary>
    /// Increases the angle the turtle rotates by 10
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        system.angle += 10f;
    }
}