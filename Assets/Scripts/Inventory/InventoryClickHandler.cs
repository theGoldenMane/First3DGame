using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryClickHandler : MonoBehaviour
{
	Inventory inventory;

	public void Start() {
		inventory = Inventory.instance;
	}

	public void Update() {
		if (Input.GetKey(KeyBindings.instance.inventorySpecialAction) && Input.GetKeyDown(KeyBindings.instance.splitStackHalf) && EventSystem.current.currentSelectedGameObject != null) {
			inventory.SplitStack(EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex());
		}
	}
}
