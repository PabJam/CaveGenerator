public class LA_AngleDecrease : L_Action
{
    public LA_AngleDecrease()
    {
        character = 'A';
    }

    /// <summary>
    /// Decreases the angle the turtle rotates by 10
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        system.angle -= 10f;
    }
}