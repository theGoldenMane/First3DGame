using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
	public static GlobalGameManager instance;
	public Camera currentCamera;

	[Header("Menus")]
	public bool inInventory = false;
	public GameObject inventory;

	private State state;
	private enum State {
		InGame,
		Inventory
	}

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
		state = State.InGame;
	}

	void Update() {
		switch (state) {
		default:
		case State.InGame:
			OpenInventory();
			break;
		case State.Inventory:
			CloseInventory();
			break;
		}
	}

	void OpenInventory() {
		if (Input.GetKeyDown(KeyCode.Tab)) {
			inInventory = true;
			Cursor.lockState = CursorLockMode.None;
			inventory.gameObject.active = true;
			state = State.Inventory;
		}
	}

	void CloseInventory() {
		if (Input.GetKeyDown(KeyCode.Tab)) {
			inInventory = false;
			Cursor.lockState = CursorLockMode.Locked;
			inventory.gameObject.active = false;
			state = State.InGame;
		}
	}

}
