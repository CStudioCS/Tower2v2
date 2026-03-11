using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using InputDevice = UnityEngine.InputSystem.InputDevice;

public class ThrowOnTowerGif: InputTestFixture
{
	public override void Setup()
	{
		base.Setup();

		InputSystem.RegisterLayout<MyDevice>();
	}

	[Test]
	public void CanCreateMyDevice()
	{
		InputSystem.AddDevice<MyDevice>();
		Assert.That(InputSystem.devices, Has.Exactly(1).TypeOf<MyDevice>());
	}
	
	[Test]
	public void SimulateLeftArrowForOneSecond()
	{
		//TODO
	}
}

[InputControlLayout(displayName = "My Device")]
public class MyDevice: InputDevice
{
	// Minimal device implementation
}
