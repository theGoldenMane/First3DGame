using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
	public static Inventory instance;
	public int inventorySize;

	public Storage currentOpenStorage;

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
		if (inventoryChanged && onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}

		return returnValue;
	}

	public int AddToIndex(Item item, int amount, int itemIndex) {
		bool inventoryChanged = true;

		if (items[itemIndex] == null) {
			// Add item to empty storage slot
			items[itemIndex] = item;
			amounts[itemIndex] = amount;
		} else {
			int newAmount = amount + amounts[itemIndex];
			if (newAmount > item.maxStackSize) {
				// Max stack size exceeded -> Fill all already existing stacks that are not exceeded
				List<int> stackableSlots = GetAllNotExceededSlotsWithSameItem(item);
				if (stackableSlots.Count > 0) {
					newAmount = amount;
					for (int i = 0; i < stackableSlots.Count; i++) {
						int slotNr = stackableSlots[i];
						if (newAmount == 0) {
							break;
						}

						if (newAmount + amounts[slotNr] > item.maxStackSize) {
							int diff = item.maxStackSize - amounts[slotNr];
							amounts[slotNr] = item.maxStackSize;
							newAmount -= diff;
						} else {
							amounts[slotNr] += newAmount;
							newAmount = 0;
						}
					}
				}

				// If there are no stacks with same item or if amount could not be distributed completly to other slots-> Check if empty slot is available
				if (stackableSlots.Count < 0 || newAmount > 0) {
					// Check if inventoy has empty slots
					int indexOfEmpty = GetFirstEmptySlot();
					if (indexOfEmpty > -1) {
						// Max stack size exceeded & space in inventory -> new additional stack
						items[indexOfEmpty] = item;
						amounts[indexOfEmpty] = newAmount;
					} else {
						// Max stack size exceeded & inventory full -> fill stack unil max amount and leave rest in Inventory
						return newAmount - item.maxStackSize;
						inventoryChanged = false;
						Debug.LogWarning("Can't pick up all, not enough space in Storage");
					}
				}
			} else {
				// Combine stacks
				amounts[itemIndex] = newAmount;
			}
		}

		if (inventoryChanged && onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}

		return 0;
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
		amounts[newIndex] = amounts[oldIndex];
		Destroy(oldIndex);
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
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
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
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
		if (inventoryChanged && onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	public void SplitOneFromStack(int itemIndex) {
		bool inventoryChanged = false;
		// Split stack if item exists in this slot and stack is greater 1
		int amount = amounts[itemIndex];
		if (items[itemIndex] != null && amounts[itemIndex] > 1) {
			// Check if there would be space for second stack
			int indexOfEmpty = GetFirstEmptySlot();
			if (indexOfEmpty > -1) {
				amounts[itemIndex] -= 1;
				items[indexOfEmpty] = items[itemIndex];
				amounts[indexOfEmpty] = 1;

				inventoryChanged = true;
			}
		}

		// Fire GUI update event if inventory items/amount changed
		if (inventoryChanged && onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	public void DeleteOneItemFromStack(int itemIndex) {
		amounts[itemIndex] -= 1;
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	public void AddToStorage(GameObject storage, Item item, int amount, int itemIndex, int deleteIndex) {
		int excessAmount = storage.GetComponent<Storage>().AddToIndex(item, amount, itemIndex);
		if (excessAmount == 0) {
			Destroy(deleteIndex);
		} else {
			amounts[deleteIndex] = excessAmount;
		}
	}

	public void MoveStackToStorage(GameObject storage, Item item, int amount, int deleteIndex) {
		int excessAmount = storage.GetComponent<Storage>().Add(item, amount);
		if (excessAmount == 0) {
			Destroy(deleteIndex);
		} else {
			amounts[deleteIndex] = excessAmount;
		}
	}

	public void MoveOneFromStackToStorage(GameObject storage, Item item, int itemIndex) {
		if (amounts[itemIndex] > 1) {
			int excessAmount = storage.GetComponent<Storage>().Add(item, 1);
			if (excessAmount == 0) {
				DeleteOneItemFromStack(itemIndex);
			} else {
				amounts[itemIndex] = excessAmount;
			}
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

	private List<int> GetAllNotExceededSlotsWithSameItem(Item item) {
		List<int> indexOfSame = new List<int>();
		if (items.Length > 0) {
			for (int i = 0; i < items.Length; i++) {
				if (items[i] == item && amounts[i] < item.maxStackSize) {
					indexOfSame.Add(i);
				}
			}
		}
		return indexOfSame;
	}

	private int ItemAlreadyInInventory(Item item) {
		if (items.Length > 0) {
			for (int i = 0; i < items.Length; i++) {
				if (items[i] == item && amounts[i] < item.maxStackSize) {
					return i;
				}
			}
		}
		return -1;
	}
}
