using System;

// TODO may need to modify as these get fired off constantly from Update()
// TODO make new zombie movement inherit
public abstract class MovementBase : EntityComponent
{
    public event Action OnWalk;
    public event Action OnRun;
    public event Action OnJump; // not constant
    public event Action OnLand; // not constant
    public event Action OnCrouch; // not used

    protected void NotifyWalk()
    {
        OnWalk?.Invoke();
    }

    protected void NotifyRun()
    {
        OnRun?.Invoke();
    }

    protected void NotifyJump()
    {
        OnJump?.Invoke();
    }

    protected void NotifyLand()
    {
        OnLand?.Invoke();
    }

    protected void NotifyCrouch()
    {
        OnCrouch?.Invoke();
    }
}