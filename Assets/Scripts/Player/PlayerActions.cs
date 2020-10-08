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
		if (Input.GetKeyDown(KeyBindings.instance.openStorage)) {
			Ray ray = GlobalGameManager.instance.currentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
			bool raycastHits = Physics.Raycast(ray, out RaycastHit hit, pickUpDistance);
			if (raycastHits) {
				if (hit.transform.tag == "Pick-Up") {
					hit.transform.gameObject.GetComponent<ItemPickup>().PickUp();
				} else if (hit.transform.tag == "Storage" && !storageOpen) {
					currentOpenStorage = hit.transform.gameObject.GetComponent<StorageController>();
					Inventory.instance.currentOpenStorage = currentOpenStorage.storageInventory.transform.GetChild(0).GetChild(0).GetComponent<Storage>();
					currentOpenStorage.OpenStorage();
					OpenInventory();
					storageOpen = true;
				} else if (storageOpen) {
					Inventory.instance.currentOpenStorage = null;
					currentOpenStorage.CloseStorage();
					CloseInventory();
					storageOpen = false;
				}
			}
		} else if (Input.GetKeyDown(KeyBindings.instance.interactWithInventory)) {
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
