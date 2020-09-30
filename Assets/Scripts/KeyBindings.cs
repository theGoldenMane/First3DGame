using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBindings : MonoBehaviour
{

	public static KeyBindings instance;

	public KeyCode openStorage;
	public KeyCode interactWithInventory;
	public KeyCode inventorySpecialAction;
	public KeyCode splitStackHalf;
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
		inventorySpecialAction = KeyCode.LeftControl;
		splitStackHalf = KeyCode.Mouse0;

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
