using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HotkeyAttribute: Attribute
{
	public KeyCode Key { get; private set; }
	public string Description { get; private set; }

	public HotkeyAttribute(KeyCode key, string description = "")
	{
		Key = key;
		Description = description;
	}
}
