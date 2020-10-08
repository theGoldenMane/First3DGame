using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBindings : MonoBehaviour
{

	public static KeyBindings instance;

	public KeyCode openStorage;
	public KeyCode interactWithInventory;
	public KeyCode inventorySplitAction;
	public string splitStackHalf;
	public string splitOneItemFromStack;
	public KeyCode inventoryMoveAction;
	public string moveStack;
	public string moveOneItemFromStack;
	public KeyCode switchCameras;
	public KeyCode sprint;
	public KeyCode crouch;
	public KeyCode jump;
	public KeyCode glider;
	public KeyCode throwHook;


	void Awake ()
	{
		if (instance == null)
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
		else if (instance != this)
		{
			Destroy (gameObject);
		}
	}

	void Start() {
		// Open storage
		openStorage = KeyCode.E;

		// Open/Close inventory
		interactWithInventory = KeyCode.Tab;

		// Split item stack in half
		inventorySplitAction = KeyCode.LeftControl;
		splitStackHalf = "Fire1";

		// Split one item from stack
		inventorySplitAction = KeyCode.LeftControl;
		splitOneItemFromStack = "Fire2";

		// Move stack (Inventory/Storage)
		inventoryMoveAction = KeyCode.LeftShift;
		moveStack = "Fire1";

		// Move one item from stack (Inventory/Storage)
		inventoryMoveAction = KeyCode.LeftShift;
		moveOneItemFromStack = "Fire2";

		// Switch between first and third person camera
		switchCameras = KeyCode.V;

		// Hold to sprint
		sprint = KeyCode.LeftShift;

		// Hold to crouch
		crouch = KeyCode.LeftControl;

		// Press to jump
		jump = KeyCode.Space;

		// Opens glider
		glider = KeyCode.Q;

		// Throw grappling hook
		throwHook = KeyCode.F;
	}
}
