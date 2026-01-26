using UnityEngine;

/// <summary>
/// Base class for lightweight entity components that register with an Entity root.
/// </summary>
public abstract class EntityComponent : MonoBehaviour
{
    public Entity Entity { get; private set; }

    protected virtual void Awake()
    {
        Entity = GetComponentInParent<Entity>();
        if (Entity != null)
        {
            Entity.Register(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (Entity != null)
        {
            Entity.Unregister(this);
        }
    }
}
