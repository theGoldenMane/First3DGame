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
		// Split stack in half
		if (Input.GetKey(KeyBindings.instance.inventorySplitAction) && Input.GetKeyDown(KeyCode.Mouse0) && EventSystem.current.currentSelectedGameObject != null) {
			if (ActionFromStorage(EventSystem.current.currentSelectedGameObject)) {
				storage.SplitStack(EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex());
			} else {
				inventory.SplitStack(EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex());
			}
		}

		//Split one item from stack
		if (Input.GetKey(KeyBindings.instance.inventorySplitAction) && Input.GetButtonDown(KeyBindings.instance.splitOneItemFromStack) && EventSystem.current.currentSelectedGameObject != null) {
			if (ActionFromStorage(EventSystem.current.currentSelectedGameObject)) {
				storage.SplitOneFromStack(EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex());
			} else {
				inventory.SplitOneFromStack(EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex());
			}
		}

		// Move item stack
		if (Input.GetKey(KeyBindings.instance.inventoryMoveAction) && Input.GetKeyDown(KeyCode.Mouse0) && EventSystem.current.currentSelectedGameObject != null) {
			if (ActionFromStorage(EventSystem.current.currentSelectedGameObject)) {
				int currentIndex = EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex();
				storage.MoveStackToInventory(storage.items[currentIndex], storage.amounts[currentIndex], currentIndex);
			} else {
				int currentIndex = EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex();
				inventory.MoveStackToStorage(inventory.currentOpenStorage.gameObject, inventory.items[currentIndex], inventory.amounts[currentIndex], currentIndex);
			}
		}

		// Move one item from stack
		if (Input.GetKey(KeyBindings.instance.inventoryMoveAction) && Input.GetButtonDown(KeyBindings.instance.moveOneItemFromStack) && EventSystem.current.currentSelectedGameObject != null) {
			if (ActionFromStorage(EventSystem.current.currentSelectedGameObject)) {
				int currentIndex = EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex();
				storage.MoveOneFromStackToInventory(storage.items[currentIndex], currentIndex);
			} else {
				int currentIndex = EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex();
				inventory.MoveOneFromStackToStorage(inventory.currentOpenStorage.gameObject, inventory.items[currentIndex], currentIndex);
			}
		}
	}

	// Check if source of action is inventory or storage
	private bool ActionFromStorage(GameObject clickedItem) {
		Transform slotHolder;
		if (clickedItem.name == "Button") {
			slotHolder = clickedItem.transform.parent.transform.parent;
		} else {
			slotHolder = clickedItem.transform.parent.transform.parent.transform.parent;
		}

		if (slotHolder.tag == "Storage") {
			storage = slotHolder.GetComponent<Storage>();
			return true;
		}

		return false;
	}
}
