public class LA_StepIncrease : L_Action
{
    public LA_StepIncrease()
    {
        character = '>';
    }

    /// <summary>
    /// Increases the amount of how much the turtle moves
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        if (system.compression <= 2 * system.baseCompression)
        {
            system.compression *= 1.05f;
        }
    }
}