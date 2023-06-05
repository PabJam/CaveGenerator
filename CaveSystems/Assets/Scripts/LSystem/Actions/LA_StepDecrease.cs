public class LA_StepDecrease : L_Action
{
    public LA_StepDecrease()
    {
        character = '<';
    }

    /// <summary>
    /// Decreases the amount of how much the turtle moves
    /// </summary>
    /// <param name="system"></param>
    public override void ExecuteAction(L_System system)
    {
        if (system.compression >= 0.5 * system.baseCompression)
        {
            system.compression *= 0.95f;
        }
    }
}