public class L_Action
{
    // Character the action is refferenced by
    public char character;

    /// <summary>
    /// how the action is executed
    /// </summary>
    /// <param name="system"></param>
    public virtual void ExecuteAction(L_System system)
    {
    }
}