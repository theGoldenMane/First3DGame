using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryStorageClickHandler : MonoBehaviour
{
	Inventory inventory;
	private Storage storage;

	public void Start() {
		inventory = Inventory.instance;
	}

	public void Update() {
		if (GlobalGameManager.instance.inMenu) {
			// Split stack in half
			if (Input.GetKey(KeyBindings.instance.inventorySplitAction) && Input.GetButtonDown(KeyBindings.instance.splitStackHalf)) {
				GameObject clickedItem = CheckClickOnNonEmptySlot();
				if (clickedItem != null) {
					if (ActionFromStorage(clickedItem)) {
						storage.SplitStack(clickedItem.transform.GetSiblingIndex());
					} else {
						inventory.SplitStack(clickedItem.transform.GetSiblingIndex());
					}
				}
			}

			//Split one item from stack
			if (Input.GetKey(KeyBindings.instance.inventorySplitAction) && Input.GetButtonDown(KeyBindings.instance.splitOneItemFromStack)) {
				GameObject clickedItem = CheckClickOnNonEmptySlot();
				if (clickedItem != null) {
					if (ActionFromStorage(clickedItem)) {
						storage.SplitOneFromStack(clickedItem.transform.GetSiblingIndex());
					} else {
						inventory.SplitOneFromStack(clickedItem.transform.GetSiblingIndex());
					}
				}
			}

			// Move item stack
			if (Input.GetKey(KeyBindings.instance.inventoryMoveAction) && Input.GetButtonDown(KeyBindings.instance.moveStack)) {
				GameObject clickedItem = CheckClickOnNonEmptySlot();
				if (clickedItem != null) {
					if (ActionFromStorage(clickedItem)) {
						int currentIndex = clickedItem.transform.GetSiblingIndex();
						storage.MoveStackToInventory(storage.items[currentIndex], storage.amounts[currentIndex], currentIndex);
					} else {
						int currentIndex = clickedItem.transform.GetSiblingIndex();
						inventory.MoveStackToStorage(inventory.currentOpenStorage.gameObject, inventory.items[currentIndex], inventory.amounts[currentIndex], currentIndex);
					}
				}
			}

			// Move one item from stack
			if (Input.GetKey(KeyBindings.instance.inventoryMoveAction) && Input.GetButtonDown(KeyBindings.instance.moveOneItemFromStack)) {
				GameObject clickedItem = CheckClickOnNonEmptySlot();
				if (clickedItem != null) {
					if (ActionFromStorage(clickedItem)) {
						int currentIndex = clickedItem.transform.GetSiblingIndex();
						storage.MoveOneFromStackToInventory(storage.items[currentIndex], currentIndex);
					} else {
						int currentIndex = clickedItem.transform.GetSiblingIndex();
						inventory.MoveOneFromStackToStorage(inventory.currentOpenStorage.gameObject, inventory.items[currentIndex], currentIndex);
					}
				}
			}
		}
	}

	// Check if source of action is inventory or storage
	private bool ActionFromStorage(GameObject clickedItem) {
		if (clickedItem.transform.parent.gameObject.tag == "Storage") {
			storage = clickedItem.transform.parent.gameObject.GetComponent<Storage>();
			return true;
		}

		return false;
	}

	// Check if click was on slot with item
	private GameObject CheckClickOnNonEmptySlot() {
		PointerEventData pointer = new PointerEventData(EventSystem.current);
		pointer.position = Input.mousePosition;

		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, raycastResults);

		if (raycastResults.Count > 0) {
			if (raycastResults[0].gameObject.name == "Icon") {
				return raycastResults[0].gameObject.transform.parent.transform.parent.gameObject;
			}
		}

		return null;
	}
}
