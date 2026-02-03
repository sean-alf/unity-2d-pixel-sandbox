using UnityEngine.InputSystem;

public static class InputSystemActionsNames
{
	public static class PlayerMap
	{
		public static readonly string Name = "Player";

		public static readonly string Move = "Move";

		public static InputAction GetMoveAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Move);
		}

		public static readonly string Attack = "Attack";

		public static InputAction GetAttackAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Attack);
		}

		public static readonly string Interact = "Interact";

		public static InputAction GetInteractAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Interact);
		}

		public static readonly string Aim = "Aim";

		public static InputAction GetAimAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Aim);
		}

		public static readonly string Shoot = "Shoot";

		public static InputAction GetShootAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Shoot);
		}

		public static readonly string Previous = "Previous";

		public static InputAction GetPreviousAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Previous);
		}

		public static readonly string Next = "Next";

		public static InputAction GetNextAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Next);
		}

	}
	public static class UIMap
	{
		public static readonly string Name = "UI";

		public static readonly string Navigate = "Navigate";

		public static InputAction GetNavigateAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Navigate);
		}

		public static readonly string Submit = "Submit";

		public static InputAction GetSubmitAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Submit);
		}

		public static readonly string Cancel = "Cancel";

		public static InputAction GetCancelAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Cancel);
		}

		public static readonly string Point = "Point";

		public static InputAction GetPointAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Point);
		}

		public static readonly string Click = "Click";

		public static InputAction GetClickAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(Click);
		}

		public static readonly string RightClick = "RightClick";

		public static InputAction GetRightClickAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(RightClick);
		}

		public static readonly string MiddleClick = "MiddleClick";

		public static InputAction GetMiddleClickAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(MiddleClick);
		}

		public static readonly string ScrollWheel = "ScrollWheel";

		public static InputAction GetScrollWheelAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(ScrollWheel);
		}

		public static readonly string TrackedDevicePosition = "TrackedDevicePosition";

		public static InputAction GetTrackedDevicePositionAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(TrackedDevicePosition);
		}

		public static readonly string TrackedDeviceOrientation = "TrackedDeviceOrientation";

		public static InputAction GetTrackedDeviceOrientationAction(PlayerInput input)
		{
			return input.actions.FindActionMap(Name).FindAction(TrackedDeviceOrientation);
		}

	}
}
