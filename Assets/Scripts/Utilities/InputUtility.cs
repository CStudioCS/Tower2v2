using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class InputUtility
{
	public static bool AnyInputPressed
	{
		get
		{
			if (Input.anyKeyDown)
				return true;

			foreach (Gamepad gamepad in Gamepad.all)
			{
				if (gamepad.allControls.OfType<ButtonControl>().Any(button => button.wasPressedThisFrame))
					return true;
			}
			return false;
		}
	}
}