using UnityEngine.InputSystem;

public static class ControlUtil
{
    public static InputAction FindProjectAction(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return null;
        }

        if (InputSystem.actions == null)
        {
            return null;
        }

        return InputSystem.actions.FindAction(actionName, throwIfNotFound: false);
    }
}