using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Storage : MonoBehaviour
{
	public int inventorySize;

	public delegate void OnStorageItemChange();
	public OnStorageItemChange onStorageItemChangedCallback;

	public Item[] items;
	public int[] amounts;

	void Start() {
		items = new Item[inventorySize];
		amounts = new int[inventorySize];
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
				amounts[itemExistsIndex] = newAmount;
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
		if (inventoryChanged && onStorageItemChangedCallback != null) {
			onStorageItemChangedCallback.Invoke();
		}

		return returnValue;
	}

	public void AddToIndex(Item item, int amount, int itemIndex) {
		if (items[itemIndex] == null) {
			items[itemIndex] = item;
			amounts[itemIndex] = amount;

		} else {
			int itemExistsIndex = ItemAlreadyInInventory(item);

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
					// Max stack size exceeded & inventory full -> fill stack unil max amount and leave rest in Inventory
					amounts[itemExistsIndex] = item.maxStackSize;
					//returnValue = newAmount - item.maxStackSize;
					Debug.LogWarning("Can't pick up all, not enough space in Inventory");
				}
			} else {
				// Combine stacks
				amounts[itemExistsIndex] = newAmount;
			}
		}

		if (onStorageItemChangedCallback != null) {
			onStorageItemChangedCallback.Invoke();
		}
	}

	public void Drop (int itemIndex) {
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		GameObject prefab = items[itemIndex].prefab;

		Vector3 spawnPos = player.transform.position + player.transform.forward * 2f;
		Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit, 5f, (1 << 9));
		spawnPos.y = hit.point.y + items[itemIndex].prefab.transform.localScale.y / 2 + 0.1f;
		GameObject dropedItem = Instantiate(prefab, spawnPos, Quaternion.identity);
		dropedItem.GetComponent<ItemPickup>().amount = amounts[itemIndex];
		dropedItem.tag = "Pick-Up";

		items[itemIndex] = null;
		if (onStorageItemChangedCallback != null) {
			onStorageItemChangedCallback.Invoke();
		}
	}

	public void Destroy (int itemIndex) {
		items[itemIndex] = null;
		if (onStorageItemChangedCallback != null) {
			onStorageItemChangedCallback.Invoke();
		}
	}

	public void Move(int oldIndex, int newIndex) {
		items[newIndex] = items[oldIndex];
		amounts[newIndex] = amounts[oldIndex];
		Destroy(oldIndex);
		if (onStorageItemChangedCallback != null) {
			onStorageItemChangedCallback.Invoke();
		}
	}

	public void Swap(int swapIndexA, int swapIndexB) {
		if (items[swapIndexA] == items[swapIndexB]) {
			Item item = items[swapIndexB];
			int newAmount = amounts[swapIndexA] + amounts[swapIndexB];
			if (newAmount > item.maxStackSize) {
				// Max stack size exceeded -> fill one until max
				amounts[swapIndexB] = item.maxStackSize;
				amounts[swapIndexA] = newAmount - item.maxStackSize;
			} else {
				// Combine stacks
				amounts[swapIndexB] = newAmount;
				Destroy(swapIndexA);
			}
		} else {
			Item tmp = items[swapIndexA];
			items[swapIndexA] = items[swapIndexB];
			items[swapIndexB] = tmp;
		}

		// Fire GUI update event
		if (onStorageItemChangedCallback != null) {
			onStorageItemChangedCallback.Invoke();
		}
	}

	public void SplitStack(int itemIndex) {
		bool inventoryChanged = false;
		// Split stack if item exists in this slot and stack is greater 1
		int amount = amounts[itemIndex];
		if (items[itemIndex] != null && amounts[itemIndex] > 1) {
			// Check if there would be space for second stack
			int indexOfEmpty = GetFirstEmptySlot();
			if (indexOfEmpty > -1) {
				int firstStack;
				int secondStack;
				// Check if stack has even amount
				if (amount % 2 != 0) {
					// Odd
					firstStack = amount / 2 + 1 ;
					secondStack = amount / 2;
				} else {
					// Even
					firstStack = amount / 2;
					secondStack = firstStack;
				}

				amounts[itemIndex] = firstStack;
				items[indexOfEmpty] = items[itemIndex];
				amounts[indexOfEmpty] = secondStack;

				inventoryChanged = true;
			}
		}

		// Fire GUI update event if inventory items/amount changed
		if (inventoryChanged && onStorageItemChangedCallback != null) {
			onStorageItemChangedCallback.Invoke();
		}
	}

	public void AddToInventory(Item item, int amount, int itemIndex, int deleteIndex) {
		Inventory.instance.AddToIndex(item, amount, itemIndex);
		Destroy(deleteIndex);
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
		Debug.Log(item);
		Debug.Log(items.Length);
		for (int i = 0; i < items.Length; i++) {
			if (items[i] == item && amounts[i] < item.maxStackSize) {
				return i;
			}
		}
		return -1;
	}
}
