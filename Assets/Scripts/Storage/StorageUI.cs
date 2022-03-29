using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageUI : MonoBehaviour
{
    public Storage storage;
	public Transform slotHolder;
	StorageSlot[] slots;

	void Start()
	{
		storage.onStorageItemChangedCallback += UpdateUI;
		slots = slotHolder.GetComponentsInChildren<StorageSlot>();
	}

	void UpdateUI() {
		for (int i = 0; i < slots.Length; i++) {
			if (storage.items[i] != null) {
				slots[i].AddItem(storage.items[i], storage.amounts[i]);
			} else {
				slots[i].ClearSlot();
			}
		}
	}
}
