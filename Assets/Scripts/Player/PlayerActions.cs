using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
	private int pickUpLayer = 1 << 12;
	public float pickUpDistance = 5f;

	void Update()
	{
		DetectPickUp();
	}

	private void DetectPickUp() {
		Ray ray = GlobalGameManager.instance.currentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		if (Physics.Raycast(ray, out RaycastHit hit, pickUpDistance, pickUpLayer)) {
			if (Input.GetKeyDown(KeyCode.E)) {
				hit.transform.gameObject.GetComponent<ItemPickup>().PickUp();
			}
		}
	}
}
