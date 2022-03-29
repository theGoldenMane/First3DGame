using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
	Inventory inventory;
	InventorySlot[] slots;
	public Transform slotHolder;

	void Start()
	{
		inventory = Inventory.instance;
		inventory.onItemChangedCallback += UpdateUI;
		slots = slotHolder.GetComponentsInChildren<InventorySlot>();
	}

	void UpdateUI() {
		for (int i = 0; i < slots.Length; i++) {
			if (inventory.items[i] != null) {
				slots[i].AddItem(inventory.items[i], inventory.amounts[i]);
			} else {
				slots[i].ClearSlot();
			}
		}
	}
}
