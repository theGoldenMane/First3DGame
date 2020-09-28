using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
	private int pickUpLayer = 1 << 12;
	public float pickUpDistance = 5f;
	private bool storageOpen = false;
	private bool inventoryOpen = false;
	private StorageController currentOpenStorage;
	public GameObject playerInventory;


	void Update()
	{
		if (Input.GetKeyDown(KeyCode.E)) {
			Ray ray = GlobalGameManager.instance.currentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
			bool raycastHits = Physics.Raycast(ray, out RaycastHit hit, pickUpDistance);
			if (raycastHits) {
				if (hit.transform.tag == "Pick-Up") {
					hit.transform.gameObject.GetComponent<ItemPickup>().PickUp();
				} else if (hit.transform.tag == "Storage" && !storageOpen) {
					currentOpenStorage = hit.transform.gameObject.GetComponent<StorageController>();
					currentOpenStorage.OpenStorage();
					OpenInventory();
					storageOpen = true;
				} else if (storageOpen) {
					currentOpenStorage.CloseStorage();
					CloseInventory();
					storageOpen = false;
				}
			}
		} else if (Input.GetKeyDown(KeyCode.Tab)) {
			if (!inventoryOpen) {
				OpenInventory();
				inventoryOpen = true;
			} else {
				CloseInventory();
				inventoryOpen = false;
			}
		}
	}

	public void OpenInventory() {
		GlobalGameManager.instance.inMenu = true;
		playerInventory.active = true;
	}

	public void CloseInventory() {
		GlobalGameManager.instance.inMenu = false;
		playerInventory.active = false;
	}
}
