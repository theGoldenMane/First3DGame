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
	}

	public bool Add (Item item) {
		// Get first empty array slot
		int indexOfEmpty = -1;
		for (int i = 0; i < items.Length; i++) {
			if (items[i] == null) {
				indexOfEmpty = i;
				break;
			}
		}

		// If empty slot exists, add item to it
		if (indexOfEmpty > -1) {
			items[indexOfEmpty] = item;
			if (onItemChangedCallback != null) {
				onItemChangedCallback.Invoke();
			}
			return true;
		} else {
			Debug.LogWarning("No space in Inventory");
		}

		return false;
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
}
