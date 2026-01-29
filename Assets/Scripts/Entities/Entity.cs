using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight entity root that can host EntityComponents and provide type-safe lookup.
/// </summary>
public class Entity : MonoBehaviour
{
	private readonly Dictionary<Type, EntityComponent> components = new();

	public void Register(EntityComponent component)
	{
		if (component == null)
		{
			return;
		}

		components[component.GetType()] = component;
	}

	public void Unregister(EntityComponent component)
	{
		if (component == null)
		{
			return;
		}

		if (components.TryGetValue(component.GetType(), out var existing) && existing == component)
		{
			components.Remove(component.GetType());
		}
	}

	public T GetEntityComponent<T>() where T : EntityComponent
	{
		if (components.TryGetValue(typeof(T), out var component))
		{
			return component as T;
		}

		return default;
	}
}
