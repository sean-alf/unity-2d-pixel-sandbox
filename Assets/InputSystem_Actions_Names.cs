using UnityEngine.InputSystem;

public static class InputSystem_Actions_Names
{
	public static class Player
	{
		public static readonly string MAP_NAME = "Player";

		public static readonly string MOVE = "Move";

		public static InputAction Move(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(MOVE);
		}

		public static readonly string LOOK = "Look";

		public static InputAction Look(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(LOOK);
		}

		public static readonly string ATTACK = "Attack";

		public static InputAction Attack(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(ATTACK);
		}

		public static readonly string INTERACT = "Interact";

		public static InputAction Interact(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(INTERACT);
		}

		public static readonly string CROUCH = "Crouch";

		public static InputAction Crouch(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(CROUCH);
		}

		public static readonly string JUMP = "Jump";

		public static InputAction Jump(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(JUMP);
		}

		public static readonly string PREVIOUS = "Previous";

		public static InputAction Previous(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(PREVIOUS);
		}

		public static readonly string NEXT = "Next";

		public static InputAction Next(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(NEXT);
		}

		public static readonly string SPRINT = "Sprint";

		public static InputAction Sprint(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(SPRINT);
		}

	}
	public static class UI
	{
		public static readonly string MAP_NAME = "UI";

		public static readonly string NAVIGATE = "Navigate";

		public static InputAction Navigate(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(NAVIGATE);
		}

		public static readonly string SUBMIT = "Submit";

		public static InputAction Submit(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(SUBMIT);
		}

		public static readonly string CANCEL = "Cancel";

		public static InputAction Cancel(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(CANCEL);
		}

		public static readonly string POINT = "Point";

		public static InputAction Point(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(POINT);
		}

		public static readonly string CLICK = "Click";

		public static InputAction Click(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(CLICK);
		}

		public static readonly string RIGHT_CLICK = "RightClick";

		public static InputAction RightClick(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(RIGHT_CLICK);
		}

		public static readonly string MIDDLE_CLICK = "MiddleClick";

		public static InputAction MiddleClick(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(MIDDLE_CLICK);
		}

		public static readonly string SCROLL_WHEEL = "ScrollWheel";

		public static InputAction ScrollWheel(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(SCROLL_WHEEL);
		}

		public static readonly string TRACKED_DEVICE_POSITION = "TrackedDevicePosition";

		public static InputAction TrackedDevicePosition(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(TRACKED_DEVICE_POSITION);
		}

		public static readonly string TRACKED_DEVICE_ORIENTATION = "TrackedDeviceOrientation";

		public static InputAction TrackedDeviceOrientation(PlayerInput input)
		{
			return input.actions.FindActionMap(MAP_NAME).FindAction(TRACKED_DEVICE_ORIENTATION);
		}

	}
}
