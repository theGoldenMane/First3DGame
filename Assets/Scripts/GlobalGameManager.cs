using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
	public static GlobalGameManager instance;
	public Camera currentCamera;

	[Header("Menus")]
	public bool inMenu = false;

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

	void Update () {
		if (inMenu) {
			Cursor.lockState = CursorLockMode.None;
		} else if (!inMenu) {
			Cursor.lockState = CursorLockMode.Locked;
		}
	}
}
