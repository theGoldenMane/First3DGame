using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
	public static Inventory instance;

	public delegate void OnItemChange();
	public OnItemChange onItemChangedCallback;

	public Item[] items;
	public int[] amounts;

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
		items = new Item[25];
		amounts = new int[25];
	}

	public int Add (Item item, int amount) {
		int returnValue = 0;
		bool inventoryChanged = true;

		// Check if item already exists in inventory
		int itemExistsIndex = ItemAlreadyInInventory(item);
		if (itemExistsIndex > -1) {
			int newAmount = amount + amounts[itemExistsIndex];
			if (newAmount > item.maxStackSize) {
				// Check if inventoy has empty slots
				int indexOfEmpty = GetFirstEmptySlot();
				if (indexOfEmpty > -1) {
					// Max stack size exceeded & space in inventory -> new additional stack
					amounts[itemExistsIndex] = item.maxStackSize;
					items[indexOfEmpty] = item;
					amounts[indexOfEmpty] = newAmount - item.maxStackSize;
				} else {
					// Max stack size exceeded & inventory full -> fill stack unil max amount and leave rest
					amounts[itemExistsIndex] = item.maxStackSize;
					returnValue = newAmount - item.maxStackSize;
					Debug.LogWarning("Can't pick up all, not enough space in Inventory");
				}
			} else {
				// Combine stacks
				amounts[itemExistsIndex] = amount + amounts[itemExistsIndex];
			}
		} else {
			// Check if inventoy has empty slots
			int indexOfEmpty = GetFirstEmptySlot();
			if (indexOfEmpty > -1) {
				// New stack
				items[indexOfEmpty] = item;
				amounts[indexOfEmpty] = amount;
			} else {
				inventoryChanged = false;
				returnValue = amount;
				Debug.LogWarning("Can't pick up -> no space in Inventory");
			}
		}

		// Fire GUI update event if inventory items/amount changed
		if (inventoryChanged && onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}

		return returnValue;
	}

	public void Drop (int itemIndex) {
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		GameObject prefab = items[itemIndex].prefab;

		Vector3 spawnPos = player.transform.position + player.transform.forward * 2f;
		Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit, 5f, (1 << 9));
		spawnPos.y = hit.point.y + items[itemIndex].prefab.transform.localScale.y / 2 + 0.1f;
		Instantiate(prefab, spawnPos, Quaternion.identity);

		items[itemIndex] = null;
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	public void Destroy (int itemIndex) {
		items[itemIndex] = null;
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	public void Move(int oldIndex, int newIndex) {
		items[newIndex] = items[oldIndex];
		items[oldIndex] = null;
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	public void Swap(int swapIndexA, int swapIndexB) {
		Item tmp = items[swapIndexA];
		items[swapIndexA] = items[swapIndexB];
		items[swapIndexB] = tmp;
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	private int GetFirstEmptySlot() {
		int indexOfEmpty = -1;
		for (int i = 0; i < items.Length; i++) {
			if (items[i] == null) {
				indexOfEmpty = i;
				break;
			}
		}
		return indexOfEmpty;
	}

	private int ItemAlreadyInInventory(Item item) {
		for (int i = 0; i < items.Length; i++) {
			if (items[i] == item) {
				return i;
			}
		}
		return -1;
	}
}
